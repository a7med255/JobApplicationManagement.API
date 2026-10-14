using AutoMapper;
using JobApplicationManagement.Application.Features.Applications.DTOs;
using JobApplicationManagement.Domain.Entities;

namespace JobApplicationManagement.Application.Features.Applications.Mappings;

public class ApplicationMappingProfile : Profile
{
    public ApplicationMappingProfile()
    {
        CreateMap<JobApplication, JobApplicationResponseDto>()
            .ForMember(destination => destination.CandidateName, option => option.MapFrom(source => source.Candidate == null ? null : source.Candidate.Name))
            .ForMember(destination => destination.JobTitle, option => option.MapFrom(source => source.Job == null ? null : source.Job.Title))
            .ForMember(destination => destination.SubmittedCvDisplayName, option => option.MapFrom(source => source.SubmittedCv == null ? null : source.SubmittedCv.DisplayName))
            .ForMember(destination => destination.SubmittedCvFileName, option => option.MapFrom(source => source.SubmittedCv == null ? null : source.SubmittedCv.OriginalFileName));
    }
}
