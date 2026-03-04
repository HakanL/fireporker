using Microsoft.AspNetCore.SignalR;
using PlanningPoker.Models;

namespace PlanningPoker;

public class PokeR
{
    private readonly IHubContext<PokerHub> _context;

    public PokeR(IHubContext<PokerHub> context)
    {
        _context = context;
    }

    public async Task PlayerJoined(Guid gameId, Player player)
    {
        await _context.Clients.Group(gameId.ToString()).SendAsync("playerJoined", player);
    }
}
