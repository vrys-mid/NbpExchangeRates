using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using NbpExchangeRatesApi.Contracts;
using NbpExchangeRatesApi.Controllers;
using NbpExchangeRatesApi.Services;

namespace NbpExchangeRates.WebApi.Tests;

public class RatesControllerTests
{
    private readonly Mock<IRatesService> _ratesServiceMock;
    private readonly Mock<ILogger<RatesController>> _loggerMock;
    private readonly RatesController _controller;

    public RatesControllerTests()
    {
        _ratesServiceMock = new Mock<IRatesService>();
        _loggerMock = new Mock<ILogger<RatesController>>();
        _controller = new RatesController(_ratesServiceMock.Object, _loggerMock.Object);
    }

    [Fact]
    public async Task GetRates_ReturnsRates_WhenDataExists()
    {
        var latestDate = new DateTime(2024, 1, 2);
        var rates = new List<CurrencyRateDto>
        {
            new CurrencyRateDto("USD", "US Dollar", 4.10m, latestDate),
            new CurrencyRateDto("EUR", "Euro", 4.60m, latestDate),
            new CurrencyRateDto("GBP", "Pound Sterling", 5.20m, latestDate)
        };

        _ratesServiceMock.Setup(s => s.GetRatesByDateAsync(null))
            .ReturnsAsync(rates);

        var result = await _controller.GetRates(null);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedRates = Assert.IsAssignableFrom<IEnumerable<CurrencyRateDto>>(okResult.Value);
        Assert.Equal(3, returnedRates.Count());
        Assert.All(returnedRates, r => Assert.Equal(latestDate, r.EffectiveDate));
    }

    [Fact]
    public async Task GetRates_ReturnsEmptyList_WhenNoDataExists()
    {
        _ratesServiceMock.Setup(s => s.GetRatesByDateAsync(null))
            .ReturnsAsync(new List<CurrencyRateDto>());

        var result = await _controller.GetRates(null);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var rates = Assert.IsAssignableFrom<IEnumerable<CurrencyRateDto>>(okResult.Value);
        Assert.Empty(rates);
    }

    [Fact]
    public async Task GetHistory_ReturnsHistoricalData_WhenCodeExists()
    {
        var code = "USD";
        var history = new List<CurrencyRateHistoryDto>
        {
            new CurrencyRateHistoryDto(new DateTime(2024, 1, 1), 4.00m),
            new CurrencyRateHistoryDto(new DateTime(2024, 1, 2), 4.10m),
            new CurrencyRateHistoryDto(new DateTime(2024, 1, 3), 4.05m)
        };

        _ratesServiceMock.Setup(s => s.GetCurrencyHistoryAsync(code))
            .ReturnsAsync(history);

        var result = await _controller.GetHistory(code);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedHistory = Assert.IsAssignableFrom<IEnumerable<CurrencyRateHistoryDto>>(okResult.Value);
        Assert.Equal(3, returnedHistory.Count());
        Assert.Equal(4.00m, returnedHistory.First().Rate);
    }

    [Fact]
    public async Task GetHistory_ReturnsEmptyList_WhenCodeDoesNotExist()
    {
        _ratesServiceMock.Setup(s => s.GetCurrencyHistoryAsync("EUR"))
            .ReturnsAsync(new List<CurrencyRateHistoryDto>());

        var result = await _controller.GetHistory("EUR");

        var okResult = Assert.IsType<OkObjectResult>(result);
        var history = Assert.IsAssignableFrom<IEnumerable<CurrencyRateHistoryDto>>(okResult.Value);
        Assert.Empty(history);
    }

    [Fact]
    public async Task GetHistory_IsCaseSensitive()
    {
        _ratesServiceMock.Setup(s => s.GetCurrencyHistoryAsync("USD"))
            .ReturnsAsync(new List<CurrencyRateHistoryDto>
            {
                new CurrencyRateHistoryDto(new DateTime(2024,1,1), 4.00m)
            });

        _ratesServiceMock.Setup(s => s.GetCurrencyHistoryAsync("usd"))
            .ReturnsAsync(new List<CurrencyRateHistoryDto>
            {
                new CurrencyRateHistoryDto(new DateTime(2024,1,1), 3.00m)            });

        var resultUpper = await _controller.GetHistory("USD");
        var resultLower = await _controller.GetHistory("usd");

        var upperResult = Assert.IsType<OkObjectResult>(resultUpper);
        var upperHistory = Assert.IsAssignableFrom<IEnumerable<CurrencyRateHistoryDto>>(upperResult.Value);
        Assert.Single(upperHistory);
        Assert.Equal(4.00m, upperHistory.First().Rate);

        var lowerResult = Assert.IsType<OkObjectResult>(resultLower);
        var lowerHistory = Assert.IsAssignableFrom<IEnumerable<CurrencyRateHistoryDto>>(lowerResult.Value);
        Assert.Single(lowerHistory);
        Assert.Equal(3.00m, lowerHistory.First().Rate);
    }
}
