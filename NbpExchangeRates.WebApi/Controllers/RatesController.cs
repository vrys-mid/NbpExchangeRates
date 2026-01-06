using Microsoft.AspNetCore.Mvc;
using NbpExchangeRatesApi.Services;

namespace NbpExchangeRatesApi.Controllers;

[ApiController]
[Route("api/rates")]
public class RatesController : ControllerBase
{
    private readonly IRatesService _ratesService;

    public RatesController(IRatesService ratesService)
    {
        _ratesService = ratesService;
    }

    
    [HttpGet("{code}/history")]
    public async Task<IActionResult> GetHistory(string code)
    {
        var data = await _ratesService.GetCurrencyHistoryAsync(code);
        return Ok(data);
    }

    [HttpGet("publication-dates")]
    public async Task<IActionResult> GetPublicationDates()
    {
        var dates = await _ratesService.GetPublicationDatesAsync();
        return Ok(dates);
    }

    [HttpGet]
    public async Task<IActionResult> GetRates(DateTime? date)
    {
        var rates = await _ratesService.GetRatesByDateAsync(date);
        
        return Ok(rates);
    }
}