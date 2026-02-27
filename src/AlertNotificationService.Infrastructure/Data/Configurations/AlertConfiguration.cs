using AlertNotificationService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlertNotificationService.Infrastructure.Data.Configurations;

public class AlertConfiguration : IEntityTypeConfiguration<Alert>
{
    public void Configure(EntityTypeBuilder<Alert> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.AlertName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(a => a.Severity)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.Status)
            .IsRequired();

        builder.Property(a => a.Summary)
            .HasMaxLength(500);

        builder.Property(a => a.Description)
            .HasMaxLength(2000);

        builder.Property(a => a.Instance)
            .HasMaxLength(200);

        builder.Property(a => a.Job)
            .HasMaxLength(200);

        builder.Property(a => a.Fingerprint)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.LabelsJson)
            .HasColumnType("jsonb");

        builder.HasIndex(a => a.Fingerprint);

        builder.ToTable("Alerts");
    }
}
