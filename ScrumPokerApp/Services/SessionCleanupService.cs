using ScrumPokerApp.Hubs;

namespace ScrumPokerApp.Services
{
   public class SessionCleanupService : BackgroundService
{
    private readonly ILogger<SessionCleanupService> _logger;

    public SessionCleanupService(ILogger<SessionCleanupService> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                ScrumPokerHub.CleanupInactiveSessions();
                _logger.LogInformation("Session cleanup completed");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cleaning up sessions");
            }
            
            await Task.Delay(TimeSpan.FromMinutes(30), stoppingToken); // Run every 30 minutes
        }
    }
} 
}