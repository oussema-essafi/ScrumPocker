namespace ScrumPokerApp.Models
{
    public class Participant
    {
        public string ConnectionId { get; set; }
        public string UserName { get; set; }
        public bool IsHost { get; set; }
    }
}