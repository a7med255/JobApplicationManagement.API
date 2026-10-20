using JobApplicationManagement.Application.Features.Candidates.DTOs;
using JobApplicationManagement.Domain.Entities;

namespace JobApplicationManagement.Application.Features.Candidates;

public static class CandidateDtoFactory
{
    public static CandidateResponseDto ToResponse(Candidate candidate) =>
        new()
        {
            Id = candidate.Id,
            Name = candidate.Name,
            CvUrl = candidate.CvUrl,
            UserId = candidate.UserId,
            ProfileImageUrl = candidate.ProfileImageUrl,
            ProfessionalTitle = candidate.ProfessionalTitle,
            Summary = candidate.Summary,
            Country = candidate.Country,
            City = candidate.City,
            YearsOfExperience = candidate.YearsOfExperience,
            ProfileCompleteness = CalculateCompleteness(candidate)
        };

    private static int CalculateCompleteness(Candidate candidate)
    {
        var completed = new[]
        {
            candidate.Name, candidate.ProfessionalTitle, candidate.Summary, candidate.Country,
            candidate.City, candidate.ProfileImageUrl
        }.Count(value => !string.IsNullOrWhiteSpace(value));
        if (candidate.YearsOfExperience.HasValue) completed++;
        if (!string.IsNullOrWhiteSpace(candidate.CvUrl) || candidate.Cvs.Any(cv => !cv.IsDeleted)) completed++;
        return completed * 100 / 8;
    }
}
