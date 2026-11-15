using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using ProjectCowork.Infrastructure.Integrations.Email.Abstractions;
using ProjectCowork.Infrastructure.Integrations.Email.Abstractions.Models;
using ProjectCowork.Infrastructure.Integrations.Email.Internal.Options;

namespace ProjectCowork.Infrastructure.Integrations.Email.Internal;

internal class MailService : IMailService
{
    private readonly EmailOptions _emailOptions;

    public MailService(IOptions<EmailOptions> emailOptions)
    {
        _emailOptions = emailOptions.Value;
    }

    public async Task SendEmailAsync(SendEmailModel model, CancellationToken ct)
    {
        await using var client = new SmtpClient();
        
        await client.ConnectAsync(_emailOptions.Host, _emailOptions.Port, SecureSocketOptions.StartTls, ct);
        await client.AuthenticateAsync(_emailOptions.Username, _emailOptions.Password, ct);

        using var message = new MimeMessage(_emailOptions.Username, model.Receiver);

        message.Subject = model.Receiver;
        message.Body = new TextPart("html")
        {
            Text = model.MessageContent
        };

        var request = new SmtpSendRequest(message);
        var options = FormatOptions.Default.Clone();
        
        await client.SendAsync(options, request, ct);
    }
}