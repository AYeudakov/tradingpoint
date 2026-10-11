using BitcoinPrices.Domain.Enums;
using BitcoinPrices.Domain.ValueObjects;

namespace BitcoinPrices.Domain.Entities;

public sealed class AggregatedPrice
{
    public long Id { get; set; }
    public Currency BaseCurrency { get; set; }
    public Currency QuoteCurrency { get; set; }
    public DateTimeOffset TimeUtc { get; set; }
    public double Price { get; set; }
    public AggregationMethod AggregationMethod { get; set; }
    public int AggregationVersion { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public ICollection<SourcePrice> Sources { get; set; } = new List<SourcePrice>();
}
