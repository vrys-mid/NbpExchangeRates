# NBPExchangeRates

A simple project to display NBP Table B Rates

--

## Features

- Displaying current currency conversions in PLN on the main table 
- Search functionality by code or currency name
- Showing historical snapshot of table B, which already has been serialized
- Sorting by Currency Code, currency name and Rate
- Showing flags corresponding to the currency
- Possibility to add multiple currenies to favorites, which is persistant (localStorage)
- Possibility to filter by favorite currencies
- Possibility to display chart of currency rate over custom time period
- Possibility to plot 2 currencies on one chart for comparison 
- Trend column showing, if in last time period the currency rate was increasing or decreasing
- Currency converter, which can convert from one currency from table B to other

--

## Project structure

1. NbpExchangeRates.ImporterCron
- **Technologies:** .NET8, EF, Hangfire, Serilog
- **Description:** Cron which is set to fetch data from the NBP Public API every wednesday  and serialize it to SQL Server db. The hangfire dashboard enables to monitor the jobs 
operation as well as manually triggering it. If the job fails there are multiple retries scheduled.
2. NbpExchangeRates.Infrastructure
- **Technologies:** .NET8, EF, Secrets 
- **Description:** Data layer of the project, containing entities and migration data 
3. NbpExchangeRates.WebApi
- **Technologies:** .NET8, EF, Serilog, Swagger
- **Description:** REST API which provides data for the web app
4. NbpExchangeRates.UI
- **Technologies:** React, Typescript, Tailwind, Vite
- **Description:** REST API which provides data for the web app
5. NbpExchangeRates.ImporterCron.Tests & NbpExchangeRates.WebApi.Tests
- **Technologies:** XUnit, Moq
- **Description:** Unit tests 

--

## First run

Prerequisits 
*Docker / SQL Server on local machine (replace xx with actual password)
*.NET 8 SDK
*NPM

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=xxx" \
  -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

next, populate connectionstring to point to new  sql server instance in projects NbpExchangeRates.ImporterCron appsettings.json, and NbpExchangeRates.WebApi appsetting.json. Move those secrets to new file appsettings.Development.json
in NbpExchangeRates.Infrastructure project please run (and replace xxx with actual password of your sql server instance)
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=NbpRates;User Id=sa;Password=xxx;TrustServerCertificate=True" --project NbpExchangeRates.Infrastructure
```

After that is done, please go to root folder of the project (where .sln file is located) and run
```bash
dotnet ef database update \
  --project Nbp.Rates.Infrastructure \
  --startup-project Nbp.Rates.Api
```
Run ImporterCron and WebAPI projects (in different terminals or a background thread), go to the project locations and run
```bash
dotnet run #in NbpExchangeRates.ImporterCron and NbpExchangeRates.WebApi
```
Finally please go to NbpExchangeRates.UI
```bash
npm install
npm run dev
```
The app should be accessible under port 5173 (http://localhost:5173/)
