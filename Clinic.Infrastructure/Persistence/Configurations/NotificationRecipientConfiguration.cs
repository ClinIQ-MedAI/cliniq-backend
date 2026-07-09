using Clinic.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public class NotificationRecipientConfiguration : IEntityTypeConfiguration<NotificationRecipient>
{
    public void Configure(EntityTypeBuilder<NotificationRecipient> builder)
    {
        builder.ToTable("NotificationRecipients");

        builder.HasKey(r => new { r.NotificationId, r.UserId });

        builder.HasOne(r => r.User)
            .WithMany()
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(r => r.UserId);
        builder.HasIndex(r => new { r.UserId, r.IsRead });
    }
}
