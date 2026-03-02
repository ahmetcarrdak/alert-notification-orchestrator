using AlertNotificationService.Application.Common.Interfaces;
using AlertNotificationService.Domain.Interfaces;
using AlertNotificationService.Infrastructure.Data;
using AlertNotificationService.Infrastructure.Logs;
using AlertNotificationService.Infrastructure.Notifications;
using AlertNotificationService.Infrastructure.Repositories;
using AlertNotificationService.Infrastructure.Settings;
using AlertNotificationService.Infrastructure.Watchdog;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AlertNotificationService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IAlertRepository, AlertRepository>();

        services.Configure<TeamsSettings>(configuration.GetSection(TeamsSettings.SectionName));
        services.AddHttpClient<ITeamsNotificationSender, PowerAutomateNotificationSender>();

        services.Configure<SeqSettings>(configuration.GetSection(SeqSettings.SectionName));
        services.AddHttpClient<ISeqLogFetcher, SeqLogFetcher>();

        services.AddSingleton<IWatchdogTracker, WatchdogTracker>();
        services.AddHostedService<WatchdogMonitorService>();

        return services;
    }
}
