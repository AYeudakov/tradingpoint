namespace BitcoinPrices.Application.Configuration;

public sealed class AggregationOptions
{
    public const string SectionName = "Aggregation";

    public int MinimumSuccessfulProviders { get; init; } = 2;
}
