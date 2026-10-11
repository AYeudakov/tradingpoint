using BitcoinPrices.Application.Abstractions;
using BitcoinPrices.Domain.Enums;

namespace BitcoinPrices.Application.Aggregation;

public sealed class PriceAggregatorResolver : IPriceAggregatorResolver
{
    private readonly IReadOnlyDictionary<AggregationMethod, IPriceAggregator> _aggregators;

    public PriceAggregatorResolver(IEnumerable<IPriceAggregator> aggregators)
    {
        _aggregators = aggregators.ToDictionary(aggregator => aggregator.Method);
    }

    public IPriceAggregator Resolve(AggregationMethod method)
    {
        return _aggregators.TryGetValue(method, out var aggregator)
            ? aggregator
            : throw new ArgumentException(
                $"Aggregation method '{method}' is not supported.",
                nameof(method));
    }
}
