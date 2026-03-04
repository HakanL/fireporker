using System.ComponentModel.DataAnnotations;

namespace FirePorker.Models;

public class PokerGame
{
    public Guid Id { get; set; }
    
    [Required(ErrorMessage = "Game name is required")]
    [Display(Name = "Game name")]
    public string Name { get; set; }
    
    [Display(Name = "Game description (optional)")]
    public string Description { get; set; }
    
    public Player Host { get; set; }
    public IList<Player> Players { get; set; }
    public DateTime ExpirationDate { get; set; }
    public IList<Story> Stories { get; set; }
    public DateTime RoundStarted { get; set; }

    // Computed properties for list view
    public Story? CurrentStory => Stories.FirstOrDefault(s => s.Estimate == null);
    
    public string Status
    {
        get
        {
            if (CurrentStory == null)
                return "Idle";
            
            var votedCount = Players.Count(p => p.CurrentVote != null);
            return votedCount > 0 ? "Voting" : "Waiting";
        }
    }
    
    public int VotedCount => Players.Count(p => p.CurrentVote != null);
    
    public string Progress
    {
        get
        {
            if (CurrentStory == null)
                return "—";
            return $"{VotedCount}/{Players.Count}";
        }
    }
    
    public string ExpiresIn
    {
        get
        {
            var remaining = ExpirationDate - DateTime.UtcNow;
            if (remaining.TotalMinutes < 1)
                return "soon";
            if (remaining.TotalHours < 1)
                return $"in {(int)remaining.TotalMinutes}m";
            if (remaining.TotalDays < 1)
                return $"in {(int)remaining.TotalHours}h";
            return $"in {(int)remaining.TotalDays}d";
        }
    }
    
    // Stable tracking key for activity view (not reversible to real ID)
    public string TrackingKey => Id.GetHashCode().ToString("x8");

    // TODO: Configurable number set?
    
    public PokerGame(string name, string hostName, string description = "", string id = "")
    {
        Id = string.IsNullOrEmpty(id) ? Guid.NewGuid() : Guid.Parse(id);
        Name = name;
        Description = description;
        Host = new Player(Id, hostName, true);
        Players = new List<Player> { Host };
        ExpirationDate = DateTime.UtcNow.AddDays(1);
        Stories = new List<Story>();
    }

    public Player AddPlayer(string name)
    {
        var player = new Player(Id, name);
        Players.Add(player);
        return player;
    }

    public Player? GetPlayer(Guid playerId)
    {
        return Players.FirstOrDefault(x => x.Id == playerId);
    }

    public void RemovePlayer(Guid playerId)
    {
        var player = Players.FirstOrDefault(x => x.Id == playerId);
        if (player != null)
            Players.Remove(player);
    }

    public Story AddStory(string title, string description)
    {
        var newStory = new Story(Id, title, description);
        Stories.Add(newStory);
        return newStory;
    }

    public Story? GetStory(Guid storyId)
    {
        return Stories.FirstOrDefault(x => x.Id == storyId);
    }

    public void ClearVotes(Action<Guid> resetEvent)
    {
        foreach (var player in Players)
        {
            player.CurrentVote = null;
            resetEvent(player.Id);
        }
    }
}
