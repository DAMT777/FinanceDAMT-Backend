using FinanceDAMT.Application.Features.Transactions.DTOs;

namespace FinanceDAMT.Application.Common.Interfaces;

public interface ITransactionRepository
{
    Task<PaginatedResult<TransactionDto>> GetTransactionsAsync(
        Guid userId,
        TransactionFilterRequest filter,
        CancellationToken cancellationToken = default);

    Task<TransactionDto?> GetByIdAsync(
        Guid userId,
        Guid transactionId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TransactionDto>> GetRecurringAsync(
        Guid userId,
        CancellationToken cancellationToken = default);
}
