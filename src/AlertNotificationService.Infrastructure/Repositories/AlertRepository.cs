using AlertNotificationService.Domain.Entities;
using AlertNotificationService.Domain.Enums;
using AlertNotificationService.Domain.Interfaces;
using AlertNotificationService.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AlertNotificationService.Infrastructure.Repositories;

public class AlertRepository : IAlertRepository
{
    private readonly AppDbContext _context;

    public AlertRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Alert?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await _context.Alerts.FindAsync([id], cancellationToken);

    public async Task<IEnumerable<Alert>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.Alerts.OrderByDescending(a => a.CreatedAt).ToListAsync(cancellationToken);

    public async Task<Alert?> GetFiringByFingerprintAsync(string fingerprint, CancellationToken cancellationToken = default)
        => await _context.Alerts
            .Where(a => a.Fingerprint == fingerprint && a.Status == AlertStatus.Firing)
            .OrderByDescending(a => a.StartsAt)
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<Alert> AddAsync(Alert alert, CancellationToken cancellationToken = default)
    {
        await _context.Alerts.AddAsync(alert, cancellationToken);
        return alert;
    }

    public Task UpdateAsync(Alert alert, CancellationToken cancellationToken = default)
    {
        _context.Alerts.Update(alert);
        return Task.CompletedTask;
    }
}
