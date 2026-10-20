using JobApplicationManagement.Application.Features.Recruiters.DTOs;
using JobApplicationManagement.Domain.Entities;

namespace JobApplicationManagement.Application.Features.Recruiters;

public static class RecruiterDtoFactory
{
    public static RecruiterResponseDto ToResponse(Recruiter recruiter) =>
        new()
        {
            Id = recruiter.Id,
            Name = recruiter.Name,
            UserId = recruiter.UserId
        };
}
