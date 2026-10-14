using AutoMapper;
using JobApplicationManagement.Application.Features.Jobs.DTOs;
using JobApplicationManagement.Domain.Entities;

namespace JobApplicationManagement.Application.Features.Jobs.Mappings;

public class JobMappingProfile : Profile
{
    public JobMappingProfile()
    {
        // Job write/read mappings are explicit in JobWriteMapper and JobDtoFactory
        // so ownership, lifecycle, skills and salary visibility cannot be overwritten.
        CreateMap<Company, CompanySummaryDto>();
    }
}
