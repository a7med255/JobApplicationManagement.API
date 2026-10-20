using AutoMapper;
using JobApplicationManagement.Application.Features.Applications.DTOs;
using JobApplicationManagement.Domain.Entities;

namespace JobApplicationManagement.Application.Features.Applications.Mappings;

public class ApplicationMappingProfile : Profile
{
    public ApplicationMappingProfile()
    {
        CreateMap<ApplicationStatusHistory, ApplicationStatusHistoryDto>();
        CreateMap<Interview, InterviewSummaryDto>()
            .ForMember(d => d.InterviewType, o => o.MapFrom(s => s.InterviewType.ToString()))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));
        CreateMap<JobApplication, JobApplicationResponseDto>()
            .ForMember(destination => destination.CandidateName, option => option.MapFrom(source => source.Candidate == null ? null : source.Candidate.Name))
            .ForMember(destination => destination.JobTitle, option => option.MapFrom(source => source.Job == null ? null : source.Job.Title))
            .ForMember(destination => destination.CompanyName, option => option.MapFrom(source => source.Job == null || source.Job.Company == null ? null : source.Job.Company.Name))
            .ForMember(destination => destination.SubmittedCvDisplayName, option => option.MapFrom(source => source.SubmittedCv == null ? null : source.SubmittedCv.DisplayName))
            .ForMember(destination => destination.SubmittedCvFileName, option => option.MapFrom(source => source.SubmittedCv == null ? null : source.SubmittedCv.OriginalFileName));
    }
}
