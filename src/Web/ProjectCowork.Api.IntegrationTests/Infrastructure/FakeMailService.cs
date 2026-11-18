using System.Collections.Concurrent;
using ProjectCowork.Infrastructure.Integrations.Email.Abstractions;
using ProjectCowork.Infrastructure.Integrations.Email.Abstractions.Models;

namespace ProjectCowork.Api.IntegrationTests.Infrastructure;

public sealed class FakeMailService : IMailService
{
    private readonly ConcurrentQueue<SendEmailModel> _sent = new();

    public Task SendEmailAsync(SendEmailModel model, CancellationToken ct)
    {
        _sent.Enqueue(model);
        return Task.CompletedTask;
    }

    public IReadOnlyList<SendEmailModel> SentTo(string receiver) =>
        _sent.Where(x => string.Equals(x.Receiver, receiver, StringComparison.OrdinalIgnoreCase)).ToList();
}
