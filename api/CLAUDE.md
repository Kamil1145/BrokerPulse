## Development

Run from this directory: `dotnet run` (or `dotnet watch run` for hot reload). Dev URLs from `Properties/launchSettings.json`: `http://localhost:5154`, `https://localhost:7208`.

Build: `dotnet build` (or `dotnet build BrokerPulse.Api.slnx` from this directory). SDK version is pinned in the repo-root `global.json`.

## Conventions

- `Nullable` reference types are enabled (`BrokerPulse.Api.csproj`) — do not suppress nullable warnings, fix them.
- OpenAPI is enabled (`AddOpenApi()`, `MapOpenApi()` in Development). Give new endpoints explicit names (`.WithName(...)`) so they stay discoverable through the OpenAPI document.
- Configuration lives in `appsettings.json` / `appsettings.Development.json`. Never put secrets there — use user-secrets or environment variables.
- `Program.cs` currently holds the unmodified minimal-API template (`/weatherforecast` sample). No endpoint/model/service layout has been decided yet — agree on one before the first real feature lands here.
