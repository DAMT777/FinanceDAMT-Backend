using MediatR;

namespace FinanceDAMT.Application.Features.Auth.Commands.ResendVerification;

public record ResendVerificationCommand(string Email) : IRequest<Unit>;
