using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.SignalR;
using PlanningPoker.Models;

namespace PlanningPoker.Controllers
{
    public class GameController : Controller
    {
        private readonly static Lazy<PokeR> _instance = new Lazy<PokeR>(() => new PokeR(GlobalHost.ConnectionManager.GetHubContext<PokerHub>()));
        private static string _cookieName = "PlanningPoker";
        
        // GET: Game
        public ActionResult Index(Guid id)
        {
            // If no cookie, invite the user to join the game
            var cookie = Request.Cookies[_cookieName];
            if (cookie == null)
            {
                return RedirectToAction("Join", new {id});
            }

            var gameId = cookie["GameId"];
            var playerId = cookie["PlayerId"];

            // If the cookie game id does not match, do not show them this game
            if (gameId != id.ToString())
            {
                //throw new Exception("You have not joined this game");
                return RedirectToAction("Join", new { id });
            }

            // If no game was found with the cookie gameId, redirect to create page
            var game = GameManager.GetPokerGame(id);
            if (game == null)
            {
                Response.Cookies.Remove(_cookieName);
                return RedirectToAction("Create");
            }

            // If neither the host or any players of this game match the cookie playerId, do not show the user this game
            if (game.Host.Id.ToString() != playerId && game.Players.All(x => x.Id.ToString() != playerId))
            {
                //throw new Exception("You have not joined this game");
                return RedirectToAction("Join", new { id });
            }

            ViewBag.GameId = gameId;
            ViewBag.PlayerId = playerId;

            // All tests passed!
            return View(game);
        }

        // GET: Game/Join
        public ActionResult Join(Guid id)
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
        public ActionResult Join(FormCollection collection)
        {
            try
            {
                var id = collection["GameId"];
                var name = collection["Name"];

                var isValid = !string.IsNullOrEmpty(id) && !string.IsNullOrEmpty(name);

                if (isValid)
                {
                    var gameId = Guid.Parse(id);

                    var game = GameManager.GetPokerGame(gameId);
                    if (game != null)
                    {
                        var player = game.AddPlayer(name);
                        GameManager.StorePokerGame(game);

                        _instance.Value.PlayerJoined(gameId, player);

                        var cookie = new HttpCookie(_cookieName)
                        {
                            ["GameId"] = gameId.ToString(),
                            ["PlayerId"] = player.Id.ToString()
                        };
                        Response.Cookies.Add(cookie);
                    }
                    
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

        // GET: Game/Create
        public ActionResult Create()
        {
#if DEBUG
            var game = new PokerGame("Test", "Michael Kaltner", "Test");
            return View(game);
#else
            return View();
#endif
        }
        
        // POST: Game/Create
        [HttpPost]
        public ActionResult Create(FormCollection collection)
        {
            try
            {
                var hostName = collection["Host.Name"];
                var gameName = collection["Name"];
                var gameDescription = collection["Description"];

                var isValid = !string.IsNullOrEmpty(hostName) && !string.IsNullOrEmpty(gameName) &&
                              !string.IsNullOrEmpty(gameDescription);

                if (isValid)
                {
                    var game = new PokerGame(gameName, hostName, gameDescription);
                    GameManager.StorePokerGame(game);

                    var cookie = new HttpCookie(_cookieName)
                    {
                        ["GameId"] = game.Id.ToString(),
                        ["PlayerId"] = game.Host.Id.ToString()
                    };
                    Response.Cookies.Add(cookie);

                    return RedirectToAction("Index", new {id = game.Id});
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
}
