using Clinic.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public class DoctorScheduleConfiguration : IEntityTypeConfiguration<DoctorSchedule>
{
    public void Configure(EntityTypeBuilder<DoctorSchedule> builder)
    {
        builder.ToTable("DoctorSchedules");

        builder.HasKey(ds => ds.Id);

        builder.HasOne(ds => ds.Doctor)
            .WithMany(d => d.Schedules)
            .HasForeignKey(ds => ds.DoctorId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configure default value for BookingCount
        builder.Property(ds => ds.BookingCount)
            .HasDefaultValue(0);

        builder.Property(ds => ds.IsAvailable)
            .HasDefaultValue(true);
    }
}
