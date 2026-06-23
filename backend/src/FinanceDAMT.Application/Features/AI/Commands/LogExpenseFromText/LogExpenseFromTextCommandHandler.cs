using System.Globalization;
using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.AI.Agent;
using FinanceDAMT.Application.Features.AI.DTOs;
using FinanceDAMT.Application.Features.Transactions.Commands.CreateTransaction;
using FinanceDAMT.Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.AI.Commands.LogExpenseFromText;

public sealed class LogExpenseFromTextCommandHandler : IRequestHandler<LogExpenseFromTextCommand, AgentLogResultDto>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly ISender _sender;

    private static readonly NumberFormatInfo MoneyFormat = new()
    {
        NumberGroupSeparator = ".",
        NumberDecimalSeparator = ",",
        NumberDecimalDigits = 0
    };

    public LogExpenseFromTextCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser, ISender sender)
    {
        _context = context;
        _currentUser = currentUser;
        _sender = sender;
    }

    public async Task<AgentLogResultDto> Handle(LogExpenseFromTextCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var parsed = FinanceTextParser.TryParseExpense(request.Text);
        if (parsed is null)
            return new AgentLogResultDto(false, "No logré identificar un monto y una acción. Intenta algo como \"compré una gaseosa a 5000\".", null);

        var account = await _context.Accounts
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (account is null)
        {
            account = new Domain.Entities.Account
            {
                UserId = userId,
                Name = "Efectivo",
                Type = AccountType.Cash,
                Balance = 0m
            };
            _context.Accounts.Add(account);
            await _context.SaveChangesAsync(cancellationToken);
        }

        var categoryType = parsed.Type == TransactionType.Income ? CategoryType.Income : CategoryType.Expense;
        var category = await ResolveCategoryAsync(userId, parsed.CategoryName, categoryType, cancellationToken);

        if (category is null)
            return new AgentLogResultDto(false, "No encontré una categoría adecuada para registrar el movimiento.", null);

        var transaction = await _sender.Send(
            new CreateTransactionCommand(
                account.Id,
                category.Id,
                parsed.Type,
                parsed.Amount,
                DateTime.UtcNow,
                parsed.Description),
            cancellationToken);

        var verb = parsed.Type == TransactionType.Income ? "ingreso" : "gasto";
        var message =
            $"✓ Registré un {verb} de {Money(parsed.Amount)} en {category.Name} ({account.Name}). " +
            "Ya quedó guardado en tus transacciones; si algo no cuadra puedes editarlo o eliminarlo.";

        return new AgentLogResultDto(true, message, transaction);
    }

    private async Task<Domain.Entities.Category?> ResolveCategoryAsync(
        Guid userId, string preferredName, CategoryType type, CancellationToken cancellationToken)
    {
        var candidates = await _context.Categories
            .Where(c => (c.UserId == null || c.UserId == userId) && c.Type == type)
            .ToListAsync(cancellationToken);

        return candidates.FirstOrDefault(c => c.Name == preferredName)
            ?? candidates.FirstOrDefault(c => c.Name == (type == CategoryType.Income ? "Other income" : "Other expense"))
            ?? candidates.FirstOrDefault();
    }

    private static string Money(decimal value) => "$" + value.ToString("#,##0", MoneyFormat);
}
