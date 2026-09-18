using AutoMapper;
using JobApplicationManagement.Application.Features.Candidates.DTOs;
using JobApplicationManagement.Domain.Entities;

namespace JobApplicationManagement.Application.Features.Candidates.Mappings;

public class CandidateMappingProfile : Profile
{
    public CandidateMappingProfile()
    {
        CreateMap<CreateCandidateDto, Candidate>()
            .ForMember(dest => dest.Id, opt => opt.Ignore());

        CreateMap<UpdateCandidateDto, Candidate>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.UserId, opt => opt.Ignore());

        CreateMap<Candidate, CandidateResponseDto>();
    }
}
