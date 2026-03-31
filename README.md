# GAS Forecasting System — Backend

ASP.NET Core 8 Web API powering the Guided-Adaptive Smoothing (GAS) forecasting system. Implements SES, HWES, and the hybrid GAS algorithm with grid-search parameter optimization, error metrics, and PostgreSQL persistence via Neon.

## Research

**Title:** Guided-Adaptive Smoothing: An Error-Driven Hybrid SES–HWES Framework for Multi-Path Range Forecasting in Uncertain Time Series

**Authors:** Jherson Aguto · Mark John Panganiban

**Adviser:** Dr. Ricardo Q. Camungao

**Institution:** CCSICT — Isabela State University, Echague Campus

---

## Tech Stack

| Layer | Technology |
|---|---|
| Runtime | .NET 8 |
| Framework | ASP.NET Core Web API |
| ORM | Entity Framework Core 9 |
| Database | PostgreSQL (Neon serverless) |
| Algorithms | C# — SES, HWES, GAS |
| Math | MathNet.Numerics |
| Docs | Swagger / OpenAPI |
| Hosting | Render (free tier) |

---

## Algorithms

### SES — Simple Exponential Smoothing
Single smoothing parameter α, optimized via grid search. Best for stationary data with no trend or seasonality.

### HWES — Holt-Winters Exponential Smoothing (Additive)
Three parameters α, β, γ optimized via grid search. Handles trend and seasonality.

### GAS — Guided-Adaptive Smoothing
Hybrid framework that:
1. Runs SES and HWES in parallel on 75% training data
2. Computes inverse-error weights from MSE performance
3. Produces three prediction paths: Weighted SES, Weighted HWES, GAS Combined (average)
4. Evaluates all paths with MSE, MAE, RMSE, MAPE

---

## API Endpoints

| Method | Route | Description |
|---|---|---|
| `POST` | `/api/upload/csvColumnNames` | Parse CSV, return column names |
| `POST` | `/api/upload/csvActualValues` | Upload CSV column data |
| `GET` | `/api/Read/ActualValuesDescriptions` | List all uploaded datasets |
| `GET` | `/api/Read/ActualValuesByColumnName` | Get data by column name |
| `POST` | `/api/algorithm/forecast` | Run forecast (train/test validation) |
| `POST` | `/api/algorithm/prediction` | Run future prediction |
| `GET` | `/api/ping` | Health / keepalive check |
| `DELETE` | `/api/Delete/AllActualData` | Delete all uploaded data |
| `DELETE` | `/api/Delete/ActualDataByID` | Delete data by ID |

Full interactive docs available at `/swagger` when running.

---

## Local Development

### Prerequisites
- .NET 8 SDK
- PostgreSQL or a [Neon](https://neon.tech) account

### Setup

```bash
# Clone
git clone https://github.com/bullet162/Gas.git
cd Gas

# Set connection string (never commit real credentials)
# appsettings.Development.json is gitignored — create it:
cat > appsettings.Development.json << 'EOF'
{
  "ConnectionStrings": {
    "DefaultConnection": "your_neon_or_local_postgres_connection_string"
  }
}
EOF

# Apply migrations
dotnet ef database update

# Run
dotnet run
```

API will be available at `http://localhost:5297`. Swagger UI at `http://localhost:5297/swagger`.

### Environment Variables (Production)

Set on Render dashboard — never commit to source:

| Variable | Description |
|---|---|
| `DATABASE_URL` | Full Neon PostgreSQL connection string |
| `PORT` | Set automatically by Render |

---

## Deployment

Render auto-deploys from the `main` branch using the `Dockerfile`. Set the `DATABASE_URL` environment variable in the Render dashboard — never commit it.

### Render Environment Variables

Go to your Render service → Environment → Add the following:

| Variable | Value |
|---|---|
| `DATABASE_URL` | `postgresql://neondb_owner:<password>@ep-round-sun-a1pq3far-pooler.ap-southeast-1.aws.neon.tech/neondb?sslmode=require&channel_binding=require` |

Render sets `PORT` automatically — the Dockerfile reads it at runtime.

### Branches

| Branch | Purpose |
|---|---|
| `main` | Single branch — Render auto-deploys from here |

---

## GitHub Actions

| Workflow | Trigger | Description |
|---|---|---|
| `deploy.yml` | Push to `main` | Builds Svelte frontend and deploys to Firebase Hosting |
| `keepalive.yml` | Cron (Mon–Fri 6am–5pm PHT) | Pings `/api/ping` every 10 min to prevent Render cold starts |

---

## Project Structure

```
├── Algorithm/          # SES, HWES, GAS implementations + grid search
├── Controllers/        # API controllers
├── Data/
│   ├── AppDbContext.cs
│   ├── Entities/       # EF Core models
│   └── Repositories/   # Data access layer
├── Dto/                # Request/response models
├── Migrations/         # EF Core PostgreSQL migrations
├── Utils/              # CSV parser, stopwatch
├── frontend/           # Svelte + Vite SPA
│   ├── src/
│   │   ├── lib/        # Components, api.ts, store.ts
│   │   ├── App.svelte
│   │   └── main.ts
│   └── vite.config.ts
├── .github/workflows/
│   ├── deploy.yml      # Firebase frontend deploy
│   ├── keepalive.yml   # Render ping Mon–Fri 6am–5pm PHT
│   └── sync-stable.yml # main → stable-backend mirror
├── firebase.json
├── .firebaserc
└── Program.cs
```

---

## License

Academic research implementation. All rights reserved © 2025 Jherson Aguto, Mark John Panganiban.
