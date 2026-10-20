using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Candidates.Commands;
using JobApplicationManagement.Application.Features.Candidates.DTOs;
using JobApplicationManagement.Application.Features.Candidates.Mappings;
using JobApplicationManagement.Application.Features.Candidates.Queries;
using JobApplicationManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;

namespace JobApplicationManagement.Tests.Features.Candidates;

public class CandidateQueryAndCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IMapper> _mapperMock;

    public CandidateQueryAndCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _mapperMock = new Mock<IMapper>();

        _mapperMock.Setup(m => m.Map(It.IsAny<UpdateCandidateDto>(), It.IsAny<Candidate>()))
            .Callback<object, object>((src, dest) =>
            {
                var s = (UpdateCandidateDto)src;
                var d = (Candidate)dest;
                d.Name = s.Name;
                d.CvUrl = s.CvUrl;
            });
    }

    [Fact]
    public async Task GetMyProfile_Returns_Candidate_When_Exists()
    {
        var candidate = new Candidate { Id = 1, UserId = "user-123", Name = "Alice", CvUrl = "https://example.com/cv.pdf" };
        _currentUserServiceMock.Setup(c => c.UserId).Returns("user-123");
        SetupCandidateQuery(new[] { candidate });

        var handler = new GetMyCandidateProfileQueryHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
        var result = await handler.Handle(new GetMyCandidateProfileQuery(), CancellationToken.None);

        Assert.Equal("Alice", result.Name);
        Assert.Equal("user-123", result.UserId);
    }

    [Fact]
    public async Task GetMyProfile_Throws_NotFoundException_When_Not_Exists()
    {
        _currentUserServiceMock.Setup(c => c.UserId).Returns("user-404");
        SetupCandidateQuery(Array.Empty<Candidate>());

        var handler = new GetMyCandidateProfileQueryHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
        await Assert.ThrowsAsync<NotFoundException>(() => handler.Handle(new GetMyCandidateProfileQuery(), CancellationToken.None));
    }

    [Fact]
    public async Task GetCandidateById_Throws_Forbidden_When_Candidate_Views_Other_Profile()
    {
        var candidate = new Candidate { Id = 5, UserId = "user-owner", Name = "Bob" };
        _currentUserServiceMock.Setup(c => c.UserId).Returns("user-stranger");
        _currentUserServiceMock.Setup(c => c.Roles).Returns(new[] { "Candidate" });
        SetupCandidateQuery(new[] { candidate });

        var handler = new GetCandidateByIdQueryHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
        await Assert.ThrowsAsync<ForbiddenException>(() => handler.Handle(new GetCandidateByIdQuery(5), CancellationToken.None));
    }

    [Fact]
    public async Task GetCandidateById_Allows_Admin_To_View_Any_Profile()
    {
        var candidate = new Candidate { Id = 5, UserId = "user-owner", Name = "Bob" };
        _currentUserServiceMock.Setup(c => c.UserId).Returns("admin-user");
        _currentUserServiceMock.Setup(c => c.Roles).Returns(new[] { "Admin" });
        SetupCandidateQuery(new[] { candidate });

        var handler = new GetCandidateByIdQueryHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
        var result = await handler.Handle(new GetCandidateByIdQuery(5), CancellationToken.None);

        Assert.Equal("Bob", result.Name);
    }

    [Fact]
    public async Task UpdateCandidate_Updates_Profile_When_Authorized()
    {
        var candidate = new Candidate { Id = 1, UserId = "user-123", Name = "Alice Old", CvUrl = "" };
        _currentUserServiceMock.Setup(c => c.UserId).Returns("user-123");
        _currentUserServiceMock.Setup(c => c.Roles).Returns(new[] { "Candidate" });

        _unitOfWorkMock.Setup(u => u.Candidates.GetByIdAsync(1)).ReturnsAsync(candidate);
        _unitOfWorkMock.Setup(u => u.Candidates.Update(It.IsAny<Candidate>()));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var handler = new UpdateCandidateProfileCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object, _mapperMock.Object);
        var result = await handler.Handle(new UpdateCandidateProfileCommand(1, new UpdateCandidateDto { Name = "Alice New", CvUrl = "https://cv.pdf" }), CancellationToken.None);

        Assert.Equal("Alice New", result.Name);
        Assert.Equal("https://cv.pdf", result.CvUrl);
    }

    private void SetupCandidateQuery(Candidate[] candidates)
    {
        var queryable = candidates.AsQueryable();
        var mockSet = CreateMockDbSet(queryable);
        _unitOfWorkMock.Setup(u => u.Candidates.Query()).Returns(mockSet.Object);
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> elements) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new CandidateTestAsyncQueryProvider<T>(elements.Provider));
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(elements.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(elements.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => elements.GetEnumerator());
        mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new CandidateTestAsyncEnumerator<T>(elements.GetEnumerator()));
        return mockSet;
    }
}

internal class CandidateTestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal CandidateTestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

    public IQueryable CreateQuery(Expression expression) => new CandidateTestAsyncEnumerable<TEntity>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new CandidateTestAsyncEnumerable<TElement>(expression);

    public object Execute(Expression expression) => _inner.Execute(expression)!;

    public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var expectedResultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(nameof(IQueryProvider.Execute), 1, new[] { typeof(Expression) })!
            .MakeGenericMethod(expectedResultType)
            .Invoke(this, new[] { expression });

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(expectedResultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

internal class CandidateTestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public CandidateTestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
    public CandidateTestAsyncEnumerable(Expression expression) : base(expression) { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) =>
        new CandidateTestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

    IQueryProvider IQueryable.Provider => new CandidateTestAsyncQueryProvider<T>(this);
}

internal class CandidateTestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;
    public CandidateTestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;
    public ValueTask DisposeAsync() { _inner.Dispose(); return ValueTask.CompletedTask; }
    public ValueTask<bool> MoveNextAsync() => ValueTask.FromResult(_inner.MoveNext());
    public T Current => _inner.Current;
}
