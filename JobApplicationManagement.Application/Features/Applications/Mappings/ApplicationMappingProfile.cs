using AutoMapper;
using JobApplicationManagement.Application.Features.Applications.DTOs;
using JobApplicationManagement.Domain.Entities;

namespace JobApplicationManagement.Application.Features.Applications.Mappings;

public class ApplicationMappingProfile : Profile
{
    public ApplicationMappingProfile()
    {
        CreateMap<CreateJobApplicationDto, JobApplication>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CandidateId, opt => opt.Ignore())
            .ForMember(dest => dest.JobApplicationStatus, opt => opt.Ignore())
            .ForMember(dest => dest.AppliedAt, opt => opt.Ignore())
            .ForMember(dest => dest.StatusUpdatedAt, opt => opt.Ignore())
            .ForMember(dest => dest.CancelledAt, opt => opt.Ignore())
            .ForMember(dest => dest.Candidate, opt => opt.Ignore())
            .ForMember(dest => dest.Job, opt => opt.Ignore());

        CreateMap<JobApplication, JobApplicationResponseDto>();
    }
}
