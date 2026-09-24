using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Applications.Services;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace JobApplicationManagement.Tests.Features.Applications.Services;

public class StaleApplicationCleanupJobTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ILogger<StaleApplicationCleanupJob>> _loggerMock;
    private readonly StaleApplicationCleanupJob _job;

    public StaleApplicationCleanupJobTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _loggerMock = new Mock<ILogger<StaleApplicationCleanupJob>>();
        _job = new StaleApplicationCleanupJob(_unitOfWorkMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task CloseStaleApplicationsAsync_ShouldDoNothing_WhenNoStaleApplicationsExist()
    {
        // Arrange: Application updated recently (5 days ago)
        var freshApplication = new JobApplication
        {
            Id = 1,
            JobApplicationStatus = JobApplicationStatus.Applied,
            StatusUpdatedAt = DateTime.UtcNow.AddDays(-5)
        };

        var applications = new List<JobApplication> { freshApplication }.AsQueryable();
        var mockSet = CreateMockDbSet(applications);
        _unitOfWorkMock.Setup(u => u.JobApplications.Query()).Returns(mockSet.Object);

        // Act
        await _job.CloseStaleApplicationsAsync(CancellationToken.None);

        // Assert
        _unitOfWorkMock.Verify(u => u.JobApplications.Update(It.IsAny<JobApplication>()), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task CloseStaleApplicationsAsync_ShouldUpdateStaleApplicationsToRejected_WhenOlderThan30Days()
    {
        // Arrange: 1 stale (35 days ago in Applied), 1 stale (40 days ago in UnderReview), 1 fresh (5 days ago)
        var staleApplied = new JobApplication
        {
            Id = 1,
            JobApplicationStatus = JobApplicationStatus.Applied,
            StatusUpdatedAt = DateTime.UtcNow.AddDays(-35)
        };

        var staleUnderReview = new JobApplication
        {
            Id = 2,
            JobApplicationStatus = JobApplicationStatus.UnderReview,
            StatusUpdatedAt = DateTime.UtcNow.AddDays(-40)
        };

        var freshApplication = new JobApplication
        {
            Id = 3,
            JobApplicationStatus = JobApplicationStatus.Applied,
            StatusUpdatedAt = DateTime.UtcNow.AddDays(-5)
        };

        var applications = new List<JobApplication> { staleApplied, staleUnderReview, freshApplication }.AsQueryable();
        var mockSet = CreateMockDbSet(applications);
        _unitOfWorkMock.Setup(u => u.JobApplications.Query()).Returns(mockSet.Object);
        _unitOfWorkMock.Setup(u => u.JobApplications.Update(It.IsAny<JobApplication>()));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(2);

        // Act
        await _job.CloseStaleApplicationsAsync(CancellationToken.None);

        // Assert
        Assert.Equal(JobApplicationStatus.Rejected, staleApplied.JobApplicationStatus);
        Assert.Equal(JobApplicationStatus.Rejected, staleUnderReview.JobApplicationStatus);
        Assert.Equal(JobApplicationStatus.Applied, freshApplication.JobApplicationStatus);

        _unitOfWorkMock.Verify(u => u.JobApplications.Update(It.Is<JobApplication>(a => a.Id == 1 && a.JobApplicationStatus == JobApplicationStatus.Rejected)), Times.Once);
        _unitOfWorkMock.Verify(u => u.JobApplications.Update(It.Is<JobApplication>(a => a.Id == 2 && a.JobApplicationStatus == JobApplicationStatus.Rejected)), Times.Once);
        _unitOfWorkMock.Verify(u => u.JobApplications.Update(It.Is<JobApplication>(a => a.Id == 3)), Times.Never);
        _unitOfWorkMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(IQueryable<T> elements) where T : class
    {
        var mockSet = new Mock<DbSet<T>>();
        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(new TestAsyncQueryProvider<T>(elements.Provider));
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(elements.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(elements.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(() => elements.GetEnumerator());
        mockSet.As<IAsyncEnumerable<T>>().Setup(m => m.GetAsyncEnumerator(It.IsAny<CancellationToken>())).Returns(new TestAsyncEnumerator<T>(elements.GetEnumerator()));
        return mockSet;
    }
}

internal class TestAsyncQueryProvider<TEntity> : IAsyncQueryProvider
{
    private readonly IQueryProvider _inner;

    internal TestAsyncQueryProvider(IQueryProvider inner)
    {
        _inner = inner;
    }

    public IQueryable CreateQuery(Expression expression)
    {
        return new TestAsyncEnumerable<TEntity>(expression);
    }

    public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
    {
        return new TestAsyncEnumerable<TElement>(expression);
    }

    public object Execute(Expression expression)
    {
        return _inner.Execute(expression)!;
    }

    public TResult Execute<TResult>(Expression expression)
    {
        return _inner.Execute<TResult>(expression);
    }

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

internal class TestAsyncEnumerable<T> : EnumerableQuery<T>, IAsyncEnumerable<T>, IQueryable<T>
{
    public TestAsyncEnumerable(IEnumerable<T> enumerable)
        : base(enumerable)
    { }

    public TestAsyncEnumerable(Expression expression)
        : base(expression)
    { }

    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
    {
        return new TestAsyncEnumerator<T>(this.AsEnumerable().GetEnumerator());
    }

    IQueryProvider IQueryable.Provider
    {
        get { return new TestAsyncQueryProvider<T>(this); }
    }
}

internal class TestAsyncEnumerator<T> : IAsyncEnumerator<T>
{
    private readonly IEnumerator<T> _inner;

    public TestAsyncEnumerator(IEnumerator<T> inner)
    {
        _inner = inner;
    }

    public ValueTask DisposeAsync()
    {
        _inner.Dispose();
        return new ValueTask();
    }

    public ValueTask<bool> MoveNextAsync()
    {
        return new ValueTask<bool>(_inner.MoveNext());
    }

    public T Current
    {
        get { return _inner.Current; }
    }
}
