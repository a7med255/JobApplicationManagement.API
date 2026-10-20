namespace JobApplicationManagement.Application.Common.Models;

public enum EmailTemplate
{
    Welcome,
    VerifyEmail,
    ResendEmailVerification,
    ForgotPasswordOtp,
    PasswordChanged,
    ApplicationSubmitted,
    NewCandidateApplication,
    ApplicationStatusChanged,
    CandidateShortlisted,
    InterviewScheduled,
    InterviewRescheduled,
    InterviewCancelled,
    ApplicationAccepted,
    ApplicationRejected
}

public sealed record EmailMessage(
    string Recipient,
    string RecipientName,
    EmailTemplate Template,
    IReadOnlyDictionary<string, string> Data,
    string? RecipientUserId = null);
