# NBPExchangeRates

A simple project to display NBP Table B Rates


## Features

- Displaying current currency conversions in PLN on the main table 
- Search functionality by code or currency name
- Showing historical snapshot of table B, which already has been serialized
- Sorting by Currency Code, currency name and Rate
- Showing flags corresponding to the currency
- Possibility to add multiple currencies to favorites, which is persistant (localStorage)
- Possibility to filter by favorite currencies
- Possibility to display chart of currency rate over custom time period
- Possibility to plot 2 currencies on one chart for comparison 
- Trend column showing, if in last time period the currency rate was increasing or decreasing
- Currency converter, which can convert from one currency from table B to other


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
- **Description:** A Web app which enables the user to interact with the NBP Table B data
5. NbpExchangeRates.ImporterCron.Tests & NbpExchangeRates.WebApi.Tests
- **Technologies:** XUnit, Moq
- **Description:** Unit tests 


## First run

Prerequisits 
*Docker / SQL Server on local machine (replace xx with actual password)
*.NET 8 SDK
*NPM

```bash
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=xxx" \
  -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

next, populate connectionstring to point to new  sql server instance in projects NbpExchangeRates.ImporterCron appsettings.json, and NbpExchangeRates.WebApi appsetting.json.  
 Move those secrets to new file appsettings.Development.json  
 
 In NbpExchangeRates.Infrastructure project please run (and replace xxx with actual password of your sql server instance)
```bash
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost,1433;Database=NbpRates;User Id=sa;Password=xxx;TrustServerCertificate=True" --project NbpExchangeRates.Infrastructure
```

After that is done, please go to root folder of the project (where .sln file is located) and run
```bash
dotnet ef database update \
  --project NbpExchangeRates.Infrastructure \
  --startup-project NbpExchangeRates.WebApi
```
Run ImporterCron and WebAPI projects (in different terminals or a background thread), go to the project locations and run
```bash
dotnet run #in NbpExchangeRates.ImporterCron and NbpExchangeRates.WebApi
```
Go to hangfire dashboard (usually http://localhost:5022/hangfire/) and retrigger import-table-b job in Recurring Jobs tab  
Finally please go to NbpExchangeRates.UI
```bash
npm install
npm run dev
```
The app should be accessible under port 5173 (http://localhost:5173/)

## Images

<img width="1889" height="557" alt="mainTable" src="https://github.com/user-attachments/assets/6d95f375-e383-4f0d-9a02-3396bfc6ef1e" />

*Figure 1: Main Table 

<img width="1899" height="854" alt="chart" src="https://github.com/user-attachments/assets/11e49550-9816-4f29-8ad4-7bf44c7d68fa" />

*Figure 2: Comparison chart

<img width="611" height="468" alt="chartDetail" src="https://github.com/user-attachments/assets/b4a197a7-bfd6-42d4-af8c-4ec2e0c7751a" />

*Figure 3: Chart details

<img width="1137" height="454" alt="currencyConverter" src="https://github.com/user-attachments/assets/e85024bc-09b1-4436-a07f-3703aa518a4c" />

*Figure 4: Currency converter

<img width="1894" height="466" alt="favorites" src="https://github.com/user-attachments/assets/d08633f8-6e6d-45d1-a3d4-d44e619a12b8" />

*Figure 5: Favorites filter on main table
