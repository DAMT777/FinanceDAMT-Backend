using FinanceDAMT.Application.Features.Auth.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Auth.Commands.Register;

public record RegisterCommand(
    string Name,
    string Email,
    string Password
) : IRequest<AuthResponse>;
