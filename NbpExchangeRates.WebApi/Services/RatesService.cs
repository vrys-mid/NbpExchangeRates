using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using NbpExchangeRates.Infrastructure.Data;
using NbpExchangeRates.Infrastructure.Entities;
using NbpExchangeRatesApi.Contracts;
using NbpExchangeRatesApi.Services;

public class RatesService : IRatesService
{
    private readonly AppDbContext _db;
    private readonly IMemoryCache _cache;

    public RatesService(AppDbContext db, IMemoryCache cache)
    {
        _db = db;
        _cache = cache;
    }
    
    public async Task<List<CurrencyRateDto>> GetRatesByDateAsync(DateTime? date)
    {
        var cacheKey = date is null
            ? "rates:latest"
            : $"rates:{date:yyyy-MM-dd}";

        if (_cache.TryGetValue(cacheKey, out List<CurrencyRateDto>? cached))
        {
            return cached!;
        }

        IQueryable<CurrencyRate> query = _db.CurrencyRates.AsNoTracking();

        if (date is not null)
        {
            var dateTime = date.Value;

            query = query.Where(r =>
                r.EffectiveDate.Date == dateTime.Date
            );
        }
        else
        {
            var latestDate = await query.MaxAsync(r => r.EffectiveDate);
            query = query.Where(r => r.EffectiveDate == latestDate);
        }

        var rates = await query
            .OrderBy(r => r.Code) 
            .Select(r => new CurrencyRateDto(
                r.Code,
                r.Currency,
                r.Mid,
                r.EffectiveDate
            ))
            .ToListAsync();

        _cache.Set(
            cacheKey,
            rates,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            }
        );

        return rates;
    }

    
    public async Task<List<DateTime>> GetPublicationDatesAsync()
    {
        const string cacheKey = "publicationDates";

        return await _cache.GetOrCreateAsync(
            cacheKey,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

                return await _db.CurrencyRates
                    .AsNoTracking()
                    .Select(r => r.EffectiveDate) 
                    .Distinct()
                    .OrderByDescending(d => d)
                    .ToListAsync();
            }
        ) ?? new List<DateTime>();
    }
    
    public async Task<List<CurrencyRateHistoryDto>> GetCurrencyHistoryAsync(string code)
    {
        var cacheKey = $"history:{code}";

        return await _cache.GetOrCreateAsync(
            cacheKey,
            async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);

                var rates = await _db.CurrencyRates
                    .AsNoTracking()
                    .Where(r => r.Code == code)
                    .OrderBy(r => r.EffectiveDate) 
                    .ToListAsync(); 

                var dtos = rates
                    .Select(r => new CurrencyRateHistoryDto(
                        r.EffectiveDate,
                        r.Mid
                    ))
                    .ToList();

                return dtos;
            }) ?? new List<CurrencyRateHistoryDto>();
    }


}