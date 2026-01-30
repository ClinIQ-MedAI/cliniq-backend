using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Clinic.Infrastructure.Entities;
using Clinic.Infrastructure.Abstractions;
using Clinic.Infrastructure.Extensions;

namespace Clinic.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AppDbContext(
        DbContextOptions<AppDbContext> options,
        IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Domain entity sets
    public DbSet<DoctorProfile> DoctorProfiles => Set<DoctorProfile>();
    public DbSet<PatientProfile> PatientProfiles => Set<PatientProfile>();


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply all configurations from assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        modelBuilder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(u => u.Gender)
                .HasConversion<string>();
        });

        // Configure Shared Primary Key for DoctorProfile
        modelBuilder.Entity<DoctorProfile>(entity =>
        {
            entity.HasKey(d => d.Id);

            entity.HasOne(d => d.User)
                .WithOne(u => u.DoctorProfile)
                .HasForeignKey<DoctorProfile>(d => d.Id)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(d => d.Status)
                .HasConversion<string>();
        });

        // Configure Shared Primary Key for PatientProfile
        modelBuilder.Entity<PatientProfile>(entity =>
        {
            entity.HasKey(p => p.Id);

            entity.HasOne(p => p.User)
                .WithOne(u => u.PatientProfile)
                .HasForeignKey<PatientProfile>(p => p.Id)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(p => p.Status)
                .HasConversion<string>();
        });

        // Disable cascade delete for all other FKs (except profile entities)
        var cascadeFKs = modelBuilder.Model
            .GetEntityTypes()
            .SelectMany(t => t.GetForeignKeys())
            .Where(fk => fk.DeleteBehavior == DeleteBehavior.Cascade && !fk.IsOwnership);

        foreach (var fk in cascadeFKs)
        {
            // Keep cascade for our profile entities
            var dependent = fk.DeclaringEntityType.ClrType;
            if (dependent != typeof(DoctorProfile) && dependent != typeof(PatientProfile))
            {
                fk.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<AuditableEntity>();

        foreach (var entityEntry in entries)
        {
            var currentUserId = _httpContextAccessor.HttpContext?.User.GetUserId();

            if (entityEntry.State == EntityState.Added)
                entityEntry.Property(x => x.CreatedById).CurrentValue = currentUserId;

            if (entityEntry.State == EntityState.Modified)
            {
                entityEntry.Property(x => x.UpdatedById).CurrentValue = currentUserId;
                entityEntry.Property(x => x.UpdatedOn).CurrentValue = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
