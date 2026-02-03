using Clinic.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public class DoctorAvailabilityConfiguration : IEntityTypeConfiguration<DoctorAvailability>
{
    public void Configure(EntityTypeBuilder<DoctorAvailability> builder)
    {
        builder.ToTable("DoctorAvailabilities");

        builder.HasKey(da => da.Id);

        builder.HasOne(da => da.Doctor)
            .WithMany(d => d.AvailabilityDays)
            .HasForeignKey(da => da.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Ensure DayOfWeek uses integer storage (default but explicit doesn't hurt, or convert to string if preferred. Leaving as default enum-to-int for DB efficiency)
        builder.Property(da => da.DayOfWeek)
            .HasConversion<int>();
    }
}
