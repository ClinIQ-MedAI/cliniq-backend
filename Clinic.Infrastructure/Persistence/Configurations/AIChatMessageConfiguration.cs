using Clinic.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public class AIChatMessageConfiguration : IEntityTypeConfiguration<AIChatMessage>
{
    public void Configure(EntityTypeBuilder<AIChatMessage> builder)
    {
        builder.ToTable("AIChatMessages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.ChatId)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(m => m.PatientId)
            .HasMaxLength(450)
            .IsRequired();

        builder.Property(m => m.Message)
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(m => m.LanguagePreference)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(m => m.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(m => m.Reply)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(m => m.QueryType)
            .HasMaxLength(50)
            .IsRequired(false);

        builder.Property(m => m.ShowUpload)
            .IsRequired();

        builder.Property(m => m.Error)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(m => m.Worker)
            .HasMaxLength(250)
            .IsRequired(false);

        builder.Property(m => m.DurationMs)
            .IsRequired(false);

        builder.Property(m => m.FinishedAt)
            .IsRequired(false);

        builder.HasOne(m => m.Patient)
            .WithMany()
            .HasForeignKey(m => m.PatientId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
