using Microsoft.Extensions.Options;

namespace ProjectCowork.Api.Utils;

public record AuthCookieSettings
{
    public const string SectionName = "AuthCookie";
    public SameSiteMode SameSite { get; init; } = SameSiteMode.Strict;

    public bool Secure { get; init; } = true;
}

public sealed class RefreshTokenCookie
{
    private const string Name = "pc_refresh_token";
    private const string CookiePath = "/api/auth";

    private readonly AuthCookieSettings _settings;

    public RefreshTokenCookie(IOptions<AuthCookieSettings> settings)
    {
        _settings = settings.Value;
    }

    public void Append(HttpResponse response, string refreshToken, DateTime expiresAtUtc)
    {
        response.Cookies.Append(Name, refreshToken, BuildOptions(expiresAtUtc));
    }

    public void Delete(HttpResponse response)
    {
        response.Cookies.Delete(Name, BuildOptions(expiresAtUtc: null));
    }

    public static string? Read(HttpRequest request)
    {
        return request.Cookies.TryGetValue(Name, out var value) && !string.IsNullOrWhiteSpace(value)
            ? value
            : null;
    }

    private CookieOptions BuildOptions(DateTime? expiresAtUtc) => new()
    {
        HttpOnly = true,
        Secure = _settings.Secure,
        SameSite = _settings.SameSite,
        Path = CookiePath,
        Expires = expiresAtUtc is null ? null : new DateTimeOffset(expiresAtUtc.Value, TimeSpan.Zero),
        IsEssential = true
    };
}
