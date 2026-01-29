namespace ClinicAPI.Persistence.EntitiesConfigurations;

public class VoteAnswerConfiguration : IEntityTypeConfiguration<VoteAnswer>
{
    public void Configure(EntityTypeBuilder<VoteAnswer> builder)
    {
        builder.HasIndex(va => new { va.QuestionId, va.VoteId }).IsUnique();
    }
}
