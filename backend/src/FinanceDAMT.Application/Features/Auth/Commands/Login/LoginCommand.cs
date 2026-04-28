using FinanceDAMT.Application.Features.Auth.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Auth.Commands.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<AuthResponse>;
