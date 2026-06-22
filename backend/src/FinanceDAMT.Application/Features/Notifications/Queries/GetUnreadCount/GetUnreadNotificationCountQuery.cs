using MediatR;

namespace FinanceDAMT.Application.Features.Notifications.Queries.GetUnreadCount;

public sealed record GetUnreadNotificationCountQuery : IRequest<int>;
