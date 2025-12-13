using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using CrossSetaWeb.Models;
using Microsoft.AspNetCore.Authorization;

namespace CrossSetaWeb.Controllers;

public class HomeController : Controller
{
    public IActionResult Index()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "index.html");
        if (System.IO.File.Exists(path))
        {
            return PhysicalFile(path, "text/html");
        }
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
