using BitcoinPrices.Application.Abstractions;
using BitcoinPrices.Application.Models;
using BitcoinPrices.Domain.Enums;

namespace BitcoinPrices.Application.Aggregation;

public sealed class ArithmeticMeanPriceAggregator : IPriceAggregator
{
    public AggregationMethod Method => AggregationMethod.ArithmeticMean;
    public int Version => 1;

    public double Aggregate(IReadOnlyCollection<ProviderPrice> prices)
    {
        ArgumentNullException.ThrowIfNull(prices);

        if (prices.Count == 0)
        {
            throw new ArgumentException("At least one source price is required.", nameof(prices));
        }

        var first = prices.First();
        if (prices.Any(price =>
                price.Instrument != first.Instrument ||
                price.TimeUtc != first.TimeUtc ||
                !double.IsFinite(price.Price) ||
                price.Price <= 0))
        {
            throw new ArgumentException(
                "Source prices must be positive finite values for the same instrument and time.",
                nameof(prices));
        }

        return prices.Average(price => price.Price);
    }
}
