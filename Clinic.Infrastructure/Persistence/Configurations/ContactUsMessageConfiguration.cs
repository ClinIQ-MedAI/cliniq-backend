using Clinic.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Clinic.Infrastructure.Persistence.Configurations;

public class ContactUsMessageConfiguration : IEntityTypeConfiguration<ContactUsMessage>
{
    public void Configure(EntityTypeBuilder<ContactUsMessage> builder)
    {
        builder.ToTable("ContactUsMessages");

        builder.HasKey(m => m.Id);

        builder.Property(m => m.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.Email)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(m => m.Subject)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(m => m.Body)
            .HasMaxLength(5000)
            .IsRequired();
    }
}
