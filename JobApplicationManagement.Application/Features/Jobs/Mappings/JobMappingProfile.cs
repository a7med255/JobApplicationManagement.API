using AutoMapper;
using JobApplicationManagement.Application.Features.Jobs.DTOs;
using JobApplicationManagement.Domain.Entities;

namespace JobApplicationManagement.Application.Features.Jobs.Mappings;

/// <summary>
/// AutoMapper profile defining all mappings related to the Job feature.
/// </summary>
public class JobMappingProfile : Profile
{
    public JobMappingProfile()
    {
        // CreateJobDto → Job
        CreateMap<CreateJobDto, Job>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.RecruiterId, opt => opt.Ignore())
            .ForMember(dest => dest.ClosedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ClosedById, opt => opt.Ignore());

        // UpdateJobDto → Job (for in-place mapping)
        CreateMap<UpdateJobDto, Job>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.IsActive, opt => opt.Ignore())
            .ForMember(dest => dest.RecruiterId, opt => opt.Ignore())
            .ForMember(dest => dest.ClosedAt, opt => opt.Ignore())
            .ForMember(dest => dest.ClosedById, opt => opt.Ignore());

        // Job → JobResponseDto
        CreateMap<Job, JobResponseDto>();

        // Job → CloseJobResponseDto
        CreateMap<Job, CloseJobResponseDto>();
    }
}
