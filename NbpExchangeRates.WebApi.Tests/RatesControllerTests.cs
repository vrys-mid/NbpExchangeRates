using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NbpExchangeRates.Infrastructure.Data;
using NbpExchangeRates.Infrastructure.Entities;
using NbpExchangeRatesApi.Contracts;
using NbpExchangeRatesApi.Controllers;

namespace NbpExchangeRates.WebApi.Tests;

public class RatesControllerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly RatesController _controller;

    public RatesControllerTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _controller = new RatesController(_context);
    }

    [Fact]
    public async Task GetLatest_ReturnsLatestRates_WhenDataExists()
    {
        var olderDate = new DateTime(2024, 1, 1);
        var latestDate = new DateTime(2024, 1, 2);

        _context.CurrencyRates.AddRange(
            new CurrencyRate { Code = "USD", Currency = "US Dollar", Mid = 4.00m, EffectiveDate = olderDate },
            new CurrencyRate { Code = "EUR", Currency = "Euro", Mid = 4.50m, EffectiveDate = olderDate },
            new CurrencyRate { Code = "USD", Currency = "US Dollar", Mid = 4.10m, EffectiveDate = latestDate },
            new CurrencyRate { Code = "EUR", Currency = "Euro", Mid = 4.60m, EffectiveDate = latestDate },
            new CurrencyRate { Code = "GBP", Currency = "Pound Sterling", Mid = 5.20m, EffectiveDate = latestDate }
        );
        await _context.SaveChangesAsync();

        var result = await _controller.GetLatest();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var rates = Assert.IsAssignableFrom<IEnumerable<CurrencyRateDto>>(okResult.Value);
        var ratesList = rates.ToList();

        Assert.Equal(3, ratesList.Count);
        Assert.All(ratesList, rate => Assert.Equal(latestDate, rate.EffectiveDate));
        
        Assert.Equal("EUR", ratesList[0].Code);
        Assert.Equal("GBP", ratesList[1].Code);
        Assert.Equal("USD", ratesList[2].Code);
    }

    [Fact]
    public async Task GetLatest_ReturnsEmptyList_WhenNoDataExists()
    {
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _controller.GetLatest()
        );

        Assert.Contains("Sequence contains no elements", exception.Message);
    }

    [Fact]
    public async Task GetLatest_ReturnsCorrectData_WithSingleRate()
    {
        var effectiveDate = new DateTime(2024, 1, 15);
        _context.CurrencyRates.Add(
            new CurrencyRate 
            { 
                Code = "CHF", 
                Currency = "Swiss Franc", 
                Mid = 4.75m, 
                EffectiveDate = effectiveDate 
            }
        );
        await _context.SaveChangesAsync();

        var result = await _controller.GetLatest();

        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var rates = Assert.IsAssignableFrom<IEnumerable<CurrencyRateDto>>(okResult.Value);
        var rate = rates.Single();

        Assert.Equal("CHF", rate.Code);
        Assert.Equal("Swiss Franc", rate.Currency);
        Assert.Equal(4.75m, rate.Mid);
        Assert.Equal(effectiveDate, rate.EffectiveDate);
    }

    [Fact]
    public async Task GetHistory_ReturnsHistoricalData_WhenCodeExists()
    {
        var code = "USD";
        _context.CurrencyRates.AddRange(
            new CurrencyRate { Code = code, Currency = "US Dollar", Mid = 4.00m, EffectiveDate = new DateTime(2024, 1, 1) },
            new CurrencyRate { Code = code, Currency = "US Dollar", Mid = 4.10m, EffectiveDate = new DateTime(2024, 1, 2) },
            new CurrencyRate { Code = code, Currency = "US Dollar", Mid = 4.05m, EffectiveDate = new DateTime(2024, 1, 3) },
            new CurrencyRate { Code = "EUR", Currency = "Euro", Mid = 4.50m, EffectiveDate = new DateTime(2024, 1, 1) }
        );
        await _context.SaveChangesAsync();

        var result = await _controller.GetHistory(code);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var history = Assert.IsAssignableFrom<IEnumerable<CurrencyRateHistoryDto>>(okResult.Value);
        var historyList = history.ToList();

        Assert.Equal(3, historyList.Count);
        Assert.Equal(4.00m, historyList[0].Rate);
        Assert.Equal(new DateTime(2024, 1, 1), historyList[0].Date);
        Assert.Equal(4.10m, historyList[1].Rate);
        Assert.Equal(4.05m, historyList[2].Rate);
    }

    [Fact]
    public async Task GetHistory_ReturnsEmptyList_WhenCodeDoesNotExist()
    {
        _context.CurrencyRates.Add(
            new CurrencyRate { Code = "USD", Currency = "US Dollar", Mid = 4.00m, EffectiveDate = new DateTime(2024, 1, 1) }
        );
        await _context.SaveChangesAsync();

        var result = await _controller.GetHistory("EUR");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var history = Assert.IsAssignableFrom<IEnumerable<CurrencyRateHistoryDto>>(okResult.Value);
        Assert.Empty(history);
    }

    [Fact]
    public async Task GetHistory_ReturnsDataInChronologicalOrder()
    {
        var code = "GBP";
        _context.CurrencyRates.AddRange(
            new CurrencyRate { Code = code, Currency = "Pound", Mid = 5.30m, EffectiveDate = new DateTime(2024, 1, 5) },
            new CurrencyRate { Code = code, Currency = "Pound", Mid = 5.10m, EffectiveDate = new DateTime(2024, 1, 1) },
            new CurrencyRate { Code = code, Currency = "Pound", Mid = 5.20m, EffectiveDate = new DateTime(2024, 1, 3) }
        );
        await _context.SaveChangesAsync();

        var result = await _controller.GetHistory(code);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var history = Assert.IsAssignableFrom<IEnumerable<CurrencyRateHistoryDto>>(okResult.Value);
        var historyList = history.ToList();

        Assert.Equal(3, historyList.Count);
        Assert.Equal(new DateTime(2024, 1, 1), historyList[0].Date);
        Assert.Equal(new DateTime(2024, 1, 3), historyList[1].Date);
        Assert.Equal(new DateTime(2024, 1, 5), historyList[2].Date);
    }

    [Fact]
    public async Task GetHistory_IsCaseSensitive()
    {
        _context.CurrencyRates.AddRange(
            new CurrencyRate { Code = "USD", Currency = "US Dollar", Mid = 4.00m, EffectiveDate = new DateTime(2024, 1, 1) },
            new CurrencyRate { Code = "usd", Currency = "lowercase", Mid = 3.00m, EffectiveDate = new DateTime(2024, 1, 1) }
        );
        await _context.SaveChangesAsync();

        var resultUpper = await _controller.GetHistory("USD");
        var resultLower = await _controller.GetHistory("usd");

        var okResultUpper = Assert.IsType<OkObjectResult>(resultUpper);
        var historyUpper = Assert.IsAssignableFrom<IEnumerable<CurrencyRateHistoryDto>>(okResultUpper.Value);
        Assert.Single(historyUpper);
        Assert.Equal(4.00m, historyUpper.First().Rate);

        var okResultLower = Assert.IsType<OkObjectResult>(resultLower);
        var historyLower = Assert.IsAssignableFrom<IEnumerable<CurrencyRateHistoryDto>>(okResultLower.Value);
        Assert.Single(historyLower);
        Assert.Equal(3.00m, historyLower.First().Rate);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}