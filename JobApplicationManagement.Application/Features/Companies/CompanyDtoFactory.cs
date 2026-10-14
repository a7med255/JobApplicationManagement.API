using JobApplicationManagement.Application.Features.Companies.DTOs;
using JobApplicationManagement.Domain.Entities;

namespace JobApplicationManagement.Application.Features.Companies;

internal static class CompanyDtoFactory
{
    public static CompanyResponseDto ToResponse(Company company) => new()
    {
        Id = company.Id,
        Name = company.Name,
        LogoUrl = company.LogoUrl,
        Description = company.Description,
        Website = company.Website,
        Industry = company.Industry,
        CompanySize = company.CompanySize,
        Headquarters = company.Headquarters,
        Country = company.Country,
        City = company.City,
        CreatedAt = company.CreatedAt,
        UpdatedAt = company.UpdatedAt
    };
}
