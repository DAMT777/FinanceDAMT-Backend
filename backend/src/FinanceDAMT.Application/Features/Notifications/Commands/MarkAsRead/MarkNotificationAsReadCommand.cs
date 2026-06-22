using MediatR;

namespace FinanceDAMT.Application.Features.Notifications.Commands.MarkAsRead;

public sealed record MarkNotificationAsReadCommand(Guid Id) : IRequest<Unit>;
