namespace BitcoinPrices.Domain.Entities;

public sealed class SourcePrice
{
    public long Id { get; set; }
    public long AggregatedPriceId { get; set; }
    public string ProviderName { get; set; } = string.Empty;
    public double Price { get; set; }
    public DateTimeOffset RetrievedAtUtc { get; set; }
    public AggregatedPrice AggregatedPrice { get; set; } = null!;
}
