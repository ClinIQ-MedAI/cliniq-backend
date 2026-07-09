using Clinic.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public class HealthNewsConfiguration : IEntityTypeConfiguration<HealthNews>
{
    public void Configure(EntityTypeBuilder<HealthNews> builder)
    {
        builder.ToTable("HealthNews");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Title)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(n => n.Image)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(n => n.Description)
            .HasMaxLength(5000)
            .IsRequired();
    }
}
