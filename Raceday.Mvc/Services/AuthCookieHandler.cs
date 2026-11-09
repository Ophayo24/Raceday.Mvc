using System.Net.Http.Headers;

namespace Raceday.Mvc.Services;

public class AuthCookieHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _http;
    public AuthCookieHandler(IHttpContextAccessor http) => _http = http;

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken ct)
    {
        var session = _http.HttpContext?.Session;
        var cookie = session?.GetString("ApiCookie");
        if (!string.IsNullOrEmpty(cookie))
            request.Headers.Add("Cookie", cookie);

        return base.SendAsync(request, ct);
    }
}
