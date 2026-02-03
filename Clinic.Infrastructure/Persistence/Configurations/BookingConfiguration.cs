using Clinic.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public class BookingConfiguration : IEntityTypeConfiguration<Booking>
{
    public void Configure(EntityTypeBuilder<Booking> builder)
    {
        builder.ToTable("Bookings");

        builder.HasKey(b => b.Id);

        builder.Property(b => b.Status)
            .HasConversion<string>();

        builder.HasOne(b => b.Patient)
            .WithMany() // Assuming PatientProfile doesn't have a Bookings collection yet, or we didn't add it. User added Bookings to DoctorSchedule but not PatientProfile? Let me check PatientProfile.
            .HasForeignKey(b => b.PatientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.DoctorSchedule)
            .WithMany(ds => ds.Bookings)
            .HasForeignKey(b => b.DoctorScheduleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
