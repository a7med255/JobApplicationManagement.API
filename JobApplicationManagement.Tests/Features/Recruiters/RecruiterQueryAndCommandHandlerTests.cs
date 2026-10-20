using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Recruiters.Commands;
using JobApplicationManagement.Application.Features.Recruiters.DTOs;
using JobApplicationManagement.Application.Features.Recruiters.Queries;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Tests.Features.SavedJobs;
using Moq;
using Xunit;

namespace JobApplicationManagement.Tests.Features.Recruiters;

public class RecruiterQueryAndCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IMapper> _mapperMock;

    public RecruiterQueryAndCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _mapperMock = new Mock<IMapper>();

        _mapperMock.Setup(m => m.Map(It.IsAny<UpdateRecruiterDto>(), It.IsAny<Recruiter>()))
            .Callback<object, object>((src, dest) =>
            {
                var s = (UpdateRecruiterDto)src;
                var d = (Recruiter)dest;
                d.Name = s.Name;
            });

        _mapperMock.Setup(m => m.Map<Recruiter>(It.IsAny<CreateRecruiterDto>()))
            .Returns<object>(src =>
            {
                var s = (CreateRecruiterDto)src;
                return new Recruiter { Name = s.Name, UserId = s.UserId };
            });
    }

    [Fact]
    public async Task GetMyProfile_Returns_Recruiter_When_Exists()
    {
        var recruiter = new Recruiter { Id = 1, UserId = "recruiter-123", Name = "Bob Recruiter" };
        _currentUserServiceMock.Setup(c => c.UserId).Returns("recruiter-123");
        SetupRecruiterQuery(new[] { recruiter });

        var handler = new GetMyRecruiterProfileQueryHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
        var result = await handler.Handle(new GetMyRecruiterProfileQuery(), CancellationToken.None);

        Assert.Equal("Bob Recruiter", result.Name);
        Assert.Equal("recruiter-123", result.UserId);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetMyProfile_Throws_NotFoundException_When_Not_Exists()
    {
        _currentUserServiceMock.Setup(c => c.UserId).Returns("recruiter-404");
        SetupRecruiterQuery(Array.Empty<Recruiter>());

        var handler = new GetMyRecruiterProfileQueryHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new GetMyRecruiterProfileQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task GetRecruiterById_Throws_Forbidden_When_Recruiter_Views_Other_Profile()
    {
        var recruiter = new Recruiter { Id = 10, UserId = "recruiter-owner", Name = "Alice" };
        _currentUserServiceMock.Setup(c => c.UserId).Returns("recruiter-stranger");
        _currentUserServiceMock.Setup(c => c.Roles).Returns(new[] { "Recruiter" });
        SetupRecruiterQuery(new[] { recruiter });

        var handler = new GetRecruiterByIdQueryHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(new GetRecruiterByIdQuery(10), CancellationToken.None));
    }

    [Fact]
    public async Task GetRecruiterById_Allows_Admin_To_View_Any_Profile()
    {
        var recruiter = new Recruiter { Id = 10, UserId = "recruiter-owner", Name = "Alice" };
        _currentUserServiceMock.Setup(c => c.UserId).Returns("admin-user");
        _currentUserServiceMock.Setup(c => c.Roles).Returns(new[] { "Admin" });
        SetupRecruiterQuery(new[] { recruiter });

        var handler = new GetRecruiterByIdQueryHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
        var result = await handler.Handle(new GetRecruiterByIdQuery(10), CancellationToken.None);

        Assert.Equal("Alice", result.Name);
        Assert.Equal(10, result.Id);
    }

    [Fact]
    public async Task GetRecruiterById_Allows_Owner_To_View_Own_Profile()
    {
        var recruiter = new Recruiter { Id = 10, UserId = "recruiter-owner", Name = "Alice" };
        _currentUserServiceMock.Setup(c => c.UserId).Returns("recruiter-owner");
        _currentUserServiceMock.Setup(c => c.Roles).Returns(new[] { "Recruiter" });
        SetupRecruiterQuery(new[] { recruiter });

        var handler = new GetRecruiterByIdQueryHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
        var result = await handler.Handle(new GetRecruiterByIdQuery(10), CancellationToken.None);

        Assert.Equal("Alice", result.Name);
        Assert.Equal("recruiter-owner", result.UserId);
    }

    [Fact]
    public async Task GetAllRecruiters_Returns_Paginated_Recruiters()
    {
        var recruiters = new[]
        {
            new Recruiter { Id = 1, UserId = "user-1", Name = "Recruiter One" },
            new Recruiter { Id = 2, UserId = "user-2", Name = "Recruiter Two" }
        };
        SetupRecruiterQuery(recruiters);

        var handler = new GetAllRecruitersQueryHandler(_unitOfWorkMock.Object);
        var result = await handler.Handle(new GetAllRecruitersQuery(1, 10), CancellationToken.None);

        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count());
        Assert.Equal("Recruiter One", result.Items.First().Name);
    }

    [Fact]
    public async Task UpdateRecruiter_Updates_Profile_When_Authorized()
    {
        var recruiter = new Recruiter { Id = 1, UserId = "recruiter-123", Name = "Old Name" };
        _currentUserServiceMock.Setup(c => c.UserId).Returns("recruiter-123");
        _currentUserServiceMock.Setup(c => c.Roles).Returns(new[] { "Recruiter" });

        _unitOfWorkMock.Setup(u => u.Recruiters.GetByIdAsync(1)).ReturnsAsync(recruiter);
        _unitOfWorkMock.Setup(u => u.Recruiters.Update(It.IsAny<Recruiter>()));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateRecruiterProfileCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object, _mapperMock.Object);
        var result = await handler.Handle(new UpdateRecruiterProfileCommand(1, new UpdateRecruiterDto { Name = "New Name" }), CancellationToken.None);

        Assert.Equal("New Name", result.Name);
        _unitOfWorkMock.Verify(u => u.Recruiters.Update(recruiter), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateRecruiter_Throws_Forbidden_When_Unauthorized()
    {
        var recruiter = new Recruiter { Id = 1, UserId = "recruiter-123", Name = "Old Name" };
        _currentUserServiceMock.Setup(c => c.UserId).Returns("stranger");
        _currentUserServiceMock.Setup(c => c.Roles).Returns(new[] { "Recruiter" });

        _unitOfWorkMock.Setup(u => u.Recruiters.GetByIdAsync(1)).ReturnsAsync(recruiter);

        var handler = new UpdateRecruiterProfileCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object, _mapperMock.Object);
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new UpdateRecruiterProfileCommand(1, new UpdateRecruiterDto { Name = "New Name" }), CancellationToken.None));
    }

    [Fact]
    public async Task DeleteRecruiter_Deletes_Profile_When_Authorized()
    {
        var recruiter = new Recruiter { Id = 1, UserId = "recruiter-123", Name = "Recruiter" };
        _currentUserServiceMock.Setup(c => c.UserId).Returns("recruiter-123");
        _currentUserServiceMock.Setup(c => c.Roles).Returns(new[] { "Recruiter" });

        _unitOfWorkMock.Setup(u => u.Recruiters.GetByIdAsync(1)).ReturnsAsync(recruiter);
        _unitOfWorkMock.Setup(u => u.Recruiters.Delete(recruiter));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new DeleteRecruiterProfileCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
        await handler.Handle(new DeleteRecruiterProfileCommand(1), CancellationToken.None);

        _unitOfWorkMock.Verify(u => u.Recruiters.Delete(recruiter), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteRecruiter_Throws_Forbidden_When_Unauthorized()
    {
        var recruiter = new Recruiter { Id = 1, UserId = "recruiter-123", Name = "Recruiter" };
        _currentUserServiceMock.Setup(c => c.UserId).Returns("stranger");
        _currentUserServiceMock.Setup(c => c.Roles).Returns(new[] { "Recruiter" });

        _unitOfWorkMock.Setup(u => u.Recruiters.GetByIdAsync(1)).ReturnsAsync(recruiter);

        var handler = new DeleteRecruiterProfileCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
        await Assert.ThrowsAsync<ForbiddenException>(() =>
            handler.Handle(new DeleteRecruiterProfileCommand(1), CancellationToken.None));
    }

    [Fact]
    public async Task CreateRecruiter_Creates_Recruiter_Successfully()
    {
        var createDto = new CreateRecruiterDto { Name = "Jane Recruiter", UserId = "user-jane" };
        _unitOfWorkMock.Setup(u => u.Recruiters.AddAsync(It.IsAny<Recruiter>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new CreateRecruiterCommandHandler(_unitOfWorkMock.Object, _mapperMock.Object);
        var result = await handler.Handle(new CreateRecruiterCommand(createDto), CancellationToken.None);

        Assert.Equal("Jane Recruiter", result.Name);
        Assert.Equal("user-jane", result.UserId);
        _unitOfWorkMock.Verify(u => u.Recruiters.AddAsync(It.IsAny<Recruiter>()), Times.Once);
    }

    private void SetupRecruiterQuery(IEnumerable<Recruiter> recruiters)
    {
        _unitOfWorkMock.Setup(u => u.Recruiters.Query())
            .Returns(new SavedJobTestAsyncEnumerable<Recruiter>(recruiters));
    }
}
