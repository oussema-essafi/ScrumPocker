using System.Collections.Generic;

namespace ScrumPokerApp.Models
{
    public class Session
    {
        public string SessionId { get; set; }
        public List<Participant> Participants { get; set; } = new();
        public Dictionary<string, string> Votes { get; set; } = new();
        public bool IsRevealed { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
    }
}