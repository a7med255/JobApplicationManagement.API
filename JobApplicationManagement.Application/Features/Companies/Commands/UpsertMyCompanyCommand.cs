using FluentValidation;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Companies.DTOs;
using JobApplicationManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Companies.Commands;

public record UpsertMyCompanyCommand(UpdateCompanyDto Company) : IRequest<CompanyResponseDto>;

public class UpsertMyCompanyCommandValidator : AbstractValidator<UpsertMyCompanyCommand>
{
    public UpsertMyCompanyCommandValidator()
    {
        RuleFor(command => command.Company.Name).NotEmpty().MaximumLength(200);
        RuleFor(command => command.Company.LogoUrl).MaximumLength(2000);
        RuleFor(command => command.Company.Description).MaximumLength(5000);
        RuleFor(command => command.Company.Website).MaximumLength(2000);
        RuleFor(command => command.Company.Industry).MaximumLength(150);
        RuleFor(command => command.Company.CompanySize).MaximumLength(100);
        RuleFor(command => command.Company.Headquarters).MaximumLength(300);
        RuleFor(command => command.Company.Country).MaximumLength(100);
        RuleFor(command => command.Company.City).MaximumLength(100);
    }
}

public class UpsertMyCompanyCommandHandler : IRequestHandler<UpsertMyCompanyCommand, CompanyResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpsertMyCompanyCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUserService)
    {
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<CompanyResponseDto> Handle(
        UpsertMyCompanyCommand request,
        CancellationToken cancellationToken)
    {
        var recruiter = await _unitOfWork.Recruiters.Query()
            .Include(item => item.Company)
            .FirstOrDefaultAsync(item => item.UserId == _currentUserService.UserId, cancellationToken)
            ?? throw new ForbiddenException("Only registered recruiters can manage a company profile.");

        var now = DateTime.UtcNow;
        var company = recruiter.Company;
        if (company is null)
        {
            company = new Company { CreatedAt = now };
            recruiter.Company = company;
            await _unitOfWork.Companies.AddAsync(company);
        }

        var dto = request.Company;
        company.Name = dto.Name.Trim();
        company.LogoUrl = Normalize(dto.LogoUrl);
        company.Description = Normalize(dto.Description);
        company.Website = Normalize(dto.Website);
        company.Industry = Normalize(dto.Industry);
        company.CompanySize = Normalize(dto.CompanySize);
        company.Headquarters = Normalize(dto.Headquarters);
        company.Country = Normalize(dto.Country);
        company.City = Normalize(dto.City);
        company.UpdatedAt = now;

        _unitOfWork.Recruiters.Update(recruiter);
        await _unitOfWork.SaveChangesAsync();
        return CompanyDtoFactory.ToResponse(company);
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
