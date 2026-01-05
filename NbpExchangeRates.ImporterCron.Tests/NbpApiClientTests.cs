using System.Net;
using System.Text.Json;
using Moq;
using Moq.Protected;
using Nbp.Rates.Importer.Services;
using NbpImporter.Nbp;

namespace NbpExchangeRates.ImporterCron.Tests.Services;

public class NbpApiClientTests : IDisposable
{
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly NbpApiClient _apiClient;

    public NbpApiClientTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri("https://api.nbp.pl/")
        };
        _apiClient = new NbpApiClient(_httpClient);
    }

    [Fact]
    public async Task GetTableBAsync_ReturnsData_WhenApiReturnsValidResponse()
    {
        var expectedResponse = new List<NbpTableResponse>
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

        SetupHttpResponse(HttpStatusCode.OK, expectedResponse);

        var result = await _apiClient.GetTableBAsync();

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("B", result[0].Table);
        Assert.Equal("2024-01-15", result[0].EffectiveDate);
        Assert.Equal(2, result[0].Rates.Count);
        
        VerifyHttpCall("api/exchangerates/tables/B/?format=json");
    }

    [Fact]
    public async Task GetTableBAsync_ThrowsException_WhenApiReturnsEmptyList()
    {
        var emptyResponse = new List<NbpTableResponse>();
        SetupHttpResponse(HttpStatusCode.OK, emptyResponse);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _apiClient.GetTableBAsync()
        );

        Assert.Equal("NBP API returned no data", exception.Message);
    }

    [Fact]
    public async Task GetTableBAsync_ThrowsHttpRequestException_WhenApiReturnsErrorStatusCode()
    {
        SetupHttpResponse(HttpStatusCode.InternalServerError, "Server Error");

        await Assert.ThrowsAsync<HttpRequestException>(
            async () => await _apiClient.GetTableBAsync()
        );
    }

    [Fact]
    public async Task GetTableBAsync_ThrowsHttpRequestException_WhenApiReturnsNotFound()
    {
        SetupHttpResponse(HttpStatusCode.NotFound, "Not Found");

        await Assert.ThrowsAsync<HttpRequestException>(
            async () => await _apiClient.GetTableBAsync()
        );
    }

    [Fact]
    public async Task GetTableBAsync_HandlesMultipleTables_InResponse()
    {
        var response = new List<NbpTableResponse>
        {
            new()
            {
                Table = "B",
                EffectiveDate = "2024-01-15",
                Rates = new List<NbpRate>
                {
                    new() { Currency = "US Dollar", Code = "USD", Mid = 4.00m }
                }
            },
            new()
            {
                Table = "B",
                EffectiveDate = "2024-01-16",
                Rates = new List<NbpRate>
                {
                    new() { Currency = "Euro", Code = "EUR", Mid = 4.50m }
                }
            }
        };

        SetupHttpResponse(HttpStatusCode.OK, response);

        var result = await _apiClient.GetTableBAsync();

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetTableBAsync_PreservesDecimalPrecision()
    {
        var response = new List<NbpTableResponse>
        {
            new()
            {
                Table = "B",
                EffectiveDate = "2024-01-15",
                Rates = new List<NbpRate>
                {
                    new() { Currency = "Test", Code = "TST", Mid = 1.23456789m }
                }
            }
        };

        SetupHttpResponse(HttpStatusCode.OK, response);

        var result = await _apiClient.GetTableBAsync();

        Assert.Equal(1.23456789m, result[0].Rates[0].Mid);
    }

    [Fact]
    public async Task GetTableBAsync_HandlesEmptyRatesList()
    {
        var response = new List<NbpTableResponse>
        {
            new()
            {
                Table = "B",
                EffectiveDate = "2024-01-15",
                Rates = new List<NbpRate>()
            }
        };

        SetupHttpResponse(HttpStatusCode.OK, response);

        var result = await _apiClient.GetTableBAsync();

        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Empty(result[0].Rates);
    }

    [Fact]
    public async Task GetTableBAsync_HandlesSpecialCharacters_InCurrencyNames()
    {
        var response = new List<NbpTableResponse>
        {
            new()
            {
                Table = "B",
                EffectiveDate = "2024-01-15",
                Rates = new List<NbpRate>
                {
                    new() { Currency = "peso argentyńskie", Code = "ARS", Mid = 0.004898m },
                    new() { Currency = "lew bułgarski", Code = "BGN", Mid = 2.2345m }
                }
            }
        };

        SetupHttpResponse(HttpStatusCode.OK, response);

        var result = await _apiClient.GetTableBAsync();

        Assert.Equal("peso argentyńskie", result[0].Rates[0].Currency);
        Assert.Equal("lew bułgarski", result[0].Rates[1].Currency);
    }

    [Fact]
    public async Task GetTableBAsync_UsesCorrectEndpoint()
    {
        var response = new List<NbpTableResponse>
        {
            new()
            {
                Table = "B",
                EffectiveDate = "2024-01-15",
                Rates = new List<NbpRate>()
            }
        };

        SetupHttpResponse(HttpStatusCode.OK, response);

        await _apiClient.GetTableBAsync();

        VerifyHttpCall("api/exchangerates/tables/B/?format=json");
    }

    [Fact]
    public async Task GetTableBAsync_ThrowsException_WhenNetworkError()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new HttpRequestException("Network error"));

        var exception = await Assert.ThrowsAsync<HttpRequestException>(
            async () => await _apiClient.GetTableBAsync()
        );

        Assert.Equal("Network error", exception.Message);
    }

    [Fact]
    public async Task GetTableBAsync_ThrowsException_WhenTimeout()
    {
        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ThrowsAsync(new TaskCanceledException("Request timed out"));

        await Assert.ThrowsAsync<TaskCanceledException>(
            async () => await _apiClient.GetTableBAsync()
        );
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, object? content)
    {
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = statusCode
        };

        if (content != null)
        {
            var json = JsonSerializer.Serialize(content);
            httpResponse.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        }

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);
    }

    private void SetupHttpResponse(HttpStatusCode statusCode, string content)
    {
        var httpResponse = new HttpResponseMessage
        {
            StatusCode = statusCode,
            Content = new StringContent(content)
        };

        _httpMessageHandlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(httpResponse);
    }

    private void VerifyHttpCall(string expectedUri)
    {
        _httpMessageHandlerMock
            .Protected()
            .Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.Method == HttpMethod.Get &&
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains(expectedUri)
                ),
                ItExpr.IsAny<CancellationToken>()
            );
    }

    public void Dispose()
    {
        _httpClient.Dispose();
    }
}