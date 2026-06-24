using FinanceDAMT.Application.Features.Auth.DTOs;
using MediatR;

namespace FinanceDAMT.Application.Features.Auth.Commands.VerifyEmail;

public record VerifyEmailCommand(string Email, string Code) : IRequest<AuthResponse>;
