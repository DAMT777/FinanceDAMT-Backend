using MediatR;

namespace FinanceDAMT.Application.Features.Transactions.Commands.UploadTransactionReceipt;

public sealed record UploadTransactionReceiptCommand(
    Guid TransactionId,
    Stream FileStream,
    string FileName,
    string ContentType
) : IRequest<string>;
