using MediatR;

namespace FinanceDAMT.Application.Features.Notifications.Commands.MarkAllAsRead;

public sealed record MarkAllNotificationsAsReadCommand : IRequest<Unit>;
