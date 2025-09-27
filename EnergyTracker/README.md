# Energy Consumption Tracking & Analytics API

## Setup

### Local
- .NET 9, SQLite
- Run `dotnet ef database update` to create the database
- Run `dotnet run` to start the API

### Azure
- Update connection string for Azure SQL in `appsettings.json`
- Use provided `azure-pipelines.yml` for CI/CD

## API Examples

### Upload Readings
```
curl -X POST http://localhost:5000/readings -H "Content-Type: application/json" -d '{"userId":"12345","readings":[{"product":"electricity","kWh":12.5,"timestamp":"2025-01-02T14:30:00Z"}]}'
```

### Aggregated Report
```
curl "http://localhost:5000/report?userId=12345&groupBy=month&product=all"
```

### Anomaly Detection
```
curl "http://localhost:5000/anomalies?userId=12345&period=month"
```

## Architecture

- Clean Architecture: Domain, Application, Infrastructure, API
- EF Core, Repository pattern, DI
- Swagger for API docs
- Azure-ready

## Assumptions & Limitations

- Only electricity/gas supported
- Pricing is per kWh, stored in DB
- SQLite for local, Azure SQL for cloud
- Batch size limit: 10,000 readings
- No duplicate readings per (userId, product, timestamp)
