# FinanceDAMT Backend

API de finanzas personales construida con .NET 10 y Clean Architecture.

## Stack

- .NET 10 / ASP.NET Core Web API
- Entity Framework Core 10
- PostgreSQL (por defecto)
- Redis
- Serilog, Hangfire, JWT

## Requisitos

- .NET SDK 10
- Docker Desktop (para BD/Redis con compose)
- (Opcional) dotnet-ef

Instalar CLI de EF Core:

```powershell
 dotnet tool install --global dotnet-ef
```

## Inicio rapido (Docker)

Desde la raiz del repo:

```powershell
 docker compose up --build
```

Servicios:

- API: http://localhost:5000
- PostgreSQL: localhost:5433 (host) -> 5432 (contenedor)
- Redis: localhost:6379

## Ejecutar API sin Docker

```powershell
 cd backend/src/FinanceDAMT.API
 dotnet run
```

## Migraciones EF Core

```powershell
 dotnet ef migrations add InitialCreate --project backend/src/FinanceDAMT.Infrastructure --startup-project backend/src/FinanceDAMT.API --output-dir Persistence/Migrations
 dotnet ef database update --project backend/src/FinanceDAMT.Infrastructure --startup-project backend/src/FinanceDAMT.API
 dotnet ef migrations list --project backend/src/FinanceDAMT.Infrastructure
```

## Variables de entorno clave

- DatabaseProvider
- ConnectionStrings__DefaultConnection
- JwtSettings__Secret
- Groq__ApiKey
- SmtpSettings__Host
- BlobStorage__ConnectionString

Sugerencia: usa un archivo .env para cllentralizar secretos cuando uses Docker.
