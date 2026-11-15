using ProjectCowork.Infrastructure.Integrations.Email.Abstractions.Models;

namespace ProjectCowork.Infrastructure.Integrations.Email.Abstractions;

public interface IMailService
{
    Task SendEmailAsync(SendEmailModel model, CancellationToken ct);
}