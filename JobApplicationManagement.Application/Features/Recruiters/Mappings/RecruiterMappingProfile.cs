using AutoMapper;
using JobApplicationManagement.Application.Features.Recruiters.DTOs;
using JobApplicationManagement.Domain.Entities;

namespace JobApplicationManagement.Application.Features.Recruiters.Mappings;

public class RecruiterMappingProfile : Profile
{
    public RecruiterMappingProfile()
    {
        CreateMap<CreateRecruiterDto, Recruiter>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Jobs, opt => opt.Ignore());

        CreateMap<UpdateRecruiterDto, Recruiter>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore())
            .ForMember(dest => dest.Jobs, opt => opt.Ignore());

        CreateMap<Recruiter, RecruiterResponseDto>();
    }
}
