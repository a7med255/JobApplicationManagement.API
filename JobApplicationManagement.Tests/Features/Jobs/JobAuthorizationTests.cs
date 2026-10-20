using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Jobs;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Tests.Features.SavedJobs;
using Moq;
using Xunit;

namespace JobApplicationManagement.Tests.Features.Jobs;

public class JobAuthorizationTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock = new();
    private readonly Mock<ICurrentUserService> _currentUserServiceMock = new();

    private void SetupRecruiters(IEnumerable<Recruiter> recruiters)
    {
        var asyncEnumerable = new SavedJobTestAsyncEnumerable<Recruiter>(recruiters);
        _unitOfWorkMock.Setup(u => u.Recruiters.Query()).Returns(asyncEnumerable);
    }

    [Fact]
    public async Task Unauthenticated_User_Throws_UnauthorizedException()
    {
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(false);
        _currentUserServiceMock.Setup(c => c.UserId).Returns((string?)null);

        var job = new Job { Id = 1, RecruiterId = 10 };

        var ex = await Assert.ThrowsAsync<UnauthorizedException>(() =>
            JobAuthorizationHelper.VerifyOwnershipOrAdminAsync(
                _unitOfWorkMock.Object, job, _currentUserServiceMock.Object, "update"));

        Assert.Equal("Authentication is required to perform this action.", ex.Message);
    }

    [Fact]
    public async Task Admin_Can_Perform_Any_Operation_Without_Owning_Job()
    {
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns("admin-user-id");
        _currentUserServiceMock.Setup(c => c.Roles).Returns(new[] { "Admin" });

        var job = new Job { Id = 1, RecruiterId = 10 };

        // Should not throw
        await JobAuthorizationHelper.VerifyOwnershipOrAdminAsync(
            _unitOfWorkMock.Object, job, _currentUserServiceMock.Object, "close");
    }

    [Fact]
    public async Task Owning_Recruiter_Can_Perform_Operation()
    {
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns("recruiter-user-id");
        _currentUserServiceMock.Setup(c => c.Roles).Returns(new[] { "Recruiter" });

        var recruiter = new Recruiter { Id = 10, UserId = "recruiter-user-id" };
        SetupRecruiters(new[] { recruiter });

        var job = new Job { Id = 1, RecruiterId = 10 };

        // Should not throw
        await JobAuthorizationHelper.VerifyOwnershipOrAdminAsync(
            _unitOfWorkMock.Object, job, _currentUserServiceMock.Object, "update");
    }

    [Fact]
    public async Task Stranger_Recruiter_Is_Denied_With_ForbiddenException()
    {
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns("recruiter-attacker");
        _currentUserServiceMock.Setup(c => c.Roles).Returns(new[] { "Recruiter" });

        var attackerRecruiter = new Recruiter { Id = 99, UserId = "recruiter-attacker" };
        SetupRecruiters(new[] { attackerRecruiter });

        var job = new Job { Id = 1, RecruiterId = 10 };

        var ex = await Assert.ThrowsAsync<ForbiddenException>(() =>
            JobAuthorizationHelper.VerifyOwnershipOrAdminAsync(
                _unitOfWorkMock.Object, job, _currentUserServiceMock.Object, "update"));

        Assert.Equal("You do not have permission to update this job.", ex.Message);
    }

    [Fact]
    public async Task Non_Recruiter_Non_Admin_Is_Denied_With_ForbiddenException()
    {
        _currentUserServiceMock.Setup(c => c.IsAuthenticated).Returns(true);
        _currentUserServiceMock.Setup(c => c.UserId).Returns("candidate-user");
        _currentUserServiceMock.Setup(c => c.Roles).Returns(new[] { "Candidate" });

        SetupRecruiters(Enumerable.Empty<Recruiter>());

        var job = new Job { Id = 1, RecruiterId = 10 };

        var ex = await Assert.ThrowsAsync<ForbiddenException>(() =>
            JobAuthorizationHelper.VerifyOwnershipOrAdminAsync(
                _unitOfWorkMock.Object, job, _currentUserServiceMock.Object, "delete"));

        Assert.Equal("You do not have permission to delete this job.", ex.Message);
    }
}
