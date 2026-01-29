
namespace ClinicAPI.Persistence.EntitiesConfigurations;

public class QuestionConfiguration : IEntityTypeConfiguration<Question>
{
    public void Configure(EntityTypeBuilder<Question> builder)
    {
        builder.HasIndex(a => new { a.PollId, a.Content }).IsUnique();
        builder.Property(a => a.Content).HasMaxLength(1000);
    }
}
