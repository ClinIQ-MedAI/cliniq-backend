using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Entities.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public class DoctorProfileConfiguration : IEntityTypeConfiguration<DoctorProfile>
{
    public void Configure(EntityTypeBuilder<DoctorProfile> builder)
    {
        // Configure Shared Primary Key for DoctorProfile
        builder.HasKey(d => d.Id);

        builder.HasOne(d => d.User)
            .WithOne(u => u.DoctorProfile)
            .HasForeignKey<DoctorProfile>(d => d.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(d => d.Status)
            .HasConversion<string>();
    }
}
