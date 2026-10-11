# Bitcoin Prices

Built with .NET 10 and React.

## Run with Docker

```powershell
docker compose up --build
```

- Frontend: `http://localhost:3000`
- API: `http://localhost:8080`

Stop the application:

```powershell
docker compose down
```

Delete the persisted SQLite data as well:

```powershell
docker compose down --volumes
```

## Run locally

Requirements: .NET SDK 10, Node.js 24 or later, and pnpm.

Start the API from the repository root:

```powershell
dotnet restore
dotnet tool restore
$env:ASPNETCORE_URLS = "http://localhost:8080"
dotnet run --project src/BitcoinPrices.Api
```

Start the frontend in a second terminal:

```powershell
cd src/BitcoinPrices.Web
pnpm install
pnpm dev
```

The frontend is normally available at `http://localhost:5173`.

## Test

Backend:

```powershell
dotnet test
```

Frontend:

```powershell
cd src/BitcoinPrices.Web
pnpm lint
pnpm test:run
pnpm build
```

## Assumptions

- BTC/USD and arithmetic mean are the only current UI choices. They remain selectors to make future options explicit.
- At least two providers must return usable prices for aggregation to succeed.
- Provider close prices and aggregates use floating-point values to match the stated requirement.
- SQLite is appropriate for this single-instance prototype. Multiple API instances would require a shared database and distributed single-flight coordination.
- A requested timestamp represents the end of a one-hour candle. For example, `2023-01-01T01:00:00Z` refers to `[00:00, 01:00)`.
