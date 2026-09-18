using FluentValidation;
using JobApplicationManagement.Application.Features.Identity.DTOs;

namespace JobApplicationManagement.Application.Features.Identity.Validators;

public class RegisterValidator : AbstractValidator<RegisterDto>
{
    private static readonly string[] AllowedRoles = { "Candidate", "Recruiter", "Admin" };

    public RegisterValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters.")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters.");

        RuleFor(x => x.FullName)
            .NotEmpty().WithMessage("Full name is required.")
            .MaximumLength(200).WithMessage("Full name must not exceed 200 characters.");

        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("Role is required.")
            .Must(role => AllowedRoles.Contains(role))
                .WithMessage("Role must be one of: Candidate, Recruiter, Admin.");
    }
}
