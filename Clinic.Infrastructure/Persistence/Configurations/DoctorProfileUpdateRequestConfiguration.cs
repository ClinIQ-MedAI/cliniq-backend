using Clinic.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public class DoctorProfileUpdateRequestConfiguration : IEntityTypeConfiguration<DoctorProfileUpdateRequest>
{
    public void Configure(EntityTypeBuilder<DoctorProfileUpdateRequest> builder)
    {
        builder.ToTable("DoctorProfileUpdateRequests");

        builder.HasKey(r => r.Id);

        builder.HasOne(r => r.Doctor)
            .WithMany()
            .HasForeignKey(r => r.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(r => r.Status)
            .HasConversion<string>();

        builder.HasIndex(r => new { r.DoctorId, r.Status });
    }
}
