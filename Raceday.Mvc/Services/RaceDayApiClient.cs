using Microsoft.Extensions.Logging;
using Raceday.Mvc.ViewModels;
using System.Net.Http.Json;
using System.Text.Json;

namespace Raceday.Mvc.Services;

public class RaceDayApiClient
{
    private readonly HttpClient _http;
    private readonly IHttpContextAccessor _httpCtx;
    private static readonly JsonSerializerOptions JsonOpts = new(JsonSerializerDefaults.Web);

    public RaceDayApiClient(HttpClient http, IHttpContextAccessor httpCtx)
    {
        _http = http;
        _httpCtx = httpCtx;
    }
    // ---------- AUTH ----------
    public async Task<ApiResult<LoginResponse>> LoginAsync(LoginVm vm)
    {
        var resp = await _http.PostAsJsonAsync("api/auth/login", vm);
        if (!resp.IsSuccessStatusCode)
            return ApiResult<LoginResponse>.Fail((int)resp.StatusCode, await ReadError(resp));

        // capture Set-Cookie from API and store in MVC session
        if (resp.Headers.TryGetValues("Set-Cookie", out var cookies))
        {
            var apiCookie = string.Join("; ", cookies.Select(c => c.Split(';')[0]));
            _httpCtx.HttpContext!.Session.SetString("ApiCookie", apiCookie);
        }

        var body = await resp.Content.ReadFromJsonAsync<LoginResponse>(JsonOpts);
        return ApiResult<LoginResponse>.Ok(body!);
    }

    public async Task LogoutAsync()
    {
        try { await _http.PostAsync("api/auth/logout", null); } catch { }
        _httpCtx.HttpContext!.Session.Remove("ApiCookie");
    }

    public async Task<ApiResult<UserProfileVm>> RegisterAsync(RegisterVm vm)
    {
        var resp = await _http.PostAsJsonAsync("api/auth/register", vm);
        if (!resp.IsSuccessStatusCode)
            return ApiResult<UserProfileVm>.Fail((int)resp.StatusCode, await ReadError(resp));
        var body = await resp.Content.ReadFromJsonAsync<UserProfileVm>(JsonOpts);
        return ApiResult<UserProfileVm>.Ok(body!);
    }

    // ---------- USERS ----------
    public async Task<UserProfileVm?> GetProfileAsync()
    {
        var resp = await _http.GetAsync("api/users/profile");
        return resp.IsSuccessStatusCode
            ? await resp.Content.ReadFromJsonAsync<UserProfileVm>(JsonOpts)
            : null;
    }

    public async Task<ApiResult<UserProfileVm>> UpdateProfileAsync(UpdateProfileVm vm)
    {
        var resp = await _http.PutAsJsonAsync("api/users/profile", vm);
        if (!resp.IsSuccessStatusCode)
            return ApiResult<UserProfileVm>.Fail((int)resp.StatusCode, await ReadError(resp));
        return ApiResult<UserProfileVm>.Ok((await resp.Content.ReadFromJsonAsync<UserProfileVm>(JsonOpts))!);
    }

    public async Task<List<MyResultVm>> MyResultsAsync()
    {
        var resp = await _http.GetAsync("api/users/me/results");
        return resp.IsSuccessStatusCode
            ? (await resp.Content.ReadFromJsonAsync<List<MyResultVm>>(JsonOpts) ?? new())
            : new();
    }

    // ---------- EVENTS ----------
    public async Task<List<EventVm>> GetEventsAsync()
    {
        var resp = await _http.GetAsync("api/events");
        return resp.IsSuccessStatusCode
            ? (await resp.Content.ReadFromJsonAsync<List<EventVm>>(JsonOpts) ?? new())
            : new();
    }

    public async Task<EventVm?> GetEventAsync(int id)
    {
        var resp = await _http.GetAsync($"api/events/{id}");
        return resp.IsSuccessStatusCode
            ? await resp.Content.ReadFromJsonAsync<EventVm>(JsonOpts)
            : null;
    }

    public async Task<ApiResult<EventVm>> CreateEventAsync(CreateEventVm vm)
    {
        var resp = await _http.PostAsJsonAsync("api/events", vm);
        if (!resp.IsSuccessStatusCode)
            return ApiResult<EventVm>.Fail((int)resp.StatusCode, await ReadError(resp));
        return ApiResult<EventVm>.Ok((await resp.Content.ReadFromJsonAsync<EventVm>(JsonOpts))!);
    }

    public async Task<ApiResult<EventVm>> UpdateEventAsync(int id, CreateEventVm vm)
    {
        var resp = await _http.PutAsJsonAsync($"api/events/{id}", vm);
        if (!resp.IsSuccessStatusCode)
            return ApiResult<EventVm>.Fail((int)resp.StatusCode, await ReadError(resp));
        return ApiResult<EventVm>.Ok((await resp.Content.ReadFromJsonAsync<EventVm>(JsonOpts))!);
    }

    public async Task<bool> DeleteEventAsync(int id) =>
        (await _http.DeleteAsync($"api/events/{id}")).IsSuccessStatusCode;

