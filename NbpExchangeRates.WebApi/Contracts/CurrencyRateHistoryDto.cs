namespace NbpExchangeRatesApi.Contracts;

public record CurrencyRateHistoryDto (
    DateTime Date,
    decimal Rate);