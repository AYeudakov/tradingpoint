using BitcoinPrices.Application.Abstractions;
using BitcoinPrices.Application.Configuration;
using BitcoinPrices.Application.Exceptions;
using BitcoinPrices.Application.Models;
using BitcoinPrices.Application.Validation;
using BitcoinPrices.Domain.Entities;
using BitcoinPrices.Domain.Enums;
using BitcoinPrices.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BitcoinPrices.Application.Services;

public sealed class PriceService(
    IEnumerable<IPriceProvider> providers,
    IPriceAggregatorResolver aggregatorResolver,
    IPriceRepository repository,
    IOptions<AggregationOptions> options,
    TimeProvider timeProvider,
    ILogger<PriceService> logger) : IPriceService
{
    private readonly IReadOnlyCollection<IPriceProvider> _providers = providers.ToArray();
    private readonly IPriceAggregatorResolver _aggregatorResolver = aggregatorResolver;
    private readonly IPriceRepository _repository = repository;
    private readonly AggregationOptions _options = options.Value;
    private readonly TimeProvider _timeProvider = timeProvider;
    private readonly ILogger<PriceService> _logger = logger;

    // TODO: Add keyed single-flight coordination before supporting concurrent or multiple service instances or even within 1.
    public async Task<AggregatedPriceResponse> GetAggregatedPriceAsync(
        Instrument instrument,
        DateTimeOffset timeUtc,
        AggregationMethod aggregationMethod,
        CancellationToken cancellationToken)
    {
        instrument.EnsureSupported();
        timeUtc.EnsureUtcHour();
        var aggregator = _aggregatorResolver.Resolve(aggregationMethod);

        if (timeUtc > _timeProvider.GetUtcNow())
        {
            throw new ArgumentException("The requested time cannot be in the future.", nameof(timeUtc));
        }

        var storedPrice = await _repository.FindAggregateAsync(
            instrument,
            timeUtc,
            aggregator.Method,
            aggregator.Version,
            cancellationToken);

        if (storedPrice is not null)
        {
            return AggregatedPriceResponse.Map(storedPrice, ResultSource.Datastore);
        }

        var fetchTasks = _providers.Select(provider =>
            FetchSafelyAsync(provider, instrument, timeUtc, cancellationToken));
        var providerResults = await Task.WhenAll(fetchTasks); // we can do foreachAsync later with specified parallel for now do WhenAll
        var fetchedPrices = providerResults
            .Where(result => result.Price is not null)
            .Select(result => result.Price!)
            .ToArray();

        if (fetchedPrices.Length < _options.MinimumSuccessfulProviders)
        {
            throw new ExternalPricesUnavailableException(
                fetchedPrices.Length,
                _options.MinimumSuccessfulProviders);
        }

        var entity = new AggregatedPrice
        {
            BaseCurrency = instrument.BaseCurrency,
            QuoteCurrency = instrument.QuoteCurrency,
            TimeUtc = timeUtc,
            Price = aggregator.Aggregate(fetchedPrices),
            AggregationMethod = aggregator.Method,
            AggregationVersion = aggregator.Version,
            CreatedAtUtc = _timeProvider.GetUtcNow(),
            Sources = fetchedPrices.Select(price => new SourcePrice
            {
                ProviderName = price.ProviderName,
                Price = price.Price,
                RetrievedAtUtc = price.RetrievedAtUtc
            }).ToList()
        };

        await _repository.AddAggregateAsync(entity, cancellationToken);
        return AggregatedPriceResponse.Map(entity, ResultSource.ExternalApis);
    }

    public async Task<PriceRangeResponse> GetPersistedPricesAsync(
        Instrument instrument,
        DateTimeOffset fromUtc,
        DateTimeOffset toUtc,
        AggregationMethod aggregationMethod,
        CancellationToken cancellationToken)
    {
        instrument.EnsureSupported();
        fromUtc.EnsureUtcHour();
        toUtc.EnsureUtcHour();
        var aggregator = _aggregatorResolver.Resolve(aggregationMethod);

        if (fromUtc >= toUtc)
        {
            throw new ArgumentException("The range start must be earlier than the range end.");
        }

        var prices = await _repository.GetAggregatesAsync(
            instrument,
            fromUtc,
            toUtc,
            aggregator.Method,
            aggregator.Version,
            cancellationToken);

        return new PriceRangeResponse(
            instrument.ToString(),
            fromUtc,
            toUtc,
            ResultSource.Datastore,
            prices.Select(price =>
                AggregatedPriceResponse.Map(price, ResultSource.Datastore)).ToArray());
    }

    private async Task<ProviderFetchResult> FetchSafelyAsync(
        IPriceProvider provider,
        Instrument instrument,
        DateTimeOffset timeUtc,
        CancellationToken cancellationToken)
    {
        try
        {
            return new ProviderFetchResult(
                await provider.GetPriceAsync(instrument, timeUtc, cancellationToken));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Price provider {ProviderName} failed for {Instrument} at {TimeUtc}.",
                provider.Name,
                instrument,
                timeUtc);

            return new ProviderFetchResult(null);
        }
    }

    private sealed record ProviderFetchResult(ProviderPrice? Price);
}
