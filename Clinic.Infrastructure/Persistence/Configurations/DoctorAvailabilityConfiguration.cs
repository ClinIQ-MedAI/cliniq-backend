using Clinic.Infrastructure.Entities;

namespace Clinic.Infrastructure.Persistence.Configurations;

public class DoctorAvailabilityConfiguration : IEntityTypeConfiguration<DoctorAvailability>
{
    public void Configure(EntityTypeBuilder<DoctorAvailability> builder)
    {
        builder.ToTable("DoctorAvailabilities");

        builder.HasKey(da => new { da.DoctorId, da.DayOfWeek });

        builder.HasOne(da => da.Doctor)
            .WithMany(d => d.AvailabilityDays)
            .HasForeignKey(da => da.DoctorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ensure DayOfWeek uses integer storage (default but explicit doesn't hurt, or convert to string if preferred. Leaving as default enum-to-int for DB efficiency)
        builder.Property(da => da.DayOfWeek)
            .HasConversion<string>();
    }
}
