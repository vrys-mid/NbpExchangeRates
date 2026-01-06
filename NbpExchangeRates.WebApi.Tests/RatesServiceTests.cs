using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NbpExchangeRates.Infrastructure.Data;
using NbpExchangeRates.Infrastructure.Entities;

namespace NbpExchangeRates.WebApi.Tests;

public class RatesServiceTests : IDisposable
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;
    private readonly RatesService _service;

    public RatesServiceTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _db = new AppDbContext(options);
        _cache = new MemoryCache(new MemoryCacheOptions());
        _service = new RatesService(_db, _cache);
    }

    [Fact]
    public async Task GetRatesByDateAsync_ReturnsLatestRates_WhenDateIsNull()
    {
        var olderDate = new DateTime(2024, 1, 1);
        var latestDate = new DateTime(2024, 1, 2);

        _db.CurrencyRates.AddRange(
            new CurrencyRate("USD", "US Dollar", 4.00m, olderDate),
            new CurrencyRate("EUR", "Euro", 4.50m, olderDate),
            new CurrencyRate("USD", "US Dollar", 4.10m, latestDate),
            new CurrencyRate("EUR", "Euro", 4.60m, latestDate),
            new CurrencyRate("GBP", "Pound Sterling", 5.20m, latestDate)
        );
        await _db.SaveChangesAsync();

        var rates = await _service.GetRatesByDateAsync(null);

        Assert.Equal(3, rates.Count);
        Assert.All(rates, r => Assert.Equal(latestDate, r.EffectiveDate));
        Assert.Equal(new[] { "EUR", "GBP", "USD" }, rates.Select(r => r.Code));
    }

    [Fact]
    public async Task GetRatesByDateAsync_ReturnsRatesForSpecificDate()
    {
        var targetDate = new DateTime(2024, 1, 1);
        _db.CurrencyRates.AddRange(
            new CurrencyRate("USD", "US Dollar", 4.00m, targetDate),
            new CurrencyRate("EUR", "Euro", 4.50m, targetDate)
        );
        await _db.SaveChangesAsync();

        var rates = await _service.GetRatesByDateAsync(targetDate);

        Assert.Equal(2, rates.Count);
        Assert.All(rates, r => Assert.Equal(targetDate, r.EffectiveDate));
    }

    [Fact]
    public async Task GetPublicationDatesAsync_ReturnsDistinctOrderedDates()
    {
        _db.CurrencyRates.AddRange(
            new CurrencyRate("USD", "US Dollar", 4.00m, new DateTime(2024, 1, 1)),
            new CurrencyRate("EUR", "Euro", 4.50m, new DateTime(2024, 1, 2)),
            new CurrencyRate("GBP", "Pound Sterling", 5.20m, new DateTime(2024, 1, 1))
        );
        await _db.SaveChangesAsync();

        var dates = await _service.GetPublicationDatesAsync();

        Assert.Equal(2, dates.Count);
        Assert.Equal(new[] { new DateTime(2024, 1, 2), new DateTime(2024, 1, 1) }, dates);
    }

    [Fact]
    public async Task GetCurrencyHistoryAsync_ReturnsHistoricalRates_ForExistingCode()
    {
        var code = "USD";
        _db.CurrencyRates.AddRange(
            new CurrencyRate(code, "US Dollar", 4.00m, new DateTime(2024, 1, 1)),
            new CurrencyRate(code, "US Dollar", 4.10m, new DateTime(2024, 1, 2)),
            new CurrencyRate(code, "US Dollar", 4.05m, new DateTime(2024, 1, 3)),
            new CurrencyRate("EUR", "Euro", 4.50m, new DateTime(2024, 1, 1))
        );
        await _db.SaveChangesAsync();

        var history = await _service.GetCurrencyHistoryAsync(code);

        Assert.Equal(3, history.Count);
        Assert.Equal(new[] { 4.00m, 4.10m, 4.05m }, history.Select(h => h.Rate));
        Assert.Equal(new[] { new DateTime(2024,1,1), new DateTime(2024,1,2), new DateTime(2024,1,3) }, history.Select(h => h.Date));
    }

    [Fact]
    public async Task GetCurrencyHistoryAsync_ReturnsEmptyList_ForNonExistingCode()
    {
        _db.CurrencyRates.Add(
            new CurrencyRate("USD", "US Dollar", 4.00m, new DateTime(2024, 1, 1))
        );
        await _db.SaveChangesAsync();

        var history = await _service.GetCurrencyHistoryAsync("EUR");

        Assert.Empty(history);
    }

    [Fact]
    public async Task GetCurrencyHistoryAsync_IsCaseSensitive()
    {
        _db.CurrencyRates.AddRange(
            new CurrencyRate("USD", "US Dollar", 4.00m, new DateTime(2024, 1, 1)),
            new CurrencyRate("usd", "lowercase", 3.00m, new DateTime(2024, 1, 1))
        );
        await _db.SaveChangesAsync();

        var upper = await _service.GetCurrencyHistoryAsync("USD");
        var lower = await _service.GetCurrencyHistoryAsync("usd");

        Assert.Single(upper);
        Assert.Equal(4.00m, upper.First().Rate);

        Assert.Single(lower);
        Assert.Equal(3.00m, lower.First().Rate);
    }

    public void Dispose()
    {
        _db.Database.EnsureDeleted();
        _db.Dispose();
    }
}
