using Microsoft.AspNetCore.Mvc;
using Raceday.Mvc.Services;
using Raceday.Mvc.ViewModels;

namespace Raceday.Mvc.Controllers;

public class HomeController : Controller
{
    private readonly RaceDayApiClient _api;
    public HomeController(RaceDayApiClient api) => _api = api;

    public async Task<IActionResult> Index(string? type, string? location)
    {
        var events = await _api.GetEventsAsync();

        if (!string.IsNullOrWhiteSpace(type))
            events = events.Where(e => e.EventType.Equals(type, StringComparison.OrdinalIgnoreCase)).ToList();
        if (!string.IsNullOrWhiteSpace(location))
            events = events.Where(e => e.Location.Contains(location, StringComparison.OrdinalIgnoreCase)).ToList();

        ViewData["TypeFilter"] = type;
        ViewData["LocationFilter"] = location;
        return View(events);
    }

    public async Task<IActionResult> Details(int id)
    {
        var ev = await _api.GetEventAsync(id);
        if (ev == null) return NotFound();
        ViewData["Categories"] = await _api.GetCategoriesAsync(id);
        return View(ev);
    }
}
