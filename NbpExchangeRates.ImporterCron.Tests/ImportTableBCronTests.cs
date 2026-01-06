using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Nbp.Rates.Importer.Jobs;
using Nbp.Rates.Importer.Services;
using NbpExchangeRates.Infrastructure.Data;
using NbpExchangeRates.Infrastructure.Entities;
using NbpImporter.Nbp;

namespace NbpExchangeRates.ImporterCron.Tests;

public class ImportTableBJobTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly Mock<INbpApiClient> _apiClientMock;
    private readonly ImportTableBJob _job;
    private readonly ILogger<ImportTableBJob> _logger;

    public ImportTableBJobTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _apiClientMock = new Mock<INbpApiClient>();
        _logger = new NullLogger<ImportTableBJob>();
        _job = new ImportTableBJob(_context, _apiClientMock.Object, _logger);
    }

    [Fact]
    public async Task RunAsync_ImportsNewRates_WhenApiReturnsValidData()
    {
        var apiResponse = new List<NbpTableResponse>
        {
            new()
            {
                Table = "B",
                EffectiveDate = "2024-01-15",
                Rates = new List<NbpRate>
                {
                    new() { Currency = "US Dollar", Code = "USD", Mid = 4.00m },
                    new() { Currency = "Euro", Code = "EUR", Mid = 4.50m }
                }
            }
        };

        _apiClientMock.Setup(x => x.GetTableBAsync())
            .ReturnsAsync(apiResponse);

        await _job.RunAsync();

        var rates = await _context.CurrencyRates.ToListAsync();
        Assert.Equal(2, rates.Count);

        var usdRate = rates.First(r => r.Code == "USD");
        Assert.Equal("US Dollar", usdRate.Currency);
        Assert.Equal(4.00m, usdRate.Mid);
        Assert.Equal(new DateTime(2024, 1, 15), usdRate.EffectiveDate);
        Assert.Equal("B", usdRate.Table);

        var eurRate = rates.First(r => r.Code == "EUR");
        Assert.Equal("Euro", eurRate.Currency);
        Assert.Equal(4.50m, eurRate.Mid);

        _apiClientMock.Verify(x => x.GetTableBAsync(), Times.Once);
    }

    [Fact]
    public async Task RunAsync_DeletesExistingRates_BeforeImportingNew()
    {
        var effectiveDate = new DateTime(2024, 1, 15);
        
        _context.CurrencyRates.AddRange(
            new CurrencyRate( "USD", "Old US Dollar", 3.50m, effectiveDate),
            new CurrencyRate( "EUR", "Old Euro", 4.00m, effectiveDate)
        );
        await _context.SaveChangesAsync();

        var apiResponse = new List<NbpTableResponse>
        {
            new()
            {
                Table = "B",
                EffectiveDate = "2024-01-15",
                Rates = new List<NbpRate>
                {
                    new() { Currency = "New US Dollar", Code = "USD", Mid = 4.10m }
                }
            }
        };

        _apiClientMock.Setup(x => x.GetTableBAsync())
            .ReturnsAsync(apiResponse);

        await _job.RunAsync();

        var rates = await _context.CurrencyRates.Where(r => r.Table == "B").ToListAsync();
        Assert.Single(rates);
        Assert.Equal("New US Dollar", rates[0].Currency);
        Assert.Equal(4.10m, rates[0].Mid);
    }

    [Fact]
    public async Task RunAsync_PreservesOtherTableData_WhenImportingTableB()
    {
        var effectiveDate = new DateTime(2024, 1, 15);
        
        _context.CurrencyRates.AddRange(
            new CurrencyRate( "USD", "Table A USD", 3.90m, effectiveDate, "A")
        );
        await _context.SaveChangesAsync();

        var apiResponse = new List<NbpTableResponse>
        {
            new()
            {
                Table = "B",
                EffectiveDate = "2024-01-15",
                Rates = new List<NbpRate>
                {
                    new() { Currency = "Table B USD", Code = "USD", Mid = 4.00m }
                }
            }
        };

        _apiClientMock.Setup(x => x.GetTableBAsync())
            .ReturnsAsync(apiResponse);

        await _job.RunAsync();

        var tableARates = await _context.CurrencyRates.Where(r => r.Table == "A").ToListAsync();
        Assert.Single(tableARates);
        Assert.Equal("Table A USD", tableARates[0].Currency);

        var tableBRates = await _context.CurrencyRates.Where(r => r.Table == "B").ToListAsync();
        Assert.Single(tableBRates);
        Assert.Equal("Table B USD", tableBRates[0].Currency);
    }

    [Fact]
    public async Task RunAsync_PreservesOtherDates_WhenImportingSpecificDate()
    {
        var oldDate = new DateTime(2024, 1, 10);
        var newDate = new DateTime(2024, 1, 15);

        _context.CurrencyRates.Add(
            new CurrencyRate("USD", "Old Date USD", 3.80m, oldDate));
        await _context.SaveChangesAsync();

        var apiResponse = new List<NbpTableResponse>
        {
            new()
            {
                Table = "B",
                EffectiveDate = "2024-01-15",
                Rates = new List<NbpRate>
                {
                    new() { Currency = "New Date USD", Code = "USD", Mid = 4.00m }
                }
            }
        };

        _apiClientMock.Setup(x => x.GetTableBAsync())
            .ReturnsAsync(apiResponse);

        await _job.RunAsync();

        var rates = await _context.CurrencyRates.OrderBy(r => r.EffectiveDate).ToListAsync();
        Assert.Equal(2, rates.Count);
        Assert.Equal(oldDate, rates[0].EffectiveDate);
        Assert.Equal("Old Date USD", rates[0].Currency);
        Assert.Equal(newDate, rates[1].EffectiveDate);
        Assert.Equal("New Date USD", rates[1].Currency);
    }

    [Fact]
    public async Task RunAsync_HandlesMultipleCurrencies_Correctly()
    {
        var apiResponse = new List<NbpTableResponse>
        {
            new()
            {
                Table = "B",
                EffectiveDate = "2024-01-15",
                Rates = new List<NbpRate>
                {
                    new() { Currency = "US Dollar", Code = "USD", Mid = 4.00m },
                    new() { Currency = "Euro", Code = "EUR", Mid = 4.50m },
                    new() { Currency = "British Pound", Code = "GBP", Mid = 5.20m },
                    new() { Currency = "Swiss Franc", Code = "CHF", Mid = 4.75m }
                }
            }
        };

        _apiClientMock.Setup(x => x.GetTableBAsync())
            .ReturnsAsync(apiResponse);

        await _job.RunAsync();

        var rates = await _context.CurrencyRates.ToListAsync();
        Assert.Equal(4, rates.Count);
        Assert.Contains(rates, r => r.Code == "USD" && r.Mid == 4.00m);
        Assert.Contains(rates, r => r.Code == "EUR" && r.Mid == 4.50m);
        Assert.Contains(rates, r => r.Code == "GBP" && r.Mid == 5.20m);
        Assert.Contains(rates, r => r.Code == "CHF" && r.Mid == 4.75m);
    }

    [Fact]
    public async Task RunAsync_ParsesDateCorrectly_WithDifferentFormats()
    {
        var apiResponse = new List<NbpTableResponse>
        {
            new()
            {
                Table = "B",
                EffectiveDate = "2024-03-25",
                Rates = new List<NbpRate>
                {
                    new() { Currency = "US Dollar", Code = "USD", Mid = 4.00m }
                }
            }
        };

        _apiClientMock.Setup(x => x.GetTableBAsync())
            .ReturnsAsync(apiResponse);

        await _job.RunAsync();

        var rate = await _context.CurrencyRates.SingleAsync();
        Assert.Equal(new DateTime(2024, 3, 25), rate.EffectiveDate);
    }

    [Fact]
    public async Task RunAsync_ThrowsException_WhenApiReturnsMultipleTables()
    {
        var apiResponse = new List<NbpTableResponse>
        {
            new()
            {
                Table = "B",
                EffectiveDate = "2024-01-15",
                Rates = new List<NbpRate>()
            },
            new()
            {
                Table = "B",
                EffectiveDate = "2024-01-16",
                Rates = new List<NbpRate>()
            }
        };

        _apiClientMock.Setup(x => x.GetTableBAsync())
            .ReturnsAsync(apiResponse);

        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _job.RunAsync()
        );
    }

    [Fact]
    public async Task RunAsync_HandlesDecimalPrecision_Correctly()
    {
        var apiResponse = new List<NbpTableResponse>
        {
            new()
            {
                Table = "B",
                EffectiveDate = "2024-01-15",
                Rates = new List<NbpRate>
                {
                    new() { Currency = "Test Currency", Code = "TST", Mid = 3.14159265m }
                }
            }
        };

        _apiClientMock.Setup(x => x.GetTableBAsync())
            .ReturnsAsync(apiResponse);

        await _job.RunAsync();

        var rate = await _context.CurrencyRates.SingleAsync();
        Assert.Equal(3.14159265m, rate.Mid);
    }

    [Fact]
    public async Task RunAsync_HandlesEmptyRatesList_Correctly()
    {
        var apiResponse = new List<NbpTableResponse>
        {
            new()
            {
                Table = "B",
                EffectiveDate = "2024-01-15",
                Rates = new List<NbpRate>()
            }
        };

        _apiClientMock.Setup(x => x.GetTableBAsync())
            .ReturnsAsync(apiResponse);

        await _job.RunAsync();

        var rates = await _context.CurrencyRates.ToListAsync();
        Assert.Empty(rates);
    }

    [Fact]
    public async Task RunAsync_PropagatesException_WhenApiClientThrows()
    {
        _apiClientMock.Setup(x => x.GetTableBAsync())
            .ThrowsAsync(new HttpRequestException("Network error"));

        await Assert.ThrowsAsync<HttpRequestException>(
            async () => await _job.RunAsync()
        );
    }

    [Fact]
    public async Task RunAsync_SetsTableProperty_ForAllRates()
    {
        var apiResponse = new List<NbpTableResponse>
        {
            new()
            {
                Table = "B",
                EffectiveDate = "2024-01-15",
                Rates = new List<NbpRate>
                {
                    new() { Currency = "US Dollar", Code = "USD", Mid = 4.00m },
                    new() { Currency = "Euro", Code = "EUR", Mid = 4.50m }
                }
            }
        };

        _apiClientMock.Setup(x => x.GetTableBAsync())
            .ReturnsAsync(apiResponse);

        await _job.RunAsync();

        var rates = await _context.CurrencyRates.ToListAsync();
        Assert.All(rates, rate => Assert.Equal("B", rate.Table));
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}