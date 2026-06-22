using FinanceDAMT.Domain.Enums;

namespace FinanceDAMT.Application.Common.Interfaces;

/// <summary>
/// Creates in-app notifications from domain events (budget alerts, goal milestones, etc.).
/// </summary>
public interface INotificationService
{
    Task CreateAsync(
        Guid userId,
        NotificationType type,
        string title,
        string message,
        CancellationToken cancellationToken = default);
}
