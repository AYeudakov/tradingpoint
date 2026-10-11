using BitcoinPrices.Application.Models;
using BitcoinPrices.Domain.Enums;
using BitcoinPrices.Domain.ValueObjects;

namespace BitcoinPrices.Application.Abstractions;

public interface IPriceService
{
    Task<AggregatedPriceResponse> GetAggregatedPriceAsync(
        Instrument instrument,
        DateTimeOffset timeUtc,
        AggregationMethod aggregationMethod,
        CancellationToken cancellationToken);

    Task<PriceRangeResponse> GetPersistedPricesAsync(
        Instrument instrument,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        AggregationMethod aggregationMethod,
        CancellationToken cancellationToken);
}
