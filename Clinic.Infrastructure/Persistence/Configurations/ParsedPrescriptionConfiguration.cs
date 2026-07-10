using Clinic.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public class ParsedPrescriptionConfiguration : IEntityTypeConfiguration<ParsedPrescription>
{
    public void Configure(EntityTypeBuilder<ParsedPrescription> builder)
    {
        builder.ToTable("ParsedPrescriptions");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.PrescriptionImageUrl)
            .HasMaxLength(2000)
            .IsRequired(false);

        builder.Property(p => p.PrescriptionImageBase64)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(p => p.RawParsedText)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(p => p.MedicationsJson)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        builder.Property(p => p.DoctorNotes)
            .HasColumnType("nvarchar(max)")
            .IsRequired(false);

        // Relationships
        builder.HasOne(p => p.Patient)
            .WithMany()
            .HasForeignKey(p => p.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Doctor)
            .WithMany()
            .HasForeignKey(p => p.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.AIJob)
            .WithMany()
            .HasForeignKey(p => p.AIJobId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
