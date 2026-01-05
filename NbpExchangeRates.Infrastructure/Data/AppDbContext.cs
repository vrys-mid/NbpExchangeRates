using Microsoft.EntityFrameworkCore;
using NbpExchangeRates.Infrastructure.Entities;

namespace NbpExchangeRates.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<CurrencyRate> CurrencyRates => Set<CurrencyRate>();
}