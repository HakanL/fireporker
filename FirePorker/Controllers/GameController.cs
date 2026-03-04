using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using FirePorker.Models;

namespace FirePorker.Controllers;

public class GameController : Controller
{
    private readonly IHubContext<PokerHub> _hubContext;
    private readonly Lazy<PokeR> _pokeR;
    private const string CookieName = "FirePorker";

    public GameController(IHubContext<PokerHub> hubContext)
    {
        _hubContext = hubContext;
        _pokeR = new Lazy<PokeR>(() => new PokeR(_hubContext));
    }

    // GET: Game
    public IActionResult Index(Guid id)
    {
        // If no cookie, invite the user to join the game
        var cookie = Request.Cookies[CookieName];
        if (cookie == null)
        {
            return RedirectToAction("Join", new { id });
        }

        var cookieParts = cookie.Split('|');
        if (cookieParts.Length != 2)
        {
            return RedirectToAction("Join", new { id });
        }

        var gameId = cookieParts[0];
        var playerId = cookieParts[1];

        // If the cookie game id does not match, do not show them this game
        if (gameId != id.ToString())
        {
            return RedirectToAction("Join", new { id });
        }

        // If no game was found with the cookie gameId, redirect to create page
        var game = GameManager.GetPokerGame(id);
        if (game == null)
        {
            Response.Cookies.Delete(CookieName);
            return RedirectToAction("Create");
        }

        // If neither the host or any players of this game match the cookie playerId, do not show the user this game
        if (game.Host.Id.ToString() != playerId && game.Players.All(x => x.Id.ToString() != playerId))
        {
            return RedirectToAction("Join", new { id });
        }

        ViewBag.GameId = gameId;
        ViewBag.PlayerId = playerId;
        ViewBag.Title = game.Name;
        ViewBag.Now = DateTime.UtcNow;

        // All tests passed!
        return View(game);
    }

    // GET: Game/Join
    public IActionResult Join(Guid id)
    {
        var game = GameManager.GetPokerGame(id);
        if (game != null)
        {
            ViewBag.GameId = id.ToString();
            return View();
        }
        return RedirectToAction("Create");
    }

    // POST: Game/Join
    [HttpPost]
    public async Task<IActionResult> Join(string GameId, string Name)
    {
        try
        {
            var isValid = !string.IsNullOrEmpty(GameId) && !string.IsNullOrEmpty(Name);

            if (isValid)
            {
                var gameId = Guid.Parse(GameId);

                var game = GameManager.GetPokerGame(gameId);
                if (game != null)
                {
                    var player = game.AddPlayer(Name);
                    GameManager.StorePokerGame(game);

                    await _pokeR.Value.PlayerJoined(gameId, player);

                    var cookieValue = $"{gameId}|{player.Id}";
                    Response.Cookies.Append(CookieName, cookieValue, new Microsoft.AspNetCore.Http.CookieOptions
                    {
                        Expires = game.ExpirationDate
                    });

                    return RedirectToAction("Index", new { id = game.Id });
                }
            }
            else
            {
                throw new Exception("Invalid data");
            }
        }
        catch
        {
            return View();
        }

        return View();
    }

    // GET: Game/Create
    public IActionResult Create()
    {
#if DEBUG
        var game = new PokerGame("Test", "Michael Kaltner", "Test");
        return View(game);
#else
        return View();
#endif
    }

    public IActionResult List()
    {
        var games = GameManager.GetPokerGames();
        return View(games);
    }

    // POST: Game/Create
    [HttpPost]
    public IActionResult Create(string? HostName, string? Name, string? Description, bool HostCanVote = false)
    {
        try
        {
            var isValid = !string.IsNullOrEmpty(HostName) && !string.IsNullOrEmpty(Name);

            if (isValid)
            {
                var game = new PokerGame(Name, HostName, Description ?? string.Empty) { HostCanVote = HostCanVote };
                GameManager.StorePokerGame(game);

                var cookieValue = $"{game.Id}|{game.Host.Id}";
                Response.Cookies.Append(CookieName, cookieValue, new Microsoft.AspNetCore.Http.CookieOptions
                {
                    Expires = game.ExpirationDate
                });

                return RedirectToAction("Index", new { id = game.Id });
            }
            else
            {
                throw new Exception("Invalid data");
            }
        }
        catch
        {
            return View();
        }
    }
}
