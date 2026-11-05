using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;
using ProjectCowork.Domain.Exceptions.Github;
using ProjectCowork.Infrastructure.Integrations.Github.Abstractions;
using ProjectCowork.Infrastructure.Integrations.Github.Internal.Options;
using ProjectCowork.Infrastructure.Integrations.Github.Models;
using RestSharp;

namespace ProjectCowork.Infrastructure.Integrations.Github.Internal;

internal class GithubService : IGithubService
{
    private readonly IRestClient _restClient;
    private readonly GithubOptions _options;
    private readonly ILogger<GithubService> _logger;
    
    public GithubService(IRestClient restClient, IOptions<GithubOptions> options, ILogger<GithubService> logger)
    {
        _restClient = restClient;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<IEnumerable<RepositoryModel>?> FetchUserRepositoriesAsync(string login, string pat, CancellationToken ct)
    {
        var url = $"{_options.BaseUrl}/user/repos"; 
        
        var request = new RestRequest(url);
        request.AddHeader("Authorization", $"Bearer {pat}");

        var pipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(),
                MaxRetryAttempts = 3,
                DelayGenerator = args =>
                {
                    var delay = TimeSpan.FromSeconds(Math.Pow(2, args.AttemptNumber));
                    return new ValueTask<TimeSpan?>(delay);
                },
                OnRetry = args =>
                {
                    _logger.LogWarning(args.Outcome.Exception,
                        "Fetching {RetryCount}/3 couldnt succeed. Next try in {TimeSpan}s.",
                        args.AttemptNumber + 1, args.RetryDelay.TotalSeconds);
                    return default;
                }
            })
            .Build();

        return await pipeline.ExecuteAsync<IEnumerable<RepositoryModel>?>(async token =>
        {
            var result = await _restClient.ExecuteAsync(request, token);

            if (string.IsNullOrEmpty(result.Content))
            {
                throw new GithubApiException("Couldnt fetch user repositories");
            }
            
            return JsonSerializer.Deserialize<IEnumerable<RepositoryModel>>(result.Content);
        }, ct);
    }
}