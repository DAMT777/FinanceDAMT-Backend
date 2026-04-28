using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.Transactions.DTOs;
using FinanceDAMT.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Infrastructure.Persistence.Repositories;

public sealed class TransactionRepository : ITransactionRepository
{
    private readonly ApplicationDbContext _context;

    public TransactionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedResult<TransactionDto>> GetTransactionsAsync(
        Guid userId,
        TransactionFilterRequest filter,
        CancellationToken cancellationToken = default)
    {
        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize is < 1 or > 100 ? 20 : filter.PageSize;

        var query = _context.Transactions
            .AsNoTracking()
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Where(t => t.UserId == userId)
            .AsQueryable();

        if (filter.AccountId.HasValue)
            query = query.Where(t => t.AccountId == filter.AccountId.Value);

        if (filter.CategoryId.HasValue)
            query = query.Where(t => t.CategoryId == filter.CategoryId.Value);

        if (filter.Type.HasValue)
            query = query.Where(t => t.Type == filter.Type.Value);

        if (filter.DateFrom.HasValue)
            query = query.Where(t => t.Date >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(t => t.Date <= filter.DateTo.Value);

        if (!string.IsNullOrWhiteSpace(filter.SearchText))
        {
            var search = filter.SearchText.Trim();
            var pattern = $"%{search}%";
            query = query.Where(t =>
                (t.Description != null && EF.Functions.Like(t.Description, pattern)) ||
                EF.Functions.Like(t.Account.Name, pattern) ||
                EF.Functions.Like(t.Category.Name, pattern));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(t => t.Date)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TransactionDto(
                t.Id,
                t.AccountId,
                t.Account.Name,
                t.CategoryId,
                t.Category.Name,
                t.Type,
                t.Amount,
                t.Date,
                t.Description,
                t.ReceiptUrl,
                t.IsRecurring))
            .ToListAsync(cancellationToken);

        return new PaginatedResult<TransactionDto>(items, page, pageSize, totalCount);
    }

    public async Task<TransactionDto?> GetByIdAsync(
        Guid userId,
        Guid transactionId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.Id == transactionId)
            .Select(t => new TransactionDto(
                t.Id,
                t.AccountId,
                t.Account.Name,
                t.CategoryId,
                t.Category.Name,
                t.Type,
                t.Amount,
                t.Date,
                t.Description,
                t.ReceiptUrl,
                t.IsRecurring))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TransactionDto>> GetRecurringAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Transactions
            .AsNoTracking()
            .Include(t => t.Account)
            .Include(t => t.Category)
            .Where(t => t.UserId == userId && t.IsRecurring)
            .OrderByDescending(t => t.Date)
            .Select(t => new TransactionDto(
                t.Id,
                t.AccountId,
                t.Account.Name,
                t.CategoryId,
                t.Category.Name,
                t.Type,
                t.Amount,
                t.Date,
                t.Description,
                t.ReceiptUrl,
                t.IsRecurring))
            .ToListAsync(cancellationToken);
    }
}
