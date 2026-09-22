# api/ Agent Guidelines

ASP.NET Core minimal API (C#, `net10.0`), currently a single-file project — no per-feature layout has been decided yet.

## Hard rules

- `Nullable` reference types are enabled (`BrokerPulse.Api.csproj`). Do not suppress warnings — fix them.
- Every endpoint needs `.WithName(...)` so it stays discoverable through the OpenAPI document (`AddOpenApi()` / `MapOpenApi()` in `Program.cs`).
- Never put secrets in `appsettings.json` / `appsettings.Development.json` — use user-secrets or environment variables.

## Commands

See `@CLAUDE.md` in this directory for run/build commands. The SDK version is pinned in `@../global.json`.

## Current state

`Program.cs` holds composition and the one example endpoint (`/weatherforecast`) together — the unmodified minimal-API template, not a real feature yet. No test project exists (`dotnet test` has nothing to run). No endpoint/model/service split has been agreed — decide one before the first real feature lands, rather than accreting more code into `Program.cs`.

## Pitfalls

`BrokerPulse.Api.http` targets `http://localhost:5154` only. The `https` launch profile (`Properties/launchSettings.json`) uses a different port (`7208`) — update the `.http` file if testing over HTTPS.
