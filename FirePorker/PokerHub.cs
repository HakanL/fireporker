using Microsoft.AspNetCore.SignalR;
using FirePorker.Models;

namespace FirePorker;

public class PokerHub : Hub
{
    public async Task PlayerJoined(string gameId, string name)
    {
        var gid = Guid.Parse(gameId);

        var game = GameManager.GetPokerGame(gid);
        if (game != null)
        {
            await Clients.Group(gameId).SendAsync("playerJoined", name);
        }
    }

    public async Task PlayerVoted(string gameId, string playerId, float? points)
    {
        var gid = Guid.Parse(gameId);
        var pid = Guid.Parse(playerId);

        var game = GameManager.GetPokerGame(gid);
        if (game != null)
        {
            var player = game.GetPlayer(pid);
            if (player != null)
            {
                player.CurrentVote = points;
                await Clients.Group(gameId).SendAsync("playerVoted", playerId, points);
            }
        }
    }

    public async Task ClearVotes(string gameId)
    {
        var gid = Guid.Parse(gameId);

        var game = GameManager.GetPokerGame(gid);
        if (game != null)
        {
            game.ClearVotes(async playerId => 
            { 
                await Clients.Group(gameId).SendAsync("playerVoted", playerId.ToString(), null); 
            });
        }
    }

    public async Task PlayerLeft(string gameId, string playerId)
    {
        var gid = Guid.Parse(gameId);
        var pid = Guid.Parse(playerId);

        var game = GameManager.GetPokerGame(gid);
        if (game != null)
        {
            game.RemovePlayer(pid);
            await Clients.Group(gameId).SendAsync("playerLeft", playerId);
        }
    }

    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
    }

    public async Task AddStory(string gameId, string title, string description)
    {
        var gid = Guid.Parse(gameId);

        var game = GameManager.GetPokerGame(gid);
        if (game != null)
        {
            game.RoundStarted = DateTime.UtcNow;

            var story = game.AddStory(title, description);
            await Clients.Group(gameId).SendAsync("storyAdded", story, game.RoundStarted);
        }
    }

    public async Task AcceptEstimate(string gameId, string storyId, float estimate)
    {
        var gid = Guid.Parse(gameId);
        var sid = Guid.Parse(storyId);

        var game = GameManager.GetPokerGame(gid);
        if (game != null)
        {
            var story = game.GetStory(sid);
            if (story != null)
            {
                story.Estimate = estimate;
                await ClearVotes(gameId);
                await Clients.Group(gameId).SendAsync("estimateAccepted", storyId, estimate);
            }
        }
    }
}
