using System.Globalization;
using FinanceDAMT.Application.Common.Exceptions;
using FinanceDAMT.Application.Common.Interfaces;
using FinanceDAMT.Application.Features.AI.Agent;
using FinanceDAMT.Application.Features.AI.DTOs;
using FinanceDAMT.Application.Features.SavingGoals.Commands.AddContribution;
using FinanceDAMT.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace FinanceDAMT.Application.Features.AI.Commands.AddContributionFromText;

public sealed class AddContributionFromTextCommandHandler
    : IRequestHandler<AddContributionFromTextCommand, AgentLogResultDto>
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

    public AddContributionFromTextCommandHandler(
        IApplicationDbContext context, ICurrentUserService currentUser, ISender sender)
    {
        _context = context;
        _currentUser = currentUser;
        _sender = sender;
    }

    public async Task<AgentLogResultDto> Handle(AddContributionFromTextCommand request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId ?? throw new UnauthorizedException("User is not authenticated.");

        var parsed = FinanceTextParser.TryParseGoalContribution(request.Text);
        if (parsed is null)
            return new AgentLogResultDto(false, "No identifiqué cuánto asignar ni a qué meta. Intenta algo como \"asigna el 10% de mi balance a mi meta del viaje\".", null);

        var goals = await _context.SavingGoals
            .Where(g => g.UserId == userId)
            .ToListAsync(cancellationToken);

        if (goals.Count == 0)
            return new AgentLogResultDto(false, "Aún no tienes metas de ahorro. Crea una primero y luego te ayudo a asignarle dinero.", null);

        var goal = ResolveGoal(goals, request.Text);
        if (goal is null)
        {
            var names = string.Join(", ", goals.Select(g => $"\"{g.Name}\""));
            return new AgentLogResultDto(false, $"No identifiqué a cuál meta te refieres. Tus metas son: {names}.", null);
        }

        decimal amount;
        if (parsed.PercentOfBalance is decimal percent)
        {
            var balance = await _context.Accounts
                .Where(a => a.UserId == userId)
                .SumAsync(a => (decimal?)a.Balance, cancellationToken) ?? 0m;

            if (balance <= 0)
                return new AgentLogResultDto(false, "Tu balance actual es $0, así que no hay saldo para asignar.", null);

            amount = Math.Round(balance * percent / 100m);
        }
        else
        {
            amount = parsed.Amount ?? 0m;
        }

        if (amount <= 0)
            return new AgentLogResultDto(false, "El monto a asignar debe ser mayor a cero.", null);

        var updated = await _sender.Send(
            new AddContributionCommand(goal.Id, amount, DateTime.UtcNow, "Asignación desde el asistente IA"),
            cancellationToken);

        var remaining = Math.Max(0m, updated.TargetAmount - updated.CurrentAmount);
        var tail = updated.IsCompleted ? ". 🎉 ¡Meta completada!" : $" (faltan {Money(remaining)}).";
        var message =
            $"✓ Asigné {Money(amount)} a tu meta \"{updated.Name}\". " +
            $"Ahorro actual: {Money(updated.CurrentAmount)} de {Money(updated.TargetAmount)}{tail} " +
            "Quedó registrado de verdad; puedes verlo en la sección de metas.";

        return new AgentLogResultDto(true, message, null);
    }

    private static SavingGoal? ResolveGoal(List<SavingGoal> goals, string text)
    {
        var n = FinanceTextParser.Normalize(text);
        SavingGoal? best = null;
        var bestScore = 0;

        foreach (var goal in goals)
        {
            var name = FinanceTextParser.Normalize(goal.Name);
            var score = 0;
            if (name.Length > 0 && n.Contains(name)) score += 10;
            foreach (var word in name.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                if (word.Length >= 3 && n.Contains(word)) score += 1;
            }
            if (score > bestScore)
            {
                bestScore = score;
                best = goal;
            }
        }

        if (bestScore == 0)
            return goals.Count == 1 ? goals[0] : null;

        return best;
    }

    private static string Money(decimal value) => "$" + value.ToString("#,##0", MoneyFormat);
}
