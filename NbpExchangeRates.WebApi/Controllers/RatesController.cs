using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NbpExchangeRates.Infrastructure.Data;
using NbpExchangeRatesApi.Contracts;

namespace NbpExchangeRatesApi.Controllers;

[ApiController]
[Route("api/rates")]
public class RatesController : ControllerBase
{
    private readonly AppDbContext _db;

    public RatesController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/rates/latest
    [HttpGet("latest")]
    public async Task<ActionResult<IEnumerable<CurrencyRateDto>>> GetLatest()
    {
        var latestDate = await _db.CurrencyRates
            .MaxAsync(x => x.EffectiveDate);

        var rates = await _db.CurrencyRates
            .Where(x => x.EffectiveDate == latestDate)
            .OrderBy(x => x.Code)
            .Select(x => new CurrencyRateDto(
                x.Code,
                x.Currency,
                x.Mid,
                x.EffectiveDate))
            .ToListAsync();

        return Ok(rates);
    }
    
    [HttpGet("{code}/history")]
    public async Task<IActionResult> GetHistory(string code)
    {
        var data = await _db.CurrencyRates
            .Where(r => r.Code == code)
            .OrderBy(r => r.EffectiveDate)
            .Select(r => new CurrencyRateHistoryDto(
                r.EffectiveDate,
                r.Mid
            ))
            .ToListAsync();

        return Ok(data);
    }
}