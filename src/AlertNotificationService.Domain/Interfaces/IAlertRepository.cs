using AlertNotificationService.Domain.Entities;

namespace AlertNotificationService.Domain.Interfaces;

public interface IAlertRepository
{
    Task<Alert?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Alert>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<Alert?> GetFiringByFingerprintAsync(string fingerprint, CancellationToken cancellationToken = default);
    Task<Alert> AddAsync(Alert alert, CancellationToken cancellationToken = default);
    Task UpdateAsync(Alert alert, CancellationToken cancellationToken = default);
}
