using System.Text.Encodings.Web;
using JobApplicationManagement.Application.Common.Models;
using Microsoft.Extensions.Options;

namespace JobApplicationManagement.Infrastructure.Email;

internal sealed record RenderedEmail(string Subject, string HtmlBody);

internal sealed class EmailTemplateRenderer
{
    private readonly EmailOptions _options;

    public EmailTemplateRenderer(IOptions<EmailOptions> options) => _options = options.Value;

    public RenderedEmail Render(EmailMessage message)
    {
        string Value(string key) => HtmlEncoder.Default.Encode(
            message.Data.TryGetValue(key, out var value) ? value : string.Empty);
        var baseAddress = string.IsNullOrWhiteSpace(_options.FrontendBaseUrl)
            ? "http://localhost:4200"
            : _options.FrontendBaseUrl.TrimEnd('/');
        var appUrl = HtmlEncoder.Default.Encode(baseAddress);
        var name = HtmlEncoder.Default.Encode(message.RecipientName);
        var (subject, heading, content, cta, path) = message.Template switch
        {
            EmailTemplate.Welcome => ("Welcome to Careerly", "Welcome to Careerly", $"Your Careerly account is ready, {name}.", "Explore jobs", "/jobs"),
            EmailTemplate.VerifyEmail => ("Verify your Careerly email", "Verify your email", $"Use this one-time code to verify your email: <strong>{Value("Otp")}</strong>", "Verify email", "/verify-email"),
            EmailTemplate.ResendEmailVerification => ("Your new Careerly verification code", "Verify your email", $"Your new one-time verification code is: <strong>{Value("Otp")}</strong>", "Verify email", "/verify-email"),
            EmailTemplate.ForgotPasswordOtp => ("Reset your Careerly password", "Password reset", $"Use this one-time code to reset your password: <strong>{Value("Otp")}</strong>", "Reset password", "/reset-password"),
            EmailTemplate.PasswordChanged => ("Your Careerly password was changed", "Password changed", "Your password was changed successfully. If this was not you, contact support immediately.", "Sign in", "/login"),
            EmailTemplate.ApplicationSubmitted => ($"Application Received — {Value("JobTitle")}", "Application received", $"We received your application for <strong>{Value("JobTitle")}</strong> at {Value("CompanyName")} on {Value("ApplicationDate")}. Current status: Applied.", "View application", $"/candidate/applications/{Value("ApplicationId")}"),
            EmailTemplate.NewCandidateApplication => ($"New Application for {Value("JobTitle")}", "New candidate application", $"{Value("CandidateName")} applied for <strong>{Value("JobTitle")}</strong> on {Value("ApplicationDate")}.", "Review application", $"/recruiter/applications/{Value("ApplicationId")}"),
            EmailTemplate.ApplicationStatusChanged => ($"Application update — {Value("JobTitle")}", "Application status updated", $"Your application for <strong>{Value("JobTitle")}</strong> is now {Value("Status")}.", "View application", $"/candidate/applications/{Value("ApplicationId")}"),
            EmailTemplate.CandidateShortlisted => ($"You were shortlisted — {Value("JobTitle")}", "Congratulations, you have been shortlisted", $"Your application for <strong>{Value("JobTitle")}</strong> has progressed to the shortlist.", "View application", $"/candidate/applications/{Value("ApplicationId")}"),
            EmailTemplate.InterviewScheduled => ($"Interview scheduled — {Value("JobTitle")}", "Interview scheduled", $"Your interview for <strong>{Value("JobTitle")}</strong> is scheduled for {Value("ScheduledAt")}.", "View interview", $"/candidate/applications/{Value("ApplicationId")}"),
            EmailTemplate.InterviewRescheduled => ($"Interview rescheduled — {Value("JobTitle")}", "Interview rescheduled", $"Your interview for <strong>{Value("JobTitle")}</strong> is now scheduled for {Value("ScheduledAt")}.", "View interview", $"/candidate/applications/{Value("ApplicationId")}"),
            EmailTemplate.InterviewCancelled => ($"Interview cancelled — {Value("JobTitle")}", "Interview cancelled", $"The interview for <strong>{Value("JobTitle")}</strong> has been cancelled.", "View application", $"/candidate/applications/{Value("ApplicationId")}"),
            EmailTemplate.ApplicationAccepted => ($"Application accepted — {Value("JobTitle")}", "Congratulations — application accepted", $"Your application for <strong>{Value("JobTitle")}</strong> has been accepted.", "View application", $"/candidate/applications/{Value("ApplicationId")}"),
            EmailTemplate.ApplicationRejected => ($"Application update — {Value("JobTitle")}", "Application status updated", $"Your application for <strong>{Value("JobTitle")}</strong> has been updated. Thank you for your interest and effort.", "View application", $"/candidate/applications/{Value("ApplicationId")}"),
            _ => throw new ArgumentOutOfRangeException(nameof(message.Template))
        };

        var href = $"{appUrl}{path}";
        var html = $$"""
        <!doctype html><html><head><meta charset="utf-8"><meta name="viewport" content="width=device-width"></head>
        <body style="margin:0;background:#f4f7fb;font-family:Arial,sans-serif;color:#152238">
          <table role="presentation" width="100%" cellspacing="0" cellpadding="0"><tr><td align="center" style="padding:32px 16px">
            <table role="presentation" width="100%" style="max-width:600px;background:#fff;border-radius:16px;overflow:hidden;border:1px solid #e4eaf2">
              <tr><td style="padding:24px 32px;background:#102a43;color:#fff;font-size:24px;font-weight:700">Careerly</td></tr>
              <tr><td style="padding:32px"><h1 style="font-size:24px;margin:0 0 16px">{{heading}}</h1><p style="line-height:1.7;margin:0 0 24px">Hello {{name}},</p><p style="line-height:1.7;margin:0 0 28px">{{content}}</p><a href="{{href}}" style="display:inline-block;background:#1261a0;color:#fff;text-decoration:none;padding:12px 20px;border-radius:8px;font-weight:700">{{cta}}</a></td></tr>
              <tr><td style="padding:20px 32px;background:#f8fafc;color:#62748a;font-size:13px">Find Your Next Opportunity · Careerly</td></tr>
            </table>
          </td></tr></table>
        </body></html>
        """;
        return new RenderedEmail(subject, html);
    }
}
