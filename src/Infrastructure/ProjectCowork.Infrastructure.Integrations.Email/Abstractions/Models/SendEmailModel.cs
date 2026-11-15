namespace ProjectCowork.Infrastructure.Integrations.Email.Abstractions.Models;

public record SendEmailModel
{
    public required string Receiver { get; init; }
    public required string MessageContent { get; init; }
}