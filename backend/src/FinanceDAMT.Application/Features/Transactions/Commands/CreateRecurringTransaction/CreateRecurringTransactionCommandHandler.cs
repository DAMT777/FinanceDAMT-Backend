using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Transactions.DTOs;
using FinanceDAMT.Domain.Entities;
using FinanceDAMT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.Transactions.Commands.CreateRecurringTransaction;

public sealed class CreateRecurringTransactionCommandHandler : IRequestHandler<CreateRecurringTransactionCommand, TransactionDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IBudgetAlertService _budgetAlertService;
    private readonly ICacheService _cache;

    public CreateRecurringTransactionCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IBudgetAlertService budgetAlertService,
        ICacheService cache)
    {
        _context = context;
        _currentUser = currentUser;
        _budgetAlertService = budgetAlertService;
        _cache = cache;
    }

    public async Task<TransactionDto> Handle(CreateRecurringTransactionCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var account = await _context.Accounts
            .FirstOrDefaultAsync(a => a.Id == request.AccountId && a.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Account not found.");

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == request.CategoryId && (c.UserId == null || c.UserId == userId), cancellationToken)
            ?? throw new NotFoundException("Category not found.");

        var transaction = new Transaction
        {
            UserId = userId,
            AccountId = account.Id,
            CategoryId = category.Id,
            Type = request.Type,
            Amount = request.Amount,
            Date = request.Date,
            Description = request.Description?.Trim(),
            IsRecurring = true
        };

        ApplyTransactionEffect(account, request.Type, request.Amount);

        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync(cancellationToken);

        await _cache.RemoveAsync($"dashboard:{userId}:{transaction.Date.Year}:{transaction.Date.Month}", cancellationToken);

        if (transaction.Type == TransactionType.Expense)
            await _budgetAlertService.CheckThresholdsAsync(userId, transaction.CategoryId, transaction.Date.Month, transaction.Date.Year, cancellationToken);

        return new TransactionDto(
            transaction.Id,
            transaction.AccountId,
            account.Name,
            transaction.CategoryId,
            category.Name,
            transaction.Type,
            transaction.Amount,
            transaction.Date,
            transaction.Description,
            transaction.ReceiptUrl,
            transaction.IsRecurring);
    }

    private static void ApplyTransactionEffect(Account account, TransactionType type, decimal amount)
    {
        if (type == TransactionType.Income)
            account.Balance += amount;
        else
            account.Balance -= amount;
    }
}
