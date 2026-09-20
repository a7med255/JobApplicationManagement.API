using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Features.Applications.Commands;
using JobApplicationManagement.Domain.Entities;
using JobApplicationManagement.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Moq;
using Xunit;

namespace JobApplicationManagement.Tests.Features.Applications.Commands;

public class CancelApplicationCommandHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CancelApplicationCommandHandler _handler;

    public CancelApplicationCommandHandlerTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _handler = new CancelApplicationCommandHandler(_unitOfWorkMock.Object, _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_ShouldThrowNotFoundException_WhenApplicationDoesNotExist()
    {
        // Arrange
        _unitOfWorkMock.Setup(u => u.JobApplications.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((JobApplication?)null);

        var command = new CancelApplicationCommand(1);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenException_WhenCandidateProfileNotFound()
    {
        // Arrange
        var application = new JobApplication { Id = 1, CandidateId = 99 };
        
        _unitOfWorkMock.Setup(u => u.JobApplications.GetByIdAsync(1))
            .ReturnsAsync(application);

        _currentUserServiceMock.Setup(c => c.UserId).Returns("user-123");

        var candidates = Array.Empty<Candidate>().AsQueryable();
        var mockSet = CreateMockDbSet(candidates);
        _unitOfWorkMock.Setup(u => u.Candidates.Query()).Returns(mockSet.Object);

        var command = new CancelApplicationCommand(1);

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_ShouldThrowForbiddenException_WhenCandidateDoesNotOwnApplication()
    {
        // Arrange
        var application = new JobApplication { Id = 1, CandidateId = 99 };
        
        _unitOfWorkMock.Setup(u => u.JobApplications.GetByIdAsync(1))
            .ReturnsAsync(application);

        _currentUserServiceMock.Setup(c => c.UserId).Returns("user-123");

        var candidates = new[] { new Candidate { Id = 100, UserId = "user-123" } }.AsQueryable();
        var mockSet = CreateMockDbSet(candidates);
        _unitOfWorkMock.Setup(u => u.Candidates.Query()).Returns(mockSet.Object);

        var command = new CancelApplicationCommand(1);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ForbiddenException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Equal("You do not have permission to cancel this application.", ex.Message);
    }

    [Theory]
    [InlineData(JobApplicationStatus.Accepted)]
    [InlineData(JobApplicationStatus.Rejected)]
    [InlineData(JobApplicationStatus.Cancelled)]
    public async Task Handle_ShouldThrowConflictException_WhenStatusIsNotAppliedOrUnderReview(JobApplicationStatus status)
    {
        // Arrange
        var application = new JobApplication { Id = 1, CandidateId = 100, JobApplicationStatus = status };
        
        _unitOfWorkMock.Setup(u => u.JobApplications.GetByIdAsync(1))
            .ReturnsAsync(application);

        _currentUserServiceMock.Setup(c => c.UserId).Returns("user-123");

        var candidates = new[] { new Candidate { Id = 100, UserId = "user-123" } }.AsQueryable();
        var mockSet = CreateMockDbSet(candidates);
        _unitOfWorkMock.Setup(u => u.Candidates.Query()).Returns(mockSet.Object);

        var command = new CancelApplicationCommand(1);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ConflictException>(() => _handler.Handle(command, CancellationToken.None));
        Assert.Contains("Only applications with status 'Applied' or 'UnderReview' can be cancelled.", ex.Message);
    }

    [Theory]
    [InlineData(JobApplicationStatus.Applied)]
    [InlineData(JobApplicationStatus.UnderReview)]
    public async Task Handle_ShouldCancelApplication_WhenValidationPasses(JobApplicationStatus status)
    {
        // Arrange
        var application = new JobApplication { Id = 1, CandidateId = 100, JobApplicationStatus = status };
        
        _unitOfWorkMock.Setup(u => u.JobApplications.GetByIdAsync(1))
            .ReturnsAsync(application);

        _currentUserServiceMock.Setup(c => c.UserId).Returns("user-123");

        var candidates = new[] { new Candidate { Id = 100, UserId = "user-123" } }.AsQueryable();
        var mockSet = CreateMockDbSet(candidates);
        _unitOfWorkMock.Setup(u => u.Candidates.Query()).Returns(mockSet.Object);

        _unitOfWorkMock.Setup(u => u.JobApplications.Update(It.IsAny<JobApplication>()));
        _unitOfWorkMock.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var command = new CancelApplicationCommand(1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(JobApplicationStatus.Cancelled, application.JobApplicationStatus);
        Assert.NotNull(application.CancelledAt);
        Assert.NotEqual(default, application.StatusUpdatedAt);

        _unitOfWorkMock.Verify(u => u.JobApplications.Update(It.Is<JobApplication>(a => a.Id == 1 && a.JobApplicationStatus == JobApplicationStatus.Cancelled)), Times.Once);
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
