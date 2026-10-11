using BitcoinPrices.Domain.Entities;
using BitcoinPrices.Domain.Enums;
using BitcoinPrices.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace BitcoinPrices.Infrastructure.Persistence.Configurations;

public sealed class AggregatedPriceConfiguration : IEntityTypeConfiguration<AggregatedPrice>
{
    public void Configure(EntityTypeBuilder<AggregatedPrice> builder)
    {
        var currencyConverter = new ValueConverter<Currency, string>(
            currency => currency.Code,
            code => new Currency(code));
        var timestampConverter = new ValueConverter<DateTimeOffset, long>(
            timestamp => timestamp.ToUnixTimeSeconds(),
            unixSeconds => DateTimeOffset.FromUnixTimeSeconds(unixSeconds));

        builder.ToTable("AggregatedPrices");
        builder.HasKey(price => price.Id);
        builder.Property(price => price.BaseCurrency)
            .HasConversion(currencyConverter)
            .HasMaxLength(10)
            .IsRequired();
        builder.Property(price => price.QuoteCurrency)
            .HasConversion(currencyConverter)
            .HasMaxLength(10)
            .IsRequired();
        builder.Property(price => price.TimeUtc)
            .HasConversion(timestampConverter)
            .IsRequired();
        builder.Property(price => price.Price).IsRequired();
        builder.Property(price => price.AggregationMethod)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        builder.Property(price => price.AggregationVersion).IsRequired();
        builder.Property(price => price.CreatedAtUtc)
            .HasConversion(timestampConverter)
            .IsRequired();

        builder.HasIndex(price => new
            {
                price.BaseCurrency,
                price.QuoteCurrency,
                price.TimeUtc,
                price.AggregationMethod,
                price.AggregationVersion
            })
            .IsUnique();

        builder.HasMany(price => price.Sources)
            .WithOne(source => source.AggregatedPrice)
            .HasForeignKey(source => source.AggregatedPriceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
