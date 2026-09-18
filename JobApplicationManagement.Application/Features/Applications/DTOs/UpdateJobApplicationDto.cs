using JobApplicationManagement.Domain.Enums;

namespace JobApplicationManagement.Application.Features.Applications.DTOs;

public class UpdateJobApplicationDto
{
    public JobApplicationStatus JobApplicationStatus { get; set; }
}
