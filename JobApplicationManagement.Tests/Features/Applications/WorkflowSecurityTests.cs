using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using JobApplicationManagement.Application.Features.Applications;
using JobApplicationManagement.Application.Features.Interviews;
using JobApplicationManagement.Domain.Enums;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using AppValidationException = JobApplicationManagement.Application.Common.Exceptions.ValidationException;

namespace JobApplicationManagement.Tests.Features.Applications;

public sealed class WorkflowSecurityTests
{
    [Theory]
    [InlineData(JobApplicationStatus.Applied, JobApplicationStatus.UnderReview, true)]
    [InlineData(JobApplicationStatus.UnderReview, JobApplicationStatus.Shortlisted, true)]
    [InlineData(JobApplicationStatus.Shortlisted, JobApplicationStatus.Interview, true)]
    [InlineData(JobApplicationStatus.Interview, JobApplicationStatus.Accepted, true)]
    [InlineData(JobApplicationStatus.Accepted, JobApplicationStatus.UnderReview, false)]
    [InlineData(JobApplicationStatus.Cancelled, JobApplicationStatus.Applied, false)]
    public void Status_Transitions_Are_Controlled(JobApplicationStatus from, JobApplicationStatus to, bool expected) =>
        Assert.Equal(expected, ApplicationStatusTransitions.CanTransition(from, to));

    [Fact]
    public void Image_Validation_Rejects_Png_Name_With_Executable_Content()
    {
        var upload = new ImageUpload("MZ fake executable bytes"u8.ToArray(), "avatar.png", "image/png");
        Assert.Throws<AppValidationException>(() => ImageUploadValidation.Validate(upload, 2 * 1024 * 1024));
    }

    [Fact]
    public void Image_Validation_Accepts_A_Valid_Png_Signature()
    {
        byte[] uploadBytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0, 0, 0, 0];
        ImageUploadValidation.Validate(new ImageUpload(uploadBytes, "avatar.png", "image/png"), 2 * 1024 * 1024);
    }

    [Fact]
    public async Task Interview_Cannot_Be_Scheduled_In_The_Past()
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.SetupGet(x => x.UserId).Returns("recruiter-1");
        var handler = new InterviewService(
            Mock.Of<IUnitOfWork>(), currentUser.Object, Mock.Of<IIdentityService>(), Mock.Of<IEmailQueue>());

        await Assert.ThrowsAsync<AppValidationException>(() => handler.ScheduleAsync(new ScheduleInterviewDto
        {
            ApplicationId = 1,
            ScheduledAt = DateTime.UtcNow.AddMinutes(-1),
            DurationMinutes = 60,
            InterviewType = InterviewType.Online,
            MeetingLink = "https://meet.example/interview"
        }, CancellationToken.None));
    }
}