    // ---------- CATEGORIES ----------
    public async Task<List<CategoryVm>> GetCategoriesAsync(int eventId)
    {
        var resp = await _http.GetAsync($"api/events/{eventId}/categories");
        return resp.IsSuccessStatusCode
            ? (await resp.Content.ReadFromJsonAsync<List<CategoryVm>>(JsonOpts) ?? new())
            : new();
    }

    public async Task<ApiResult<CategoryVm>> CreateCategoryAsync(int eventId, CreateCategoryVm vm)
    {
        var resp = await _http.PostAsJsonAsync($"api/events/{eventId}/categories", vm);
        if (!resp.IsSuccessStatusCode)
            return ApiResult<CategoryVm>.Fail((int)resp.StatusCode, await ReadError(resp));
        return ApiResult<CategoryVm>.Ok((await resp.Content.ReadFromJsonAsync<CategoryVm>(JsonOpts))!);
    }

    public async Task<ApiResult<CategoryVm>> UpdateCategoryAsync(int id, CreateCategoryVm vm)
    {
        var resp = await _http.PutAsJsonAsync($"api/categories/{id}", vm);
        if (!resp.IsSuccessStatusCode)
            return ApiResult<CategoryVm>.Fail((int)resp.StatusCode, await ReadError(resp));
        return ApiResult<CategoryVm>.Ok((await resp.Content.ReadFromJsonAsync<CategoryVm>(JsonOpts))!);
    }

    public async Task<bool> DeleteCategoryAsync(int id) =>
        (await _http.DeleteAsync($"api/categories/{id}")).IsSuccessStatusCode;

    // ---------- ENROLMENTS ----------
    public async Task<ApiResult<EnrolmentVm>> EnrolAsync(CreateEnrolmentVm vm)
    {
        var resp = await _http.PostAsJsonAsync("api/enrolments", vm);
        if (!resp.IsSuccessStatusCode)
            return ApiResult<EnrolmentVm>.Fail((int)resp.StatusCode, await ReadError(resp));
        return ApiResult<EnrolmentVm>.Ok((await resp.Content.ReadFromJsonAsync<EnrolmentVm>(JsonOpts))!);
    }

    public async Task<List<EnrolmentVm>> MyEnrolmentsAsync()
    {
        var resp = await _http.GetAsync("api/enrolments/me");
        return resp.IsSuccessStatusCode
            ? (await resp.Content.ReadFromJsonAsync<List<EnrolmentVm>>(JsonOpts) ?? new())
            : new();
    }

    public async Task<List<EventEnrolmentVm>> EventEnrolmentsAsync(int eventId)
    {
        var resp = await _http.GetAsync($"api/enrolments/event/{eventId}");
        return resp.IsSuccessStatusCode
            ? (await resp.Content.ReadFromJsonAsync<List<EventEnrolmentVm>>(JsonOpts) ?? new())
            : new();
    }

    public async Task<bool> CancelEnrolmentAsync(int id) =>
        (await _http.PutAsync($"api/enrolments/{id}/cancel", null)).IsSuccessStatusCode;

    // ---------- RESULTS ----------
    public async Task<ApiResult<ResultVm>> CaptureResultAsync(int enrolmentId, CreateResultVm vm)
    {
        var resp = await _http.PostAsJsonAsync($"api/enrolments/{enrolmentId}/result", vm);
        if (!resp.IsSuccessStatusCode)
            return ApiResult<ResultVm>.Fail((int)resp.StatusCode, await ReadError(resp));
        return ApiResult<ResultVm>.Ok((await resp.Content.ReadFromJsonAsync<ResultVm>(JsonOpts))!);
    }

    public async Task<List<ResultVm>> EventResultsAsync(int eventId)
    {
        var resp = await _http.GetAsync($"api/events/{eventId}/results");
        return resp.IsSuccessStatusCode
            ? (await resp.Content.ReadFromJsonAsync<List<ResultVm>>(JsonOpts) ?? new())
            : new();
    }

    // ---------- UPLOADS ----------
    public async Task<string?> UploadFileAsync(string endpoint, Stream stream, string fileName, string contentType)
    {
        using var content = new MultipartFormDataContent();
        var fileContent = new StreamContent(stream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName);

        var resp = await _http.PostAsync(endpoint, content);
        if (!resp.IsSuccessStatusCode) return null;

        var doc = await resp.Content.ReadFromJsonAsync<JsonElement>();
        return doc.TryGetProperty("url", out var url) ? url.GetString() : null;
    }

    private static async Task<string> ReadError(HttpResponseMessage resp)
    {
        try
        {
            var doc = await resp.Content.ReadFromJsonAsync<JsonElement>();
            return doc.TryGetProperty("error", out var e) ? e.GetString() ?? "Request failed." : "Request failed.";
        }
        catch { return "Request failed."; }
    }
}

public class ApiResult<T>
{
    public bool Success { get; init; }
    public int StatusCode { get; init; }
    public string? Error { get; init; }
    public T? Data { get; init; }

    public static ApiResult<T> Ok(T data) => new() { Success = true, StatusCode = 200, Data = data };
    public static ApiResult<T> Fail(int code, string error) => new() { Success = false, StatusCode = code, Error = error };
}
