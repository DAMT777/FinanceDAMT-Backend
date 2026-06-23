using FinanceDAMT.Domain.Enums;

namespace FinanceDAMT.Application.Common.Interfaces;

public interface INotificationService
{
    Task CreateAsync(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        CancellationToken cancellationToken = default);
}
