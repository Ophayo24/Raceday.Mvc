using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Raceday.Mvc.Services;
using Raceday.Mvc.ViewModels;

namespace Raceday.Mvc.Controllers;

[Authorize(Roles = "Participant")]
public class ParticipantController : Controller
{
    private readonly RaceDayApiClient _api;
    public ParticipantController(RaceDayApiClient api) => _api = api;

    public async Task<IActionResult> Index()
    {
        var enrolments = await _api.MyEnrolmentsAsync();
        var events = await _api.GetEventsAsync();
        var results = await _api.MyResultsAsync();

        ViewData["EnrolmentCount"] = enrolments.Count;
        ViewData["UpcomingCount"] = events.Count(e => e.EventDate >= DateTime.Today);
        ViewData["ResultCount"] = results.Count;
        return View();
    }

    public async Task<IActionResult> MyEvents()
    {
        var enrolments = await _api.MyEnrolmentsAsync();
        var events = await _api.GetEventsAsync();
        var result = new List<dynamic>();
        var categoryCache = new Dictionary<int, List<CategoryVm>>();

        foreach (var en in enrolments)
        {
            var ev = events.FirstOrDefault(e => e.EventId == en.EventId);
            if (ev == null) continue;

            if (!categoryCache.TryGetValue(en.EventId, out var cats))
            {
                cats = await _api.GetCategoriesAsync(en.EventId);
                categoryCache[en.EventId] = cats;
            }

            var catName = cats.FirstOrDefault(c => c.CategoryId == en.CategoryId)?.CategoryName ?? $"Category {en.CategoryId}";
            result.Add(new { Enrolment = en, Event = ev, CategoryName = catName });
        }
        return View(result);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Enrol(int eventId, int categoryId)
    {
        var result = await _api.EnrolAsync(new CreateEnrolmentVm { EventId = eventId, CategoryId = categoryId });
        TempData[result.Success ? "Success" : "Error"] =
            result.Success ? "You have successfully enrolled!" : result.Error ?? "Enrolment failed.";
        return RedirectToAction("Details", "Home", new { id = eventId });
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> CancelEnrolment(int id)
    {
        var ok = await _api.CancelEnrolmentAsync(id);
        TempData[ok ? "Success" : "Error"] = ok ? "Enrolment cancelled." : "Could not cancel.";
        return RedirectToAction(nameof(MyEvents));
    }

    public async Task<IActionResult> MyResults()
    {
        var results = await _api.MyResultsAsync();
        return View(results);
    }

    public async Task<IActionResult> Profile()
    {
        var profile = await _api.GetProfileAsync();
        if (profile == null) return RedirectToAction("Login", "Account");
        return View(profile);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(
        [Bind("FirstName,LastName,Phone,ProfilePictureUrl")] UpdateProfileVm vm,
        IFormFile? profilePicture)
    {
        if (profilePicture != null && profilePicture.Length > 0)
        {
            await using var s = profilePicture.OpenReadStream();
            var url = await _api.UploadFileAsync("api/uploads/profile-picture", s,
                profilePicture.FileName, profilePicture.ContentType);
            if (url != null) vm.ProfilePictureUrl = url;
        }

        var result = await _api.UpdateProfileAsync(vm);
        TempData[result.Success ? "Success" : "Error"] =
            result.Success ? "Profile updated." : result.Error ?? "Update failed.";
        return RedirectToAction(nameof(Profile));
    }
}