using BitcoinPrices.Application.Abstractions;
using BitcoinPrices.Application.Models;
using BitcoinPrices.Domain.Enums;
using BitcoinPrices.Domain.ValueObjects;
using Microsoft.AspNetCore.Mvc;

namespace BitcoinPrices.Api.Controllers;

[ApiController]
[Route("api/prices")]
public sealed class PricesController(IPriceService priceService) : ControllerBase
{
    private readonly IPriceService _priceService = priceService;

    [HttpGet]
    [ProducesResponseType<AggregatedPriceResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<AggregatedPriceResponse>> GetAggregatedPrice(
        [FromQuery] string baseCurrency,
        [FromQuery] string quoteCurrency,
        [FromQuery] DateTimeOffset timeUtc,
        CancellationToken cancellationToken,
        [FromQuery] AggregationMethod aggregationMethod = AggregationMethod.ArithmeticMean)
    {
        var instrument = new Instrument(
            new Currency(baseCurrency),
            new Currency(quoteCurrency));

        var response = await _priceService.GetAggregatedPriceAsync(
            instrument,
            timeUtc,
            aggregationMethod,
            cancellationToken);

        return Ok(response);
    }

    [HttpGet("history")]
    [ProducesResponseType<PriceRangeResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<PriceRangeResponse>> GetPersistedPrices(
        [FromQuery] string baseCurrency,
        [FromQuery] string quoteCurrency,
        [FromQuery] DateTimeOffset fromUtc,
        [FromQuery] DateTimeOffset toUtc,
        CancellationToken cancellationToken,
        [FromQuery] AggregationMethod aggregationMethod = AggregationMethod.ArithmeticMean)
    {
        var instrument = new Instrument(
            new Currency(baseCurrency),
            new Currency(quoteCurrency));

        var response = await _priceService.GetPersistedPricesAsync(
            instrument,
            fromUtc,
            toUtc,
            aggregationMethod,
            cancellationToken);

        return Ok(response);
    }
}
