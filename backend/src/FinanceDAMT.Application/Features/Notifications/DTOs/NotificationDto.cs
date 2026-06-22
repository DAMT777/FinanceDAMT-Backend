using FinanceDAMT.Domain.Enums;

namespace FinanceDAMT.Application.Features.Notifications.DTOs;

public sealed record NotificationDto(
    Guid Id,
    NotificationType Type,
    string Title,
    string Message,
    bool IsRead,
    DateTime CreatedAt
);
