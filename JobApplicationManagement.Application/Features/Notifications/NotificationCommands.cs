using JobApplicationManagement.Application.Common.Exceptions;
using JobApplicationManagement.Application.Common.Interfaces;
using JobApplicationManagement.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace JobApplicationManagement.Application.Features.Notifications;

public record MarkNotificationReadCommand(int Id) : IRequest;
public record MarkAllNotificationsReadCommand : IRequest;
public record UpdateNotificationPreferencesCommand(NotificationPreferenceDto Preferences) : IRequest<NotificationPreferenceDto>;

public sealed class MarkNotificationReadCommandHandler : IRequestHandler<MarkNotificationReadCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    public MarkNotificationReadCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        => (_unitOfWork, _currentUser) = (unitOfWork, currentUser);
    public async Task Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
    {
        var item = await _unitOfWork.Notifications.Query()
            .FirstOrDefaultAsync(x => x.Id == request.Id && x.UserId == _currentUser.UserId, cancellationToken)
            ?? throw new NotFoundException(nameof(Notification), request.Id);
        if (!item.IsRead)
        {
            item.IsRead = true;
            item.ReadAt = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
    }
}

public sealed class MarkAllNotificationsReadCommandHandler : IRequestHandler<MarkAllNotificationsReadCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    public MarkAllNotificationsReadCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        => (_unitOfWork, _currentUser) = (unitOfWork, currentUser);
    public async Task Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
    {
        var items = await _unitOfWork.Notifications.Query()
            .Where(x => x.UserId == _currentUser.UserId && !x.IsRead).ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;
        foreach (var item in items) { item.IsRead = true; item.ReadAt = now; }
        if (items.Count > 0) await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

public sealed class UpdateNotificationPreferencesCommandHandler : IRequestHandler<UpdateNotificationPreferencesCommand, NotificationPreferenceDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    public UpdateNotificationPreferencesCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        => (_unitOfWork, _currentUser) = (unitOfWork, currentUser);
    public async Task<NotificationPreferenceDto> Handle(UpdateNotificationPreferencesCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new ForbiddenException("An authenticated user is required.");
        var entity = await _unitOfWork.NotificationPreferences.Query()
            .FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
        if (entity is null)
        {
            entity = new NotificationPreference { UserId = userId };
            await _unitOfWork.NotificationPreferences.AddAsync(entity);
        }
        entity.InAppEnabled = request.Preferences.InAppEnabled;
        entity.EmailEnabled = request.Preferences.EmailEnabled;
        entity.JobAlertsEnabled = request.Preferences.JobAlertsEnabled;
        entity.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return request.Preferences;
    }
}
