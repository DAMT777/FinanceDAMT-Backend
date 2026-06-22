using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Domain.Entities;
using FinanceDAMT.Domain.Enums;

namespace FinanceDAMT.Infrastructure.Services;

public sealed class NotificationService : INotificationService
{
    private readonly IApplicationDbContext _context;

    public NotificationService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        CancellationToken cancellationToken = default)
    {
        _context.Notifications.Add(new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Message = message,
            IsRead = false
        });

        await _context.SaveChangesAsync(cancellationToken);
    }
}
