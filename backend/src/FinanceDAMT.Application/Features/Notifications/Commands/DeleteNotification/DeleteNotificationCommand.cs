using MediatR;

namespace FinanceDAMT.Application.Features.Notifications.Commands.DeleteNotification;

public sealed record DeleteNotificationCommand(Guid Id) : IRequest<Unit>;
