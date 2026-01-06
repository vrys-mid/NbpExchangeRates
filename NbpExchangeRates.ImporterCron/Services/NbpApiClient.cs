using System.Net.Http.Json;
using System.Text.Json;
using NbpImporter.Nbp;

namespace Nbp.Rates.Importer.Services;

public class NbpApiClient : INbpApiClient
{
    private readonly HttpClient _httpClient;

    public NbpApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }
    
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
    };

    public async Task<List<NbpTableResponse>> GetTableBAsync()
    {
        var response = await _httpClient.GetFromJsonAsync<List<NbpTableResponse>>(
            "api/exchangerates/tables/B/?format=json",
            JsonOptions);

        if (response == null || response.Count == 0)
        {
            throw new InvalidOperationException("NBP API returned no data");
        }

        return response;
    }
}