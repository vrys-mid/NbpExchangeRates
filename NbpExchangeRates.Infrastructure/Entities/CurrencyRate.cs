using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace NbpExchangeRates.Infrastructure.Entities;

[Index(nameof(Code), nameof(EffectiveDate), IsUnique = true)]
public class CurrencyRate
{
    public int Id { get; set; }
    public string Currency { get; set; } = null!;
    public string Code { get; set; } = null!;
    
    [Column(TypeName = "decimal(18,8)")]
    public decimal Mid { get; set; }
    
    public DateTime EffectiveDate { get; set; }
    public string Table { get; set; } = "B";
}