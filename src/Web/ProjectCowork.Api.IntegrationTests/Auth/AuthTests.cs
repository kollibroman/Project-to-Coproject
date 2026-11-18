using System.Net;
using System.Security.Claims;
using ProjectCowork.Api.IntegrationTests.Infrastructure;

namespace ProjectCowork.Api.IntegrationTests.Auth;

[ClassDataSource<ApiFactory>(Shared = SharedType.PerTestSession)]
public class AuthTests(ApiFactory factory) : ApiTestBase(factory)
{
    private static IEnumerable<string> RoleClaims(string token) =>
        ReadJwt(token).Claims.Where(x => x.Type is "role" or ClaimTypes.Role).Select(x => x.Value);

    [Test]
    [Arguments("user", "User")]
    [Arguments("business", "Business")]
    public async Task Register_ReturnsTokenWithRole(string kind, string expectedRole)
    {
        var user = await RegisterAsync(kind);

        await Assert.That(RoleClaims(user.AccessToken)).IsEquivalentTo(new[] { expectedRole });
        await Assert.That(user.Id).IsNotEqualTo(Guid.Empty);
    }

    [Test]
    public async Task Login_SetsHttpOnlyRefreshCookieScopedToAuthPath()
    {
        var response = await LoginAsync((await RegisterAsync()).Email, DefaultPassword);

        var cookie = GetSetCookieHeader(response)?.ToLowerInvariant();

        await Assert.That(cookie).IsNotNull();
        await Assert.That(cookie!).Contains("httponly");
        await Assert.That(cookie).Contains("path=/api/auth");
    }

    [Test]
    public async Task Login_ValidCredentials_ReturnsTokenForSameUser()
    {
        var user = await RegisterAsync();

        var response = await LoginAsync(user.Email, user.Password);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var token = ReadJwt(await ReadAccessTokenAsync(response));
        await Assert.That(token.Claims.Any(x => x.Value == user.Id.ToString())).IsTrue();
    }

    [Test]
    public async Task Login_UnknownEmail_Returns404()
    {
        var response = await LoginAsync($"{Unique("nobody")}@example.com", DefaultPassword);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task AccessToken_IsAcceptedByProtectedEndpoint()
    {
        var user = await RegisterAsync();

        var response = await user.Client.GetAsync("/api/projects");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Refresh_WithoutCookie_Returns401()
    {
        var response = await PostWithRefreshCookieAsync("/api/auth/token/refresh", null);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Refresh_WithGarbageCookie_Returns401_AndClearsCookie()
    {
        var response = await PostWithRefreshCookieAsync("/api/auth/token/refresh", "nie-ma-takiego-tokena");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
        await Assert.That(GetRefreshCookie(response)).IsEqualTo(string.Empty);
    }

    [Test]
    public async Task Refresh_ValidCookie_ReturnsNewTokens()
    {
        var user = await RegisterAsync();

        var response = await PostWithRefreshCookieAsync("/api/auth/token/refresh", user.RefreshToken);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);

        var newRefresh = GetRefreshCookie(response);
        await Assert.That(newRefresh).IsNotNull();
        await Assert.That(newRefresh).IsNotEqualTo(user.RefreshToken);

        var newAccess = await ReadAccessTokenAsync(response);
        var projects = await Authorized(newAccess).GetAsync("/api/projects");
        await Assert.That(projects.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Refresh_RotatesToken_OldTokenCannotBeReused()
    {
        var user = await RegisterAsync();
        var first = await PostWithRefreshCookieAsync("/api/auth/token/refresh", user.RefreshToken);
        await EnsureStatusAsync(first, HttpStatusCode.OK);

        var replay = await PostWithRefreshCookieAsync("/api/auth/token/refresh", user.RefreshToken);

        await Assert.That(replay.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Logout_RevokesRefreshToken_AndClearsCookie()
    {
        var user = await RegisterAsync();

        var logout = await PostWithRefreshCookieAsync("/api/auth/logout", user.RefreshToken);

        await Assert.That(logout.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
        await Assert.That(GetRefreshCookie(logout)).IsEqualTo(string.Empty);

        var refresh = await PostWithRefreshCookieAsync("/api/auth/token/refresh", user.RefreshToken);
        await Assert.That(refresh.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Logout_WithoutCookie_Returns204()
    {
        var response = await PostWithRefreshCookieAsync("/api/auth/logout", null);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task SecondLogin_InvalidatesRefreshTokenOfFirstSession()
    {
        var user = await RegisterAsync();
        var secondLogin = await LoginAsync(user.Email, user.Password);
        await EnsureStatusAsync(secondLogin, HttpStatusCode.OK);

        var firstSession = await PostWithRefreshCookieAsync("/api/auth/token/refresh", user.RefreshToken);
        var secondSession = await PostWithRefreshCookieAsync("/api/auth/token/refresh", GetRefreshCookie(secondLogin));

        await Assert.That(firstSession.StatusCode).IsEqualTo(HttpStatusCode.Unauthorized);
        await Assert.That(secondSession.StatusCode).IsEqualTo(HttpStatusCode.OK);
    }
}
