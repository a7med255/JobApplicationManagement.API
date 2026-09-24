using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.SavedJobs.Queries;
using JobApplicationManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;

namespace JobApplicationManagement.Tests.Features.SavedJobs;

public class SavedJobQueryHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public SavedJobQueryHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task Candidate_Can_Get_Own_Saved_Jobs()
    {
        // Arrange
        var candidate = new Candidate { Id = 1, UserId = "user-1" };
        SetupCandidate(candidate);

        var savedJobs = new[]
        {
            new SavedJob
            {
                Id = 1, CandidateId = 1, JobId = 10, CreatedAt = DateTime.UtcNow,
                Job = new Job { Id = 10, Title = "Developer", Description = "Dev job", IsActive = true }
            },
            new SavedJob
            {
                Id = 2, CandidateId = 1, JobId = 20, CreatedAt = DateTime.UtcNow.AddMinutes(-5),
                Job = new Job { Id = 20, Title = "Designer", Description = "Design job", IsActive = false }
            }
        };
        SetupSavedJobQuery(savedJobs);

        var handler = new GetMySavedJobsQueryHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);

        // Act
        var result = await handler.Handle(new GetMySavedJobsQuery(1, 10), CancellationToken.None);

        // Assert
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(2, result.Items.Count());
    }

    [Fact]
    public async Task Candidate_Cannot_Get_Another_Candidates_Saved_Jobs()
    {
        // Arrange — Candidate 2 queries saved jobs, but only Candidate 1's saved jobs exist
        var candidate2 = new Candidate { Id = 2, UserId = "user-2" };
        SetupCandidate(candidate2);

        // Saved jobs all belong to CandidateId=1
        var savedJobs = new[]
        {
            new SavedJob
            {
                Id = 1, CandidateId = 1, JobId = 10, CreatedAt = DateTime.UtcNow,
                Job = new Job { Id = 10, Title = "Developer", Description = "Dev job", IsActive = true }
            }
        };
        SetupSavedJobQuery(savedJobs);

        var handler = new GetMySavedJobsQueryHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);

        // Act
        var result = await handler.Handle(new GetMySavedJobsQuery(1, 10), CancellationToken.None);

        // Assert — Candidate 2 should see no saved jobs (they belong to Candidate 1)
        Assert.Equal(0, result.TotalCount);
        Assert.Empty(result.Items);
    }

    [Fact]
    public async Task IsJobSaved_Returns_True_When_Job_Is_Saved()
    {
        // Arrange
        var candidate = new Candidate { Id = 1, UserId = "user-1" };
        SetupCandidate(candidate);
        SetupSavedJobQuery(new[] { new SavedJob { Id = 1, CandidateId = 1, JobId = 10, CreatedAt = DateTime.UtcNow } });

        var handler = new IsJobSavedQueryHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);

        // Act
        var result = await handler.Handle(new IsJobSavedQuery(10), CancellationToken.None);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task IsJobSaved_Returns_False_When_Job_Is_Not_Saved()
    {
        // Arrange
        var candidate = new Candidate { Id = 1, UserId = "user-1" };
        SetupCandidate(candidate);
        SetupSavedJobQuery(Array.Empty<SavedJob>());

        var handler = new IsJobSavedQueryHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);

        // Act
        var result = await handler.Handle(new IsJobSavedQuery(99), CancellationToken.None);

        // Assert
        Assert.False(result);
    }

    // ── Helper Methods ──────────────────────────────────────────────────────────

    private void SetupCandidate(Candidate candidate)
    {
        _currentUserServiceMock.Setup(c => c.UserId).Returns(candidate.UserId);
        var candidates = new[] { candidate }.AsQueryable();
        var mockSet = CreateMockDbSet(candidates);
        _unitOfWorkMock.Setup(u => u.Candidates.Query()).Returns(mockSet.Object);
    }

    private void SetupSavedJobQuery(SavedJob[] savedJobs)
    {
        var queryable = savedJobs.AsQueryable();
        var mockSet = CreateMockDbSet(queryable);
        _unitOfWorkMock.Setup(u => u.SavedJobs.Query()).Returns(mockSet.Object);
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> elements) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new QueryTestAsyncQueryProvider<T>(elements.Provider));
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(elements.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(elements.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => elements.GetEnumerator());
        mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new QueryTestAsyncEnumerator<T>(elements.GetEnumerator()));
        return mockSet;
    }
}

// ── Async query infrastructure ──────────────────────────────────────────────

internal class QueryTestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal QueryTestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

    public IQueryable CreateQuery(Expression expression) => new QueryTestAsyncEnumerable<TEntity>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new QueryTestAsyncEnumerable<TElement>(expression);

    public object Execute(Expression expression) => _inner.Execute(expression)!;

    public TResult Execute<TResult>(Expression expression) => _inner.Execute<TResult>(expression);

    public TResult ExecuteAsync<TResult>(Expression expression, CancellationToken cancellationToken = default)
    {
        var expectedResultType = typeof(TResult).GetGenericArguments()[0];
        var executionResult = typeof(IQueryProvider)
            .GetMethod(
                name: nameof(IQueryProvider.Execute),
                genericParameterCount: 1,
                types: new[] { typeof(Expression) })!
            .MakeGenericMethod(expectedResultType)
            .Invoke(this, new[] { expression });

        return (TResult)typeof(Task).GetMethod(nameof(Task.FromResult))!
            .MakeGenericMethod(expectedResultType)
            .Invoke(null, new[] { executionResult })!;
    }
}

internal class QueryTestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public QueryTestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
    public QueryTestAsyncEnumerable(Expression expression) : base(expression) { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new QueryTestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

    IQueryProvider IQueryable.Provider => new QueryTestAsyncQueryProvider<T>(this);
}

internal class QueryTestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public QueryTestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;

    public ValueTask DisposeAsync() { _inner.Dispose(); return new ValueTask(); }

    public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());

    public T Current => _inner.Current;
}
