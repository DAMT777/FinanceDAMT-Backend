using FinanceDAMT.Application.Features.Auth.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Auth.Commands.RefreshToken;

public record RefreshTokenCommand(
    string Token
) : IRequest<AuthResponse>;
