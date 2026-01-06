using Microsoft.AspNetCore.Mvc;
using NbpExchangeRatesApi.Services;

namespace NbpExchangeRatesApi.Controllers;

[ApiController]
[Route("api/rates")]
public class RatesController : ControllerBase
{
    private readonly IRatesService _ratesService;
    private readonly ILogger<RatesController> _logger;

    public RatesController(IRatesService ratesService, ILogger<RatesController> logger)
    {
        _ratesService = ratesService;
        _logger = logger;
    }

    
    [HttpGet("{code}/history")]
    public async Task<IActionResult> GetHistory(string code)
    {
        _logger.LogInformation($"Getting history for code {code}");
        try
        {
            var data = await _ratesService.GetCurrencyHistoryAsync(code);
            _logger.LogInformation($"Returning history for code {code}");
            return Ok(data);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error getting history for code {code}");        
            return StatusCode(500);
        }

    }

    [HttpGet("publication-dates")]
    public async Task<IActionResult> GetPublicationDates()
    {
        _logger.LogInformation($"Getting publication dates");
        try
        {
            var dates = await _ratesService.GetPublicationDatesAsync();
            _logger.LogInformation($"Returning publication dates");
            return Ok(dates);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error getting publication dates");
            return StatusCode(500);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetRates(DateTime? date)
    {
        _logger.LogInformation($"Getting rates");
        try
        {
            var rates = await _ratesService.GetRatesByDateAsync(date);
            _logger.LogInformation($"Returning rates");
            return Ok(rates);
        }
        catch (Exception e)
        {
            _logger.LogError(e, $"Error getting rates");
            return StatusCode(500);
        }
    }
}