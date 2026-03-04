using System.ComponentModel.DataAnnotations;

namespace FirePorker.Models;

public class Player
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    
    [Required(ErrorMessage = "Your name is required")]
    [Display(Name = "Your name")]
    public string Name { get; set; }
    
    public bool IsHost { get; set; }
    public float? CurrentVote { get; set; }
    
    // TODO: Track player IP for security?

    public Player(Guid gameId, string name, bool isHost = false)
    {
        Id = Guid.NewGuid();
        GameId = gameId;
        Name = name;
        IsHost = isHost;
    }
}
