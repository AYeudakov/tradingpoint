using BitcoinPrices.Application.Abstractions;
using BitcoinPrices.Application.Aggregation;
using BitcoinPrices.Application.Configuration;
using BitcoinPrices.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BitcoinPrices.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<AggregationOptions>()
            .Bind(configuration.GetSection(AggregationOptions.SectionName))
            .Validate(
                options => options.MinimumSuccessfulProviders > 0,
                "MinimumSuccessfulProviders must be greater than zero.")
            .ValidateOnStart();

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IPriceAggregator, ArithmeticMeanPriceAggregator>();
        services.AddSingleton<IPriceAggregatorResolver, PriceAggregatorResolver>();
        services.AddScoped<IPriceService, PriceService>();

        return services;
    }
}
