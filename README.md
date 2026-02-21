# Almaviva Slot Monitoring (.NET 10, Blazor Web App Server)

Production-oriented Blazor Web App (Server) for slot availability monitoring with:

- ASP.NET Core Identity (PostgreSQL, roles Admin/User)
- External OAuth login via Google redirect flow
- Fluent UI Blazor dashboard and settings page
- Periodic checks via BackgroundService
- Manual checks from Dashboard
- Telegram notification service via sendMessage API
- EF Core migrations for Identity + app tables

## Run

1. Configure `appsettings.json`
2. Run PostgreSQL
3. Apply migrations automatically on startup
4. Start app

```bash
dotnet restore
dotnet run
```

## Default seed

- Admin user: `admin@local`
- Password: `Admin123!`
