using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using ScrumPokerApp.Models;

namespace ScrumPokerApp.Hubs
{
public class ScrumPokerHub : Hub
{
    private static readonly ConcurrentDictionary<string, Session> _sessions = new();
    private static readonly ConcurrentDictionary<string, DateTime> _joinAttempts = new();

    public async Task CreateSession(string userName)
    {
        if (string.IsNullOrWhiteSpace(userName) || userName.Length > 50)
        {
            await Clients.Caller.SendAsync("Error", "Name is required");
            return;
        }
        var sessionId = Guid.NewGuid().ToString("N").ToUpper();;
        var session = new Session { SessionId = sessionId };
        
        var participant = new Participant
        {
            ConnectionId = Context.ConnectionId,
            UserName = userName,
            IsHost = true
        };
        
        session.Participants.Add(participant);
        _sessions.TryAdd(sessionId, session);
        
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
        await Clients.Caller.SendAsync("SessionCreated", sessionId);
    }

    public async Task JoinSession(string sessionId, string userName)
    {
        if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(userName) || userName.Length > 50)
        {
            await Clients.Caller.SendAsync("Error", "Name is required");
            return;
        }
            // Rate limiting
        var ip = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString();
        
        if (ip != null && _joinAttempts.TryGetValue(ip, out var lastAttempt))
        {
            if (DateTime.UtcNow - lastAttempt < TimeSpan.FromSeconds(5))
            {
                await Clients.Caller.SendAsync("Error", "Too many attempts. Please wait 5 seconds.");
                return;
            }
        }
        
        _joinAttempts.AddOrUpdate(ip, DateTime.UtcNow, (_, _) => DateTime.UtcNow);

        // Validate session ID format
        if (!Guid.TryParseExact(sessionId, "N", out _))
        {
            await Clients.Caller.SendAsync("Error", "Invalid session ID format");
            return;
        }
        if (!_sessions.TryGetValue(sessionId, out var session))
        {
            await Clients.Caller.SendAsync("Error", "Session not found");
            return;
        }

            if (session.Participants.Any(p => p.UserName == userName))
            {
                await Clients.Caller.SendAsync("Error", "Username already taken");
                return;
            }

            var participant = new Participant
        {
            ConnectionId = Context.ConnectionId,
            UserName = userName,
            IsHost = false
        };
        
        session.Participants.Add(participant);
        await Groups.AddToGroupAsync(Context.ConnectionId, sessionId);
              // Notify user if they're host
        var isHost = session.Participants.Any(p => 
            p.ConnectionId == Context.ConnectionId && p.IsHost);
        
        await Clients.Caller.SendAsync("JoinedSession", session.Participants);
        await Clients.Group(sessionId).SendAsync("UserJoined", userName);
    }

    public async Task SubmitVote(string sessionId, string vote)
    {
            if (string.IsNullOrWhiteSpace(sessionId) || string.IsNullOrWhiteSpace(vote) || vote.Length > 10)
            {
                await Clients.Caller.SendAsync("Error", "Invalid vote");
                return;
            }
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                session.Votes[Context.ConnectionId] = vote;
                var participant = session.Participants.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
                if (participant != null)
                {
                    await Clients.Group(sessionId).SendAsync("VoteReceived", participant.UserName, vote);
                }
            }
        }

public async Task ResetVotes(string sessionId)
{
    if (!_sessions.TryGetValue(sessionId, out var session)) return;

    var participant = session.Participants.FirstOrDefault(p => 
        p.ConnectionId == Context.ConnectionId);
    
    if (participant == null || !participant.IsHost) return;

    session.Votes.Clear();
    session.IsRevealed = false;
    await Clients.Group(sessionId).SendAsync("VotesReset");
}
    public async Task RevealVotes(string sessionId)
    {
            if (_sessions.TryGetValue(sessionId, out var session))
            {
                var participant = session.Participants.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
                if (participant == null || !participant.IsHost)
                {
                    await Clients.Caller.SendAsync("Error", "Only the host can reveal votes");
                    return;
                }
                var votesWithNames = session.Votes.ToDictionary(
                    kv => session.Participants.FirstOrDefault(p => p.ConnectionId == kv.Key)?.UserName ?? kv.Key,
                    kv => kv.Value
                );
                session.IsRevealed = true;
                await Clients.Group(sessionId).SendAsync("VotesRevealed", votesWithNames);
            }

        }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Clean up disconnected users from sessions
        foreach (var session in _sessions.Values)
        {
            var participant = session.Participants.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);
            if (participant != null)
            {
                session.Participants.Remove(participant);
                await Clients.Group(session.SessionId).SendAsync("UserLeft", participant.UserName);
            }
        }
        await base.OnDisconnectedAsync(exception);
    }

    public static void CleanupInactiveSessions()
    {
        var cutoff = DateTime.UtcNow.AddHours(-24); // 24-hour inactivity timeout
        var expiredSessions = _sessions.Where(s => s.Value.LastActivity < cutoff).ToList();
        
        foreach (var session in expiredSessions)
        {
            _sessions.TryRemove(session.Key, out _);
        }
    }

    // Update all hub methods to refresh LastActivity
    private void UpdateSessionActivity(string sessionId)
    {
        if (_sessions.TryGetValue(sessionId, out var session))
        {
            session.LastActivity = DateTime.UtcNow;
        }
    }
}
}