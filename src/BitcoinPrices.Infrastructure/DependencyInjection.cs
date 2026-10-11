using BitcoinPrices.Application.Abstractions;
using BitcoinPrices.Infrastructure.Configuration;
using BitcoinPrices.Infrastructure.Persistence;
using BitcoinPrices.Infrastructure.Providers.Bitfinex;
using BitcoinPrices.Infrastructure.Providers.Bitstamp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BitcoinPrices.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<PriceDbContext>(options =>
            options.UseSqlite(configuration.GetConnectionString("Prices")));

        services.AddScoped<IPriceRepository, PriceRepository>();

        AddProvider<BitstampPriceProvider>(services, configuration, "Bitstamp");
        AddProvider<BitfinexPriceProvider>(services, configuration, "Bitfinex");

        return services;
    }

    private static void AddProvider<TProvider>(
        IServiceCollection services,
        IConfiguration configuration,
        string configurationName)
        where TProvider : class, IPriceProvider
    {
        var section = configuration.GetSection($"PriceProviders:{configurationName}");
        services.Configure<PriceProviderOptions>(configurationName, section);

        var options = section.Get<PriceProviderOptions>()
            ?? throw new InvalidOperationException(
                $"Missing configuration for price provider '{configurationName}'.");

        services.AddHttpClient<TProvider>(client =>
        {
            client.BaseAddress = new Uri(options.BaseUrl);
            client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
        });

        services.AddTransient<IPriceProvider>(provider =>
            provider.GetRequiredService<TProvider>());
    }
}
