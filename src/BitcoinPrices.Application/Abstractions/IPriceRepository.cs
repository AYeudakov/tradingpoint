using BitcoinPrices.Domain.Entities;
using BitcoinPrices.Domain.Enums;
using BitcoinPrices.Domain.ValueObjects;

namespace BitcoinPrices.Application.Abstractions;

public interface IPriceRepository
{
    Task<AggregatedPrice?> FindAggregateAsync(
        Instrument instrument,
        DateTimeOffset timeUtc,
        AggregationMethod aggregationMethod,
        int aggregationVersion,
        CancellationToken cancellationToken);

    Task<IReadOnlyCollection<AggregatedPrice>> GetAggregatesAsync(
        Instrument instrument,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        AggregationMethod aggregationMethod,
        int aggregationVersion,
        CancellationToken cancellationToken);

    Task AddAggregateAsync(
        AggregatedPrice aggregatedPrice,
        CancellationToken cancellationToken);
}
