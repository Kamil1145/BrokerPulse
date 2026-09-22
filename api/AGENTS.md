# api/ Agent Guidelines

ASP.NET Core minimal API (C#, `net10.0`) using Vertical Slice Architecture — one folder per feature under `Features/`.

## Hard rules

- New endpoints go in `Features/<Feature>/<Feature>Endpoints.cs` as a class implementing `IEndpointGroup` (`Common/Extensions/EndpointExtensions.cs`). `Program.cs` never changes when adding a feature — `app.MapEndpointGroups()` auto-discovers every `IEndpointGroup` in the assembly via reflection.
- `Nullable` reference types are enabled (`BrokerPulse.Api.csproj`). Do not suppress warnings — fix them.
- Every endpoint needs `.WithName(...)` so it stays discoverable through the OpenAPI document (`AddOpenApi()` / `MapOpenApi()` in `Program.cs`).

## Commands

`dotnet run` (or `dotnet watch run` for hot reload) from this directory — dev URLs from `Properties/launchSettings.json`: `http://localhost:5154`, `https://localhost:7208`. `dotnet build` (or `dotnet build BrokerPulse.Api.slnx`). SDK version pinned in `@../global.json`.

## Current state

`Features/WeatherForecast/` is the only slice so far — the unmodified template sample, migrated into the new layout as the reference example, not a real feature. No test project exists. PostgreSQL via EF Core is the planned data store (`@../context/foundation/tech-stack.md`) but isn't added to the project yet. When it lands: entity type configurations (`IEntityTypeConfiguration<T>`) can live next to the feature that owns the entity, but the `DbContext` itself (registration, connection string, migrations) stays centralized — a separate `DbContext` per feature only makes sense with genuinely separate databases/bounded contexts, which would be overkill for this MVP.

## Pitfalls

`BrokerPulse.Api.http` targets `http://localhost:5154` only — the `https` profile uses port `7208`; update the `.http` file if testing over HTTPS.
