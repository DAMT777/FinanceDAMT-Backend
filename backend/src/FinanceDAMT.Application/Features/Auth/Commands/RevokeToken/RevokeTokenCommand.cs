using MediatR;

namespace FinanceDAMT.Application.Features.Auth.Commands.RevokeToken;

public record RevokeTokenCommand(
    string Token
) : IRequest<Unit>;
