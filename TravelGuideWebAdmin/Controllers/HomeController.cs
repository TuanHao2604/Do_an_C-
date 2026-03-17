using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelGuideWebAdmin.Models;

namespace TravelGuideWebAdmin.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly TravelGuideWebAdmin.Data.TravelGuideContext _context;

    public HomeController(ILogger<HomeController> logger, TravelGuideWebAdmin.Data.TravelGuideContext context)
    {
        _logger = logger;
        _context = context;
    }

    [Authorize]
    public IActionResult Index()
    {
        ViewBag.PoiCount = _context.POIs.Count();
        ViewBag.TourCount = _context.Tours.Count();
        ViewBag.UserCount = _context.Users.Count();
        ViewBag.TourPoiCount = _context.Tour_POIs.Count();
        return View();
    }


    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
