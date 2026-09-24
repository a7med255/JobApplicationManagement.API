using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.SavedJobs.Commands;
using JobApplicationManagement.Application.Features.SavedJobs.Queries;
using JobApplicationManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JobApplicationManagement.Tests.Features.SavedJobs;

public class SavedJobCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<ILogger<SaveJobCommandHandler>> _saveLoggerMock;
    private readonly Mock<ILogger<RemoveSavedJobCommandHandler>> _removeLoggerMock;
    private readonly SaveJobCommandHandler _saveHandler;
    private readonly RemoveSavedJobCommandHandler _removeHandler;

    public SavedJobCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _saveLoggerMock = new Mock<ILogger<SaveJobCommandHandler>>();
        _removeLoggerMock = new Mock<ILogger<RemoveSavedJobCommandHandler>>();
        _saveHandler = new SaveJobCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object, _saveLoggerMock.Object);
        _removeHandler = new RemoveSavedJobCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object, _removeLoggerMock.Object);
    }

    // ── Save Job Tests ──────────────────────────────────────────────────────────

    [Fact]
    public async Task Candidate_Can_Save_Job()
    {
        // Arrange
        var candidate = new Candidate { Id = 1, UserId = "user-1", Name = "Test" };
        var job = new Job { Id = 10, Title = "Developer", Description = "Build things", IsActive = true };

        SetupCandidate(candidate);
        SetupJobQuery(new[] { job });
        SetupSavedJobQuery(Array.Empty<SavedJob>());

        _unitOfWorkMock.Setup(u => u.SavedJobs.AddAsync(It.IsAny<SavedJob>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        var result = await _saveHandler.Handle(new SaveJobCommand(10), CancellationToken.None);

        // Assert
        Assert.Equal(10, result.JobId);
        Assert.Equal("Developer", result.JobTitle);
        _unitOfWorkMock.Verify(u => u.SavedJobs.AddAsync(It.Is<SavedJob>(sj => sj.CandidateId == 1 && sj.JobId == 10)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Candidate_Cannot_Save_Same_Job_Twice()
    {
        // Arrange
        var candidate = new Candidate { Id = 1, UserId = "user-1" };
        var job = new Job { Id = 10, Title = "Dev", Description = "Desc", IsActive = true };

        SetupCandidate(candidate);
        SetupJobQuery(new[] { job });
        SetupSavedJobQuery(new[] { new SavedJob { Id = 1, CandidateId = 1, JobId = 10, CreatedAt = DateTime.UtcNow } });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(() => _saveHandler.Handle(new SaveJobCommand(10), CancellationToken.None));
        Assert.Contains("already saved", ex.Message);
    }

    [Fact]
    public async Task Save_NonExisting_Job_Fails()
    {
        // Arrange
        var candidate = new Candidate { Id = 1, UserId = "user-1" };
        SetupCandidate(candidate);
        SetupJobQuery(Array.Empty<Job>());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _saveHandler.Handle(new SaveJobCommand(999), CancellationToken.None));
    }

    [Fact]
    public async Task Recruiter_Cannot_Save_Job_As_Candidate()
    {
        // Arrange — user exists in Identity but has no Candidate profile
        _currentUserServiceMock.Setup(c => c.UserId).Returns("recruiter-user-1");

        var candidates = Array.Empty<Candidate>().AsQueryable();
        var mockSet = CreateMockDbSet(candidates);
        _unitOfWorkMock.Setup(u => u.Candidates.Query()).Returns(mockSet.Object);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(() => _saveHandler.Handle(new SaveJobCommand(10), CancellationToken.None));
        Assert.Contains("Only registered candidates", ex.Message);
    }

    [Fact]
    public async Task Saving_Job_Does_Not_Create_Application()
    {
        // Arrange
        var candidate = new Candidate { Id = 1, UserId = "user-1" };
        var job = new Job { Id = 10, Title = "Dev", Description = "Desc", IsActive = true };

        SetupCandidate(candidate);
        SetupJobQuery(new[] { job });
        SetupSavedJobQuery(Array.Empty<SavedJob>());

        _unitOfWorkMock.Setup(u => u.SavedJobs.AddAsync(It.IsAny<SavedJob>())).Returns(Task.CompletedTask);
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _saveHandler.Handle(new SaveJobCommand(10), CancellationToken.None);

        // Assert — JobApplications.AddAsync should NEVER be called
        _unitOfWorkMock.Verify(u => u.JobApplications.AddAsync(It.IsAny<JobApplication>()), Times.Never);
    }

    // ── Remove Saved Job Tests ──────────────────────────────────────────────────

    [Fact]
    public async Task Candidate_Can_Remove_Saved_Job()
    {
        // Arrange
        var candidate = new Candidate { Id = 1, UserId = "user-1" };
        var savedJob = new SavedJob { Id = 5, CandidateId = 1, JobId = 10, CreatedAt = DateTime.UtcNow };

        SetupCandidate(candidate);
        SetupSavedJobQuery(new[] { savedJob });

        _unitOfWorkMock.Setup(u => u.SavedJobs.Delete(It.IsAny<SavedJob>()));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _removeHandler.Handle(new RemoveSavedJobCommand(10), CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(u => u.SavedJobs.Delete(It.Is<SavedJob>(sj => sj.Id == 5)), Times.Once);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Candidate_Cannot_Remove_Another_Candidates_Saved_Job()
    {
        // Arrange — candidate 2 tries to remove candidate 1's saved job
        var candidate2 = new Candidate { Id = 2, UserId = "user-2" };
        SetupCandidate(candidate2);

        // SavedJob belongs to CandidateId=1, but query is scoped to CandidateId=2, so it won't be found
        SetupSavedJobQuery(Array.Empty<SavedJob>());

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _removeHandler.Handle(new RemoveSavedJobCommand(10), CancellationToken.None));
    }

    [Fact]
    public async Task Removing_Saved_Job_Does_Not_Delete_Application()
    {
        // Arrange
        var candidate = new Candidate { Id = 1, UserId = "user-1" };
        var savedJob = new SavedJob { Id = 5, CandidateId = 1, JobId = 10, CreatedAt = DateTime.UtcNow };

        SetupCandidate(candidate);
        SetupSavedJobQuery(new[] { savedJob });

        _unitOfWorkMock.Setup(u => u.SavedJobs.Delete(It.IsAny<SavedJob>()));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        // Act
        await _removeHandler.Handle(new RemoveSavedJobCommand(10), CancellationToken.None);

        // Assert — JobApplications.Delete should NEVER be called
        _unitOfWorkMock.Verify(u => u.JobApplications.Delete(It.IsAny<JobApplication>()), Times.Never);
    }

    // ── Helper Methods ──────────────────────────────────────────────────────────

    private void SetupCandidate(Candidate candidate)
    {
        _currentUserServiceMock.Setup(c => c.UserId).Returns(candidate.UserId);
        var candidates = new[] { candidate }.AsQueryable();
        var mockSet = CreateMockDbSet(candidates);
        _unitOfWorkMock.Setup(u => u.Candidates.Query()).Returns(mockSet.Object);
    }

    private void SetupJobQuery(Job[] jobs)
    {
        var queryable = jobs.AsQueryable();
        var mockSet = CreateMockDbSet(queryable);
        _unitOfWorkMock.Setup(u => u.Jobs.Query()).Returns(mockSet.Object);
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
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new SavedJobTestAsyncQueryProvider<T>(elements.Provider));
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(elements.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(elements.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => elements.GetEnumerator());
        mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new SavedJobTestAsyncEnumerator<T>(elements.GetEnumerator()));
        return mockSet;
    }
}

// ── Async query infrastructure (reused from existing test patterns) ─────────

internal class SavedJobTestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal SavedJobTestAsyncQueryProvider(IQueryProvider inner) => _inner = inner;

    public IQueryable CreateQuery(Expression expression) => new SavedJobTestAsyncEnumerable<TEntity>(expression);

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => new SavedJobTestAsyncEnumerable<TElement>(expression);

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

internal class SavedJobTestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public SavedJobTestAsyncEnumerable(IEnumerable<T> enumerable) : base(enumerable) { }
    public SavedJobTestAsyncEnumerable(Expression expression) : base(expression) { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        => new SavedJobTestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());

    IQueryProvider IQueryable.Provider => new SavedJobTestAsyncQueryProvider<T>(this);
}

internal class SavedJobTestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public SavedJobTestAsyncEnumerator(IEnumerator<T> inner) => _inner = inner;

    public ValueTask DisposeAsync() { _inner.Dispose(); return new ValueTask(); }

    public ValueTask<bool> MoveNextAsync() => new ValueTask<bool>(_inner.MoveNext());

    public T Current => _inner.Current;
}
