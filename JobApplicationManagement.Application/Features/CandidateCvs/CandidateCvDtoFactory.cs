using JobApplicationManagement.Application.Features.CandidateCvs.DTOs;
using JobApplicationManagement.Domain.Entities;

namespace JobApplicationManagement.Application.Features.CandidateCvs;

internal static class CandidateCvDtoFactory
{
    public static CandidateCvResponseDto ToResponse(CandidateCv cv) => new()
    {
        Id = cv.Id,
        DisplayName = cv.DisplayName,
        OriginalFileName = cv.OriginalFileName,
        ContentType = cv.ContentType,
        FileSize = cv.FileSize,
        IsDefault = cv.IsDefault,
        UploadedAt = cv.UploadedAt
    };
}
