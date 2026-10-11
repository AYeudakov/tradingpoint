using BitcoinPrices.Application.Abstractions;
using BitcoinPrices.Domain.Entities;
using BitcoinPrices.Domain.Enums;
using BitcoinPrices.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace BitcoinPrices.Infrastructure.Persistence;

public sealed class PriceRepository(PriceDbContext dbContext) : IPriceRepository
{
    private readonly PriceDbContext _dbContext = dbContext;

    public Task<AggregatedPrice?> FindAggregateAsync(
        Instrument instrument,
        DateTimeOffset timeUtc,
        AggregationMethod aggregationMethod,
        int aggregationVersion,
        CancellationToken cancellationToken)
    {
        return _dbContext.AggregatedPrices
            .AsNoTracking()
            .Include(price => price.Sources)
            .SingleOrDefaultAsync(price =>
                    price.BaseCurrency == instrument.BaseCurrency &&
                    price.QuoteCurrency == instrument.QuoteCurrency &&
                    price.TimeUtc == timeUtc &&
                    price.AggregationMethod == aggregationMethod &&
                    price.AggregationVersion == aggregationVersion,
                cancellationToken);
    }

    public async Task<IReadOnlyCollection<AggregatedPrice>> GetAggregatesAsync(
        Instrument instrument,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        AggregationMethod aggregationMethod,
        int aggregationVersion,
        CancellationToken cancellationToken)
    {
        return await _dbContext.AggregatedPrices
            .AsNoTracking()
            .Include(price => price.Sources)
            .Where(price =>
                price.BaseCurrency == instrument.BaseCurrency &&
                price.QuoteCurrency == instrument.QuoteCurrency &&
                price.TimeUtc >= fromUtc &&
                price.TimeUtc < toUtc &&
                price.AggregationMethod == aggregationMethod &&
                price.AggregationVersion == aggregationVersion)
            .OrderBy(price => price.TimeUtc)
            .ToArrayAsync(cancellationToken);
    }

    public async Task AddAggregateAsync(
        AggregatedPrice aggregatedPrice,
        CancellationToken cancellationToken)
    {
        _dbContext.AggregatedPrices.Add(aggregatedPrice);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

}
