namespace FinanceDAMT.Application.Common.Interfaces;

public interface IBudgetAlertService
{
    Task CheckThresholdsAsync(Guid userId, Guid categoryId, int month, int year, CancellationToken cancellationToken = default);
}
