using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Nbp.Rates.Importer.Jobs;
using Nbp.Rates.Importer.Services;
using NbpExchangeRates.Infrastructure.Data;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/importer-.log", rollingInterval: RollingInterval.Day)
    .MinimumLevel.Information() 
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));

builder.Services.AddHttpClient<INbpApiClient, NbpApiClient>(client =>
{
    client.BaseAddress = new Uri("https://api.nbp.pl/");
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHangfire(config =>
{
    config.UseSqlServerStorage(
        builder.Configuration.GetConnectionString("SqlServer"),
        new SqlServerStorageOptions
        {
            PrepareSchemaIfNecessary = true
        });
});

builder.Services.AddHangfireServer();

builder.Services.AddScoped<ImportTableBJob>();

var app = builder.Build();

app.UseHangfireDashboard("/hangfire");

using (var scope = app.Services.CreateScope())
{
    var recurringJobs = scope.ServiceProvider.GetRequiredService<IRecurringJobManager>();

    recurringJobs.AddOrUpdate<ImportTableBJob>(
        "import-table-b",
        job => job.RunAsync(),
        Cron.Weekly(DayOfWeek.Wednesday, 12)
    );
}

try
{
    Log.Information("Starting ImporterCron...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}