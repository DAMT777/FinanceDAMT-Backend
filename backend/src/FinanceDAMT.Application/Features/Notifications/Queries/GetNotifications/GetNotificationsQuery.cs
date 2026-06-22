using FinanceDAMT.Application.Features.Notifications.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Notifications.Queries.GetNotifications;

public sealed record GetNotificationsQuery : IRequest<IReadOnlyList<NotificationDto>>;
