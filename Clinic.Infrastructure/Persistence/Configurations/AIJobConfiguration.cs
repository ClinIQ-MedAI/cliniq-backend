using Clinic.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public class AIJobConfiguration : IEntityTypeConfiguration<AIJob>
{
    public void Configure(EntityTypeBuilder<AIJob> builder)
    {
        builder.ToTable("AIJobs");

        builder.HasKey(j => j.Id);

        builder.Property(j => j.Id)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(j => j.Modality)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(j => j.PatientId)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(j => j.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(j => j.ImageBase64)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(j => j.ImageUrl)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(j => j.OptionsJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(j => j.ReplyTo)
            .HasMaxLength(250)
            .IsRequired(false);

        builder.Property(j => j.ResultJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(j => j.ErrorMessage)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(j => j.Worker)
            .HasMaxLength(250)
            .IsRequired(false);

        builder.Property(j => j.DurationMs)
            .IsRequired(false);

        builder.Property(j => j.FinishedAt)
            .IsRequired(false);
    }
}
