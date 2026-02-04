using Clinic.Infrastructure.Entities;

namespace Clinic.Infrastructure.Persistence.Configurations;

public class PatientProfileConfiguration : IEntityTypeConfiguration<PatientProfile>
{
    public void Configure(EntityTypeBuilder<PatientProfile> builder)
    {
        // Configure Shared Primary Key for PatientProfile
        builder.HasKey(p => p.Id);

        builder.HasOne(p => p.User)
            .WithOne(u => u.PatientProfile)
            .HasForeignKey<PatientProfile>(p => p.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(p => p.Status)
            .HasConversion<string>();
    }
}
