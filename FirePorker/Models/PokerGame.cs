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

    [Display(Name = "Jira base URL (optional)")]
    public string JiraBaseUrl { get; set; }

    public Player Host { get; set; }
    public IList<Player> Players { get; set; }
    public DateTime ExpirationDate { get; set; }
    public IList<Story> Stories { get; set; }
    public DateTime RoundStarted { get; set; }
    public bool HostCanVote { get; set; } = false;

    // TODO: Configurable number set?
    
    public PokerGame(string name, string hostName, string description = "", string jiraBaseUrl = "", string id = "")
    {
        Id = string.IsNullOrEmpty(id) ? Guid.NewGuid() : Guid.Parse(id);
        Name = name;
        Description = description;
        JiraBaseUrl = jiraBaseUrl;
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
