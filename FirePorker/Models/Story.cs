namespace FirePorker.Models;

public class Story
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public float? Estimate { get; set; }
    
    public Story(Guid gameId, string title, string description)
    {
        Id = Guid.NewGuid();
        GameId = gameId;
        Title = title;
        Description = description;
    }
}
