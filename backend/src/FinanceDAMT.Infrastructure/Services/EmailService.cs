using FinanceDAMT.Application.Common.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace FinanceDAMT.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(
        string toEmail, string toName, string resetLink, CancellationToken cancellationToken = default)
    {
        var subject = "Reset your FinanceDAMT password";
        var htmlBody = $"""
            <h2>Password Reset</h2>
            <p>Hi {toName},</p>
            <p>Click the link below to reset your password. This link expires in 24 hours.</p>
            <p><a href="{resetLink}" style="background:#4F46E5;color:white;padding:12px 24px;text-decoration:none;border-radius:6px;">Reset Password</a></p>
            <p>If you did not request a password reset, you can safely ignore this email.</p>
            <br/>
            <p>The FinanceDAMT Team</p>
            """;

        await SendEmailAsync(toEmail, toName, subject, htmlBody, cancellationToken);
    }

    public async Task SendEmailAsync(
        string toEmail, string toName, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var host = _configuration["SmtpSettings:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogWarning("SMTP host not configured. Email to {Email} not sent. Subject: {Subject}", toEmail, subject);
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            _configuration["SmtpSettings:FromName"] ?? "FinanceDAMT",
            _configuration["SmtpSettings:FromEmail"] ?? "noreply@financedamt.com"));
        message.To.Add(new MailboxAddress(toName, toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
        message.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();

        var port = _configuration.GetValue<int>("SmtpSettings:Port", 587);
        var user = _configuration["SmtpSettings:User"];
        var pass = _configuration["SmtpSettings:Pass"];

        await client.ConnectAsync(host, port, SecureSocketOptions.StartTls, cancellationToken);

        if (!string.IsNullOrWhiteSpace(user) && !string.IsNullOrWhiteSpace(pass))
            await client.AuthenticateAsync(user, pass, cancellationToken);

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);

        _logger.LogInformation("Email sent to {Email}, subject: {Subject}", toEmail, subject);
    }
}
