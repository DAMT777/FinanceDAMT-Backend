using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.Transactions.Commands.UploadTransactionReceipt;

public sealed class UploadTransactionReceiptCommandHandler : IRequestHandler<UploadTransactionReceiptCommand, string>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IBlobStorageService _blobStorage;

    public UploadTransactionReceiptCommandHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IBlobStorageService blobStorage)
    {
        _context = context;
        _currentUser = currentUser;
        _blobStorage = blobStorage;
    }

    public async Task<string> Handle(UploadTransactionReceiptCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var transaction = await _context.Transactions
            .FirstOrDefaultAsync(t => t.Id == request.TransactionId && t.UserId == userId, cancellationToken)
            ?? throw new NotFoundException("Transaction not found.");

        var blobName = await _blobStorage.UploadAsync(request.FileStream, request.FileName, request.ContentType, cancellationToken);
        var receiptUrl = _blobStorage.GetUrl(blobName);

        transaction.ReceiptUrl = receiptUrl;
        await _context.SaveChangesAsync(cancellationToken);

        return receiptUrl;
    }
}
