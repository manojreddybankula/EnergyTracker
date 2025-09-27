# Energy Consumption Tracking & Analytics API

## Project Overview

This project provides a RESTful API for tracking and analyzing energy consumption data. It supports batch uploads, aggregation reports, and anomaly detection for electricity and gas usage. The solution is built using .NET 9, follows Clean Architecture principles, and is ready for local development and cloud deployment.

### Main Features
- Upload energy readings in batches
- Aggregated consumption and cost reports (grouped by day/week/month)
- Anomaly detection based on rolling averages
- Swagger/OpenAPI documentation
- Ready for Azure deployment
- Docker support for containerized runs

## Setup Instructions

### Run Locally
1. **Requirements:**
   - .NET 9 SDK
   - SQLite (no manual setup needed; DB file auto-created)
2. **Database Setup:**
   - Run: `dotnet ef database update` (creates `energy.db`)
3. **Start API:**
   - Run: `dotnet run --project EnergyTracker`
   - API available at: `http://localhost:80` (or configured port)
4. **Swagger UI:**
   - Visit `http://localhost:80/swagger` for API docs and testing
5. **Run Unit Tests:**
   - Run: `dotnet test EnergyTracker.UnitTests`

### Docker Run
1. **Build Docker Image:**
   - `docker build -t energytracker-api ./EnergyTracker`
2. **Run Container:**
   - `docker run -d -p 8080:80 --name energytracker energytracker-api`
   - API available at: `http://localhost:8080`
3. **Persist Data:**
   - To persist the SQLite DB, mount a volume:
     - `docker run -d -p 8080:80 -v $(pwd)/data:/app/data --name energytracker energytracker-api`

### Azure Deployment
- Update connection string for Azure SQL in `appsettings.json`
- Use provided `azure-pipelines.yml` for CI/CD

## API Examples

### Upload Readings
```
curl -X POST http://localhost:80/readings -H "Content-Type: application/json" -d '{"userId":"12345","readings":[{"product":"electricity","kWh":12.5,"timestamp":"2025-01-02T14:30:00Z"}]}'
```

### Aggregated Report
```
curl "http://localhost:80/report?userId=12345&groupBy=month&product=all"
```

### Anomaly Detection
```
curl "http://localhost:80/anomalies?userId=12345&period=month"
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

## Repository Structure
- `EnergyTracker/` - Main API project
- `EnergyTracker.UnitTests/` - MSTest unit tests
- `energy.db` - SQLite database file (local)

---
For more details, see Swagger UI or review the source code.
