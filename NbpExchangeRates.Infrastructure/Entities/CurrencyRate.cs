using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.Json;

namespace NbpExchangeRates.Infrastructure.Entities;

[Index(nameof(Code), nameof(EffectiveDate), IsUnique = true)]
public class CurrencyRate
{
    public int Id { get; set; }
    public string Currency { get; set; } = null!;
    public string Code { get; set; } = null!;
    public decimal Mid { get; set; }
    
    public DateTime EffectiveDate { get; set; }
    public string Table { get; set; } = "B";
}