using Clinic.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public class PatientScanConfiguration : IEntityTypeConfiguration<PatientScan>
{
    public void Configure(EntityTypeBuilder<PatientScan> builder)
    {
        builder.ToTable("PatientScans");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Modality)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.ScanUrl)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(s => s.ScanBase64)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(s => s.AIAnalysisResult)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(s => s.DoctorNotes)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        // Relationships
        builder.HasOne(s => s.Patient)
            .WithMany()
            .HasForeignKey(s => s.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Doctor)
            .WithMany()
            .HasForeignKey(s => s.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.AIJob)
            .WithMany()
            .HasForeignKey(s => s.AIJobId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
