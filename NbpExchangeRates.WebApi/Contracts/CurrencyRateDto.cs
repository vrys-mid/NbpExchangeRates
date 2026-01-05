namespace NbpExchangeRatesApi.Contracts;

public record CurrencyRateDto(string Code, string Currency, decimal Mid, DateTime EffectiveDate);