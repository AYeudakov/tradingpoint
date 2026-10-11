using BitcoinPrices.Application.Models;
using BitcoinPrices.Domain.Enums;

namespace BitcoinPrices.Application.Abstractions;

public interface IPriceAggregator
{
    AggregationMethod Method { get; }
    int Version { get; }

    double Aggregate(IReadOnlyCollection<ProviderPrice> prices);
}
