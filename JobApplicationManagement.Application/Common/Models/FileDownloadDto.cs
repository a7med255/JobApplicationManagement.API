namespace JobApplicationManagement.Application.Common.Models;

public record FileDownloadDto(byte[] Content, string ContentType, string FileName);
