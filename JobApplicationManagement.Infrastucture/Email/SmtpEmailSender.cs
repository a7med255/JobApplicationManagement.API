using JobApplicationManagement.Application.Common.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;

namespace JobApplicationManagement.Infrastructure.Email;

internal sealed class SmtpEmailSender : IEmailSender
{
    private readonly EmailOptions _options;
    private readonly EmailTemplateRenderer _renderer;

    public SmtpEmailSender(IOptions<EmailOptions> options, EmailTemplateRenderer renderer)
    {
        _options = options.Value;
        _renderer = renderer;
    }

    public async Task SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
            throw new InvalidOperationException("Email delivery is disabled. Configure the Email section to process the outbox.");

        var rendered = _renderer.Render(message);
        var mime = new MimeMessage();
        var fromAddress = !string.IsNullOrWhiteSpace(_options.FromAddress)
            ? _options.FromAddress
            : _options.UserName;
        mime.From.Add(new MailboxAddress(_options.FromName, fromAddress));
        mime.To.Add(MailboxAddress.Parse(message.Recipient));
        mime.Subject = rendered.Subject;
        mime.Body = new BodyBuilder { HtmlBody = rendered.HtmlBody }.ToMessageBody();

        using var client = new SmtpClient();
        var socketOptions = _options.UseSsl
            ? SecureSocketOptions.SslOnConnect
            : (_options.Port == 587 ? SecureSocketOptions.StartTls : SecureSocketOptions.StartTlsWhenAvailable);
        await client.ConnectAsync(_options.Host, _options.Port, socketOptions, cancellationToken);
        if (!string.IsNullOrWhiteSpace(_options.UserName))
            await client.AuthenticateAsync(_options.UserName, _options.Password, cancellationToken);
        await client.SendAsync(mime, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
