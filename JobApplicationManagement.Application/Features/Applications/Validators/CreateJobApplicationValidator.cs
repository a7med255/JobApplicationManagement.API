using FluentValidation;
using JobApplicationManagement.Application.Features.Applications.DTOs;
using JobApplicationManagement.Application.Features.Applications.Commands;

namespace JobApplicationManagement.Application.Features.Applications.Validators;

public class CreateJobApplicationValidator : AbstractValidator<CreateJobApplicationDto>
{
    public CreateJobApplicationValidator()
    {
        RuleFor(x => x.JobId)
            .GreaterThan(0).WithMessage("JobId is required and must be greater than 0.");
        RuleFor(x => x.CandidateCvId)
            .GreaterThan(0).When(x => x.CandidateCvId.HasValue)
            .WithMessage("CandidateCvId must be greater than 0 when provided.");
    }
}

public class ApplyForJobCommandValidator : AbstractValidator<ApplyForJobCommand>
{
    public ApplyForJobCommandValidator() =>
        RuleFor(command => command.Dto).SetValidator(new CreateJobApplicationValidator());
}
