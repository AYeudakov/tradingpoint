using BitcoinPrices.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BitcoinPrices.Infrastructure.Persistence;

public sealed class PriceDbContext(DbContextOptions<PriceDbContext> options)
    : DbContext(options)
{
    public DbSet<AggregatedPrice> AggregatedPrices => Set<AggregatedPrice>();
    public DbSet<SourcePrice> SourcePrices => Set<SourcePrice>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PriceDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
