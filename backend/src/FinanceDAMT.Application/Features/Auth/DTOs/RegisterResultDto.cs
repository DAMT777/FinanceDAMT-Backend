namespace FinanceDAMT.Application.Features.Auth.DTOs;

public sealed record RegisterResultDto(string Email, bool RequiresEmailVerification);
