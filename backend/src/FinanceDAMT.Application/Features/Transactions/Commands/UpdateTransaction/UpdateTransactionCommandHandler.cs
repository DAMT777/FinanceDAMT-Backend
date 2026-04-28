using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Transactions.DTOs;
using FinanceDAMT.Domain.Entities;
using FinanceDAMT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.Transactions.Commands.UpdateTransaction;

public sealed class UpdateTransactionCommandHandler : IRequestHandler<UpdateTransactionCommand, TransactionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ICacheService _cache;

    public UpdateTransactionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        ICacheService cache)
    {
        _context = context;
        _currentUser = currentUser;
        _cache = cache;
    }

    public async Task<TransactionDto> Handle(UpdateTransactionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var existing = await _context.Transactions
            .Include(t => t.Account)
            .Include(t => t.Category)
            .FirstOrDefaultAsync(t => t.Id == request.Id && t.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Transaction not found.");

        var previousMonth = existing.Date.Month;
        var previousYear = existing.Date.Year;

        var newAccount = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId && a.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Account not found.");

        var newCategory = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && (c.UserId == null || c.UserId == userId), cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        RevertTransactionEffect(existing.Account, existing.Type, existing.Amount);
        ApplyTransactionEffect(newAccount, request.Type, request.Amount);

        existing.AccountId = newAccount.Id;
        existing.CategoryId = newCategory.Id;
        existing.Type = request.Type;
        existing.Amount = request.Amount;
        existing.Date = request.Date;
        existing.Description = request.Description?.Trim();
        existing.IsRecurring = request.IsRecurring;

        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync($"dashboard:{userId}:{previousYear}:{previousMonth}", cancellationToken);
        await _cache.RemoveAsync($"dashboard:{userId}:{existing.Date.Year}:{existing.Date.Month}", cancellationToken);

        return new TransactionDto(
            existing.Id,
            existing.AccountId,
            newAccount.Name,
            existing.CategoryId,
            newCategory.Name,
            existing.Type,
            existing.Amount,
            existing.Date,
            existing.Description,
            existing.ReceiptUrl,
            existing.IsRecurring);
    }

    private static void RevertTransactionEffect(Account account, TransactionType type, decimal amount)
    {
        if (type == TransactionType.Income)
            account.Balance -= amount;
        else
            account.Balance += amount;
    }

    private static void ApplyTransactionEffect(Account account, TransactionType type, decimal amount)
    {
        if (type == TransactionType.Income)
            account.Balance += amount;
        else
            account.Balance -= amount;
    }
}
