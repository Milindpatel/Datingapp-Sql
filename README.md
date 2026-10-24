# DatingApp – SQL Server API

ASP.NET Core Web API for a dating app, using SQL Server (database-first: the `User` table and `DataContext` were scaffolded from an existing database). Provides registration and JWT login.

![CI](https://github.com/Milindpatel/Datingapp-Sql/actions/workflows/ci.yml/badge.svg)

## Tech stack

.NET 10, ASP.NET Core controllers, EF Core 10 (SQL Server), JWT bearer authentication, xUnit v3.

## Getting started

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download) and SQL Server (LocalDB, Express or Docker) with the `DatingApp` database and `User` table.

```bash
cd DatingApp.API
# Point at your SQL Server
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Database=DatingApp;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet run      # http://localhost:5000
```

In development the JWT signing key comes from `appsettings.Development.json`. In production set `AppSettings__Token` (64+ characters); the API refuses to start without it.

## API

| Method | Route | Description |
| --- | --- | --- |
| POST | `/api/auth/register` | `{ "username", "password" }` (password 8–64 chars) |
| POST | `/api/auth/login` | Returns `{ "token" }` |
| GET | `/api/auth/me` | Requires `Authorization: Bearer <token>` |

## Tests

```bash
dotnet test --solution DatingApp.slnx
```

Tests use EF Core's in-memory provider, so no SQL Server is needed.
