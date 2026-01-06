using Microsoft.EntityFrameworkCore;
using Nbp.Rates.Importer.Services;
using NbpExchangeRates.Infrastructure.Data;
using NbpExchangeRates.Infrastructure.Entities;

namespace Nbp.Rates.Importer.Jobs;

public class ImportTableBJob
{
    private readonly AppDbContext _db;
    private readonly INbpApiClient _apiClient;
    private readonly ILogger<ImportTableBJob> _logger;

    
    public ImportTableBJob(AppDbContext db, INbpApiClient apiClient, ILogger<ImportTableBJob> logger)
    {
        _db = db;
        _apiClient = apiClient;
        _logger = logger;
    }

    public async Task RunAsync()
    {
        _logger.LogInformation("Starting import table job");
        try
        {
            var tables = await _apiClient.GetTableBAsync();
            var table = tables.Single();

            var effectiveDate = DateTime.Parse(table.EffectiveDate);

            var existingRates = await _db.CurrencyRates
                .Where(x => x.Table == "B" && x.EffectiveDate == effectiveDate)
                .ToListAsync();

            if (existingRates.Any())
            {
                _db.CurrencyRates.RemoveRange(existingRates);
            }

            var entities = table.Rates.Select(r => new CurrencyRate
            {
                Table = "B",
                Currency = r.Currency,
                Code = r.Code,
                Mid = r.Mid,
                EffectiveDate = effectiveDate
            });

            _db.CurrencyRates.AddRange(entities);
            await _db.SaveChangesAsync();
            _logger.LogInformation("Finished import table job");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error while importing table job");
        }
    }
}