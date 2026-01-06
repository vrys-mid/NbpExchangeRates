using NbpExchangeRates.Infrastructure.Entities;
using NbpExchangeRatesApi.Contracts;

namespace NbpExchangeRatesApi.Services;

public interface IRatesService
{
    Task<List<CurrencyRateDto>> GetRatesByDateAsync(DateTime? date);
    Task<List<DateTime>> GetPublicationDatesAsync();
    Task<List<CurrencyRateHistoryDto>> GetCurrencyHistoryAsync(string code);


}