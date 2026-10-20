using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Notifications;

public record GetMyNotificationsQuery(int PageNumber = 1, int PageSize = 20) : IRequest<PaginatedResult<NotificationDto>>;
public record GetUnreadNotificationCountQuery : IRequest<int>;
public record GetMyNotificationPreferencesQuery : IRequest<NotificationPreferenceDto>;

public sealed class GetMyNotificationsQueryHandler : IRequestHandler<GetMyNotificationsQuery, PaginatedResult<NotificationDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    public GetMyNotificationsQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        => (_unitOfWork, _currentUser) = (unitOfWork, currentUser);

    public async Task<PaginatedResult<NotificationDto>> Handle(GetMyNotificationsQuery request, CancellationToken cancellationToken)
    {
        var page = Math.Max(1, request.PageNumber);
        var size = Math.Clamp(request.PageSize, 1, 50);
        var query = _unitOfWork.Notifications.Query().AsNoTracking()
            .Where(x => x.UserId == _currentUser.UserId);
        var count = await query.CountAsync(cancellationToken);
        var items = await query.OrderByDescending(x => x.CreatedAt).Skip((page - 1) * size).Take(size)
            .Select(x => new NotificationDto
            {
                Id = x.Id, Type = x.Type, Title = x.Title, Message = x.Message,
                RelatedEntityId = x.RelatedEntityId, RelatedEntityType = x.RelatedEntityType,
                IsRead = x.IsRead, CreatedAt = x.CreatedAt, ReadAt = x.ReadAt
            }).ToListAsync(cancellationToken);
        return new PaginatedResult<NotificationDto> { Items = items, TotalCount = count, PageNumber = page, PageSize = size };
    }
}

public sealed class GetUnreadNotificationCountQueryHandler : IRequestHandler<GetUnreadNotificationCountQuery, int>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    public GetUnreadNotificationCountQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        => (_unitOfWork, _currentUser) = (unitOfWork, currentUser);
    public Task<int> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken) =>
        _unitOfWork.Notifications.Query().AsNoTracking()
            .CountAsync(x => x.UserId == _currentUser.UserId && !x.IsRead, cancellationToken);
}

public sealed class GetMyNotificationPreferencesQueryHandler : IRequestHandler<GetMyNotificationPreferencesQuery, NotificationPreferenceDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    public GetMyNotificationPreferencesQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        => (_unitOfWork, _currentUser) = (unitOfWork, currentUser);
    public async Task<NotificationPreferenceDto> Handle(GetMyNotificationPreferencesQuery request, CancellationToken cancellationToken)
    {
        var value = await _unitOfWork.NotificationPreferences.Query().AsNoTracking()
            .FirstOrDefaultAsync(x => x.UserId == _currentUser.UserId, cancellationToken);
        return value is null
            ? new NotificationPreferenceDto(true, true, true)
            : new NotificationPreferenceDto(value.InAppEnabled, value.EmailEnabled, value.JobAlertsEnabled);
    }
}
