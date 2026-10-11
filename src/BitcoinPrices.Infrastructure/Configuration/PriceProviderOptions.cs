namespace BitcoinPrices.Infrastructure.Configuration;

public sealed class PriceProviderOptions
{
    public required string BaseUrl { get; init; }
    public int TimeoutSeconds { get; init; } = 10;
}
