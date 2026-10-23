using GiphyIntegration.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GiphyIntegration.Infrastructure.Persistence.Configurations;

internal sealed class UserSearchRecordConfiguration : IEntityTypeConfiguration<UserSearchRecord>
{
    public void Configure(EntityTypeBuilder<UserSearchRecord> builder)
    {
        builder.ToTable("UserSearches");
        builder.HasKey(s => s.Id);

        // Long enough for any IPv6 address, or a user id later on.
        builder.Property(s => s.UserKey).HasMaxLength(64).IsRequired();

        builder.HasIndex(s => new { s.UserKey, s.SearchResultId }).IsUnique();
        builder.HasIndex(s => new { s.UserKey, s.LastSearchedAtUtc });
    }
}
