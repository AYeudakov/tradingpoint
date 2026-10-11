using BitcoinPrices.Domain.Enums;

namespace BitcoinPrices.Application.Abstractions;

public interface IPriceAggregatorResolver
{
    IPriceAggregator Resolve(AggregationMethod method);
}
