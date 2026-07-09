using Clinic.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(n => n.Body)
            .HasMaxLength(1000);

        builder.Property(n => n.Type)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.HasMany(n => n.Recipients)
            .WithOne(r => r.Notification)
            .HasForeignKey(r => r.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
