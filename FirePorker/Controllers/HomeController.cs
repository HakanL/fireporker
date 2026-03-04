using Microsoft.AspNetCore.Mvc;
using FirePorker.Models;

namespace FirePorker.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        ViewBag.GameCount = GameManager.GameCount.ToString("F0");
        return View();
    }

    public IActionResult About()
    {
        ViewBag.Message = "Your application description page.";
        return View();
    }

    public IActionResult Contact()
    {
        ViewBag.Message = "Your contact page.";
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
