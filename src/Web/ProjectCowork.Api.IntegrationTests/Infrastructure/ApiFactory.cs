using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using ProjectCowork.Infrastructure.Integrations.Email.Abstractions;
using Testcontainers.PostgreSql;
using TUnit.Core.Interfaces;

namespace ProjectCowork.Api.IntegrationTests.Infrastructure;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncInitializer
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("postgres:18-alpine")
        .WithDatabase("ProjectCowork")
        .Build();

    private readonly string _originalDirectory = Directory.GetCurrentDirectory();

    public FakeMailService Mail { get; } = new();

    public string WorkDirectory { get; } =
        Path.Combine(Path.GetTempPath(), $"projectcowork-tests-{Guid.NewGuid():N}");

    public string ReposRoot => Path.Combine(WorkDirectory, "Repos");

    public Uri BaseAddress { get; private set; } = null!;

    public ApiFactory()
    {
        UseKestrel(0);
    }

    public async Task InitializeAsync()
    {
        await _postgres.StartAsync();

        StartServer();

        Directory.CreateDirectory(WorkDirectory);
        Directory.SetCurrentDirectory(WorkDirectory);

        var address = Services.GetRequiredService<IServer>().Features.Get<IServerAddressesFeature>()!
            .Addresses.First();
        BaseAddress = new Uri(address.Replace("[::]", "127.0.0.1").Replace("0.0.0.0", "127.0.0.1"));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("ConnectionStrings:DefaultConnection", _postgres.GetConnectionString());
        builder.UseSetting("AuthCookie:Secure", "false");
        builder.UseSetting("AuthCookie:SameSite", "Lax");
        builder.UseSetting("Serilog:MinimumLevel", "Warning");

        builder.ConfigureTestServices(services =>
        {
            var blobInitializers = services
                .Where(x => x.ServiceType == typeof(IHostedService)
                            && x.ImplementationType?.Name == "BlobContainerInitializer")
                .ToList();
            foreach (var descriptor in blobInitializers)
            {
                services.Remove(descriptor);
            }

            services.RemoveAll<IMailService>();
            services.AddSingleton<IMailService>(Mail);
        });
    }

    public HttpClient CreateApiClient() => CreateClient(new WebApplicationFactoryClientOptions
    {
        BaseAddress = BaseAddress,
        HandleCookies = false,
        AllowAutoRedirect = false
    });

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();

        Directory.SetCurrentDirectory(_originalDirectory);
        try
        {
            Directory.Delete(WorkDirectory, recursive: true);
        }
        catch (IOException)
        {
            // Sprzątanie katalogu tymczasowego nie może psuć wyniku testów.
        }
    }
}
