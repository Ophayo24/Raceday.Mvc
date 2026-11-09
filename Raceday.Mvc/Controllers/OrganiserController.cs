using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raceday.Mvc.Services;
using Raceday.Mvc.ViewModels;

namespace Raceday.Mvc.Controllers;

[Authorize(Roles = "Organiser")]
public class OrganiserController : Controller
{
    private readonly RaceDayApiClient _api;
    public OrganiserController(RaceDayApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        var events = await _api.GetEventsAsync();
        var myEvents = events; // API returns all; organiser only sees their own via filtering in client is fine for now
        var counts = new Dictionary<int, int>();
        foreach (var e in myEvents)
        {
            var ens = await _api.EventEnrolmentsAsync(e.EventId);
            counts[e.EventId] = ens.Count;
        }
        ViewData["EnrolmentCounts"] = counts;
        return View(myEvents);
    }

    [HttpGet]
    public IActionResult CreateEvent() => View(new CreateEventVm { EventDate = DateTime.Today.AddDays(30) });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateEvent(CreateEventVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        if (vm.BannerFile != null && vm.BannerFile.Length > 0)
        {
            await using var s = vm.BannerFile.OpenReadStream();
            var url = await _api.UploadFileAsync("api/uploads/event-banner", s, vm.BannerFile.FileName, vm.BannerFile.ContentType);
            if (url != null) vm.BannerImageUrl = url;
        }

        var result = await _api.CreateEventAsync(vm);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error ?? "Create failed.");
            return View(vm);
        }
        TempData["Success"] = "Event created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> EditEvent(int id)
    {
        var ev = await _api.GetEventAsync(id);
        if (ev == null) return NotFound();
        return View(new CreateEventVm
        {
            Name = ev.Name,
            Description = ev.Description,
            EventDate = ev.EventDate,
            Location = ev.Location,
            DistanceKm = ev.DistanceKm,
            EventType = ev.EventType,
            BannerImageUrl = ev.BannerImageUrl
        });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> EditEvent(int id, CreateEventVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        if (vm.BannerFile != null && vm.BannerFile.Length > 0)
        {
            await using var s = vm.BannerFile.OpenReadStream();
            var url = await _api.UploadFileAsync("api/uploads/event-banner", s, vm.BannerFile.FileName, vm.BannerFile.ContentType);
            if (url != null) vm.BannerImageUrl = url;
        }

        var result = await _api.UpdateEventAsync(id, vm);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error ?? "Update failed.");
            return View(vm);
        }
        TempData["Success"] = "Event updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteEvent(int id)
    {
        var ok = await _api.DeleteEventAsync(id);
        TempData[ok ? "Success" : "Error"] = ok ? "Event deleted." : "Delete failed.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Categories(int eventId)
    {
        var ev = await _api.GetEventAsync(eventId);
        if (ev == null) return NotFound();
        ViewData["Event"] = ev;
        return View(await _api.GetCategoriesAsync(eventId));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCategory(int eventId, CreateCategoryVm vm)
    {
        if (!ModelState.IsValid) { TempData["Error"] = "Invalid category."; return RedirectToAction(nameof(Categories), new { eventId }); }
        var result = await _api.CreateCategoryAsync(eventId, vm);
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Category added." : result.Error ?? "Failed.";
        return RedirectToAction(nameof(Categories), new { eventId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCategory(int id, int eventId)
    {
        var ok = await _api.DeleteCategoryAsync(id);
        TempData[ok ? "Success" : "Error"] = ok ? "Category deleted." : "Failed.";
        return RedirectToAction(nameof(Categories), new { eventId });
    }

    public async Task<IActionResult> Enrolments(int eventId)
    {
        var ev = await _api.GetEventAsync(eventId);
        if (ev == null) return NotFound();
        ViewData["Event"] = ev;
        return View(await _api.EventEnrolmentsAsync(eventId));
    }

    public async Task<IActionResult> Results(int eventId)
    {
        var ev = await _api.GetEventAsync(eventId);
        if (ev == null) return NotFound();
        ViewData["Event"] = ev;
        ViewData["Enrolments"] = await _api.EventEnrolmentsAsync(eventId);
        return View(await _api.EventResultsAsync(eventId));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CaptureResult(int eventId, int enrolmentId, string finishTime, int finishingPosition)
    {
        if (!TimeSpan.TryParse(finishTime, out var ts))
        {
            TempData["Error"] = "Invalid finish time. Use HH:MM:SS.";
            return RedirectToAction(nameof(Results), new { eventId });
        }
        var result = await _api.CaptureResultAsync(enrolmentId,
            new CreateResultVm { FinishTime = ts, FinishingPosition = finishingPosition });
        TempData[result.Success ? "Success" : "Error"] = result.Success ? "Result captured." : result.Error ?? "Failed.";
        return RedirectToAction(nameof(Results), new { eventId });
    }
}
