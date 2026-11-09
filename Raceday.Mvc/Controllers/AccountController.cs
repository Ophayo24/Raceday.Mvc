using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Raceday.Mvc.Services;
using Raceday.Mvc.ViewModels;
using System.Security.Claims;

namespace Raceday.Mvc.Controllers;

public class AccountController : Controller
{
    private readonly RaceDayApiClient _api;
    public AccountController(RaceDayApiClient api) => _api = api;

    [HttpGet]
    public IActionResult Register() => View(new RegisterVm());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await _api.RegisterAsync(vm);
        if (!result.Success)
        {
            ModelState.AddModelError("", result.Error ?? "Registration failed.");
            return View(vm);
        }

        // Auto-login after registration
        var login = await _api.LoginAsync(new LoginVm { Email = vm.Email, Password = vm.Password });
        if (login.Success)
            await SignInMvc(result.Data!.UserId, result.Data.Email, result.Data.Role, $"{result.Data.FirstName} {result.Data.LastName}");

        return RedirectToRoleHome(result.Data!.Role);
    }

    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginVm());
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginVm vm, string? returnUrl = null)
    {
        if (!ModelState.IsValid) return View(vm);

        var result = await _api.LoginAsync(vm);
        if (!result.Success)
        {
            ModelState.AddModelError("", "Invalid email or password.");
            return View(vm);
        }

        var profile = await _api.GetProfileAsync();
        if (profile == null)
        {
            ModelState.AddModelError("", "Could not load profile.");
            return View(vm);
        }

        await SignInMvc(profile.UserId, profile.Email, profile.Role,
            $"{profile.FirstName} {profile.LastName}", profile.ProfilePictureUrl);

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            return Redirect(returnUrl);

        return RedirectToRoleHome(profile.Role);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _api.LogoutAsync();
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index", "Home");
    }

    public IActionResult Denied() => View();

    private async Task SignInMvc(int userId, string email, string role, string name, string? picture = null)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Email, email),
            new(ClaimTypes.Name, name),
            new(ClaimTypes.Role, role)
        };
        if (!string.IsNullOrEmpty(picture))
            claims.Add(new Claim("ProfilePictureUrl", picture));

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));
    }

    private IActionResult RedirectToRoleHome(string role) => role switch
    {
        "Organiser" => RedirectToAction("Index", "Organiser"),
        "Participant" => RedirectToAction("Index", "Participant"),
        _ => RedirectToAction("Index", "Home")
    };
}