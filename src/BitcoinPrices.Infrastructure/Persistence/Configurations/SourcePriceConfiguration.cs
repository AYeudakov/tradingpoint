using BitcoinPrices.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BitcoinPrices.Infrastructure.Persistence.Configurations;

public sealed class SourcePriceConfiguration : IEntityTypeConfiguration<SourcePrice>
{
    public void Configure(EntityTypeBuilder<SourcePrice> builder)
    {
        var timestampConverter = new ValueConverter<DateTimeOffset, long>(
            timestamp => timestamp.ToUnixTimeSeconds(),
            unixSeconds => DateTimeOffset.FromUnixTimeSeconds(unixSeconds));

        builder.ToTable("SourcePrices");
        builder.HasKey(source => source.Id);
        builder.Property(source => source.ProviderName)
            .HasMaxLength(100)
            .IsRequired();
        builder.Property(source => source.Price).IsRequired();
        builder.Property(source => source.RetrievedAtUtc)
            .HasConversion(timestampConverter)
            .IsRequired();

        builder.HasIndex(source => new
            {
                source.AggregatedPriceId,
                source.ProviderName
            })
            .IsUnique();
    }
}
