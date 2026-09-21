---
project: BrokerPulse
assessed_at: 2026-09-21T20:03:32Z
assessed_commit: none
scope: "api/, web/ (repo-level criteria assessed within the same scope)"
agent_readiness: ready-with-compensation
components:
  - name: api
    path: api/
    language: C# (net9.0, nullable enabled)
    framework: ASP.NET Core minimal API (Microsoft.AspNetCore.OpenApi 9.0.4)
    test_runner: null
    package_manager: NuGet
  - name: web
    path: web/
    language: TypeScript (astro/tsconfigs/strict)
    framework: Astro ^7.3.3 (Node >=22.12.0)
    test_runner: null
    package_manager: npm
structure:
  ok: 3
  partial: 5
  missing: 2
  not_applicable: 0
gates_passed: 14
gates_failed: 0
not_assessed:
  - "runtime behaviour: build, tests and dev servers were not run; the assessment is static"
  - "git history: repository has no commits, so history-based signals were unavailable"
  - "context/, .claude/, .agents/ and .github/skills/ (toolkit and project-context files): excluded from code metrics"
  - "deployment: no deployment configuration exists, so nothing could be assessed beyond its absence"
---

## Inventory

| Component | Path | Language | Framework | Test runner | Package manager | Source files |
|---|---|---|---|---|---|---|
| api | `api/` | C# (`net9.0`, `Nullable` enabled) | ASP.NET Core minimal API, `Microsoft.AspNetCore.OpenApi` 9.0.4 | none | NuGet | 6 (1 `.cs`, 1 `.csproj`, 1 `.http`, 3 JSON) |
| web | `web/` | TypeScript (extends `astro/tsconfigs/strict`) | Astro ^7.3.3, `engines.node` >=22.12.0 | none | npm (`package-lock.json`) | 15 (3 `.astro` under `src/`) |

Top-level layout (files, excluding `context/`, `.claude/`, `.agents/`): `web/` 15, `api/` 6, `.github/` 4 (only `skills/`, no `workflows/`), plus `CLAUDE.md`, `.gitignore`, `skills-lock.json`, `.10x-cli.json`. CI/CD: not detected. Deployment: not detected. Agent instruction files: `CLAUDE.md` at the root only. The repository has no commits.

Both components are still unmodified starter templates: `api/Program.cs` is the weather-forecast sample, `web/src/components/Welcome.astro` is the Astro demo.

## Structure Assessment

| Criterion | Status | Evidence |
|---|---|---|
| Repository layout and component boundaries | ok | `api/BrokerPulse.Api.csproj` and `web/package.json` are the only manifests, one per component; the root holds only repo config (`CLAUDE.md`, `.gitignore`, `skills-lock.json`, `.10x-cli.json`) and no source code. |
| Conventions inside projects | partial | `web/`: `src/pages/`, `src/layouts/`, `src/components/`, `public/` follow the Astro layout. `api/`: only `Program.cs`, `Properties/`, `appsettings*.json`; the endpoint and the `WeatherForecast` record live inline in `Program.cs`, so there is no defined place yet for endpoints, models or services. |
| Tests | missing | 0 test files in the repo; no `*.Tests.csproj`, no `vitest`/`jest`/`playwright` config, no test scripts in `web/package.json` (`dev`, `build`, `preview`, `astro` only). |
| Build and environment reproducibility | partial | `web/`: `package-lock.json` committed, `engines.node >=22.12.0`, scripts documented in `web/README.md`. `api/`: `<TargetFramework>net9.0` set, but no `global.json` (SDK version not pinned), and no documented build/run command for `api/` anywhere; no root README. NuGet lock file absent (normal for .NET, noted only). |
| Code quality tooling | partial | Strict typing is configured in both: `web/tsconfig.json` extends `astro/tsconfigs/strict`; `api/BrokerPulse.Api.csproj` has `<Nullable>enable</Nullable>`. No formatter or linter: no `.editorconfig`, no ESLint/Prettier/Biome, no `TreatWarningsAsErrors`. `web/README.md` lists `astro check`, but `web/package.json` has neither `@astrojs/check` nor `typescript`, so type checking is only available in the editor. |
| CI/CD and deployment | missing | `.github/` contains only `skills/`; no `workflows/`, no `.gitlab-ci.yml`, `Jenkinsfile` or `.circleci/`; no `Dockerfile`, `docker-compose.yml` or other deployment config. |
| Configuration and secrets hygiene | ok | Root `.gitignore` covers `bin/`, `obj/`, `node_modules/`, `web/dist/`, `web/.astro/`, `.env`, `.env.*`, `api/appsettings.*.local.json`, `.claude/settings.local.json`; `web/.gitignore` present. `api/appsettings.json` holds only logging and `AllowedHosts`. Secret scan (file names only): 0 hits. No `.env.example` yet, none needed while no environment variables exist. |
| Agent context and documentation | partial | One `CLAUDE.md` at the root (98 lines): lines 1-71 are a tool-managed block (`BEGIN/END @przeprogramowani/10x-cli`) describing the course toolkit, not this project; lines 73-98 are the `web/ (Astro)` section (dev server command, doc links). No project-specific overview, no build/test/run commands for `api/`, no structure or conventions. No `AGENTS.md` (intentionally a single instruction file). No root README; `web/README.md` is the unmodified Astro template README. |
| Cross-component contract | partial | `api/Program.cs` enables OpenAPI (`AddOpenApi()`, `MapOpenApi()` in Development). `web/` does not consume it: no client generator (`openapi-typescript`, `orval`, `NSwag`) in `web/package.json` or the `.csproj`, and no API-calling code. `api/Program.cs` has no CORS configuration (`AddCors`/`UseCors`: 0 matches), which matters if the two run on separate origins. |
| Size and complexity hotspots | ok | Largest source files: `web/src/components/Welcome.astro` 210 lines (template demo), `api/Program.cs` 41, `web/src/layouts/Layout.astro` 23. None above 500 lines. |

Legend: ok = satisfied, partial = partly satisfied, missing = absent, n/a = not applicable

## Quality Gate Assessment

**api (C# / ASP.NET Core 9)**

| Component | Typed | Convention | Training Data | Documented | Verdict |
|---|---|---|---|---|---|
| Language (C#) | ✓ | — | — | — | pass |
| Framework (ASP.NET Core minimal API) | — | ✓ | ✓ | ✓ | pass |
| Build tool (.NET SDK / MSBuild) | — | ✓ | ✓ | ✓ | pass |
| Test runner | — | — | — | — | none present (see structure) |

**web (TypeScript / Astro 7)**

| Component | Typed | Convention | Training Data | Documented | Verdict |
|---|---|---|---|---|---|
| Language (TypeScript) | ✓ | — | — | — | pass |
| Framework (Astro) | — | ✓ | ✓ | ✓ | pass |
| Build tool (Astro CLI on Vite) | — | ✓ | ✓ | ✓ | pass |
| Test runner | — | — | — | — | none present (see structure) |

Legend: ✓ = pass, ✗ = fail, ~ = partial, — = not applicable. 14 cells assessed, 14 passed, 0 failed. Absent test runners are not scored as gate failures; they are counted under structure.

### Gate Details

- **C# typed:** `api/BrokerPulse.Api.csproj` line 5: `<Nullable>enable</Nullable>`; C# is statically typed.
- **TypeScript typed:** `web/tsconfig.json`: `"extends": "astro/tsconfigs/strict"`.
- **ASP.NET Core convention:** `Microsoft.NET.Sdk.Web` project with `Program.cs`, `Properties/launchSettings.json`, `appsettings.*.json` in the standard places. Minimal APIs leave the internal layering to the project; that is captured under structure, not as a gate failure.
- **Astro convention:** file-based routing under `web/src/pages/`, layouts and components in the standard folders.
- **Training data (per language family):** ASP.NET Core is a mainstream choice within C#/.NET; Astro is a mainstream meta-framework within the JS/TS family. Caveat for `web/`: the manifest pins the major version `^7`; if agent knowledge of that major is thin, version-pinned docs in the instruction file compensate (see below).
- **Documented:** ASP.NET Core docs at `learn.microsoft.com/aspnet/core` are versioned and the project pins `net9.0`; Astro docs at `docs.astro.build` and links already present in `CLAUDE.md`.

## Gaps & Compensation

- **No tests (structure, missing).** An agent cannot verify its own changes to either component, so regressions surface only in manual runs. Compensation: create a test project and a test runner, and put the commands in the instruction file (blocks below).
- **No CI (structure, missing).** No automated feedback loop on pull requests; nothing checks that both components still build. Compensation: a workflow with path filters per component.
- **`api/` has no internal layout yet (partial).** The endpoint and its record sit in `Program.cs`; new features will accrete there. Compensation: state where endpoints and models live before the first feature.
- **Reproducibility (partial).** The .NET SDK version is not pinned and `api/` has no documented commands. Compensation: `global.json` and commands in `CLAUDE.md`.
- **Quality tooling (partial).** Typing is strict, but formatting and linting are unconfigured, and `astro check` is not runnable. Compensation: `.editorconfig`, a linter/formatter, `@astrojs/check` + `typescript`.
- **Agent context (partial).** `CLAUDE.md` is mostly toolkit text; nothing tells an agent what BrokerPulse is, how to run or test `api/`, or where code goes. Compensation: the blocks below.
- **Contract (partial).** OpenAPI is produced but not consumed, and CORS is not configured. Compensation: generated TypeScript types and an explicit decision on origins.

### Recommended Instruction File Additions

All blocks go into the root `CLAUDE.md`, **below** the `<!-- END @przeprogramowani/10x-cli -->` marker and outside the tool-managed block (the repo uses a single `CLAUDE.md` by decision). Blocks marked *(new convention)* describe a structure that does not exist yet; adjust before pasting.

```markdown
## Repository layout

- `api/` — ASP.NET Core 10 minimal API (C#, `net10.0`). Project: `api/BrokerPulse.Api.csproj`.
- `web/` — Astro 7 frontend (TypeScript strict), npm.
- `context/` — project artifacts (PRD, tech stack, logs); never edit files under `context/archive/`.
- Both components are independent: no shared code between `api/` and `web/`.

## api/ (ASP.NET Core)

- Run from the repo root: `dotnet run --project api` (dev URLs from `api/Properties/launchSettings.json`: `http://localhost:5154`, `https://localhost:7208`).
- Build: `dotnet build api`. Nullable reference types are enabled (`<Nullable>enable</Nullable>`); do not suppress nullable warnings, fix them.
- OpenAPI is enabled (`AddOpenApi()`, `MapOpenApi()` in Development). Every endpoint must be reachable through it, so give endpoints explicit names (`.WithName(...)`) and typed results.
- Configuration lives in `appsettings.json` / `appsettings.Development.json`. Never put secrets there; use user-secrets or environment variables.
- *(new convention)* Keep `Program.cs` for composition only. Endpoints go in `api/Endpoints/<Feature>Endpoints.cs` as `MapGroup` extension methods, DTOs and records in `api/Models/`, services in `api/Services/`, registered via extension methods called from `Program.cs`.
- *(new convention)* Every new endpoint needs a test in the test project (see Testing).

## web/ conventions

- Astro version is pinned by `web/package.json` (`^7.3.3`); check `docs.astro.build` for the matching major before using an API you are unsure about. Node >= 22.12.0.
- Commands (run in `web/`): `npm run dev`, `npm run build`, `npm run preview`.
- Routes are files in `web/src/pages/`, shared layouts in `web/src/layouts/`, components in `web/src/components/`. TypeScript is strict (`astro/tsconfigs/strict`): no `any` without a comment explaining why.
- *(new convention)* All calls to the API go through one typed client module (`web/src/lib/api/`), never ad hoc `fetch` in components.

## Testing

- *(new convention)* `api/`: xUnit project `api.Tests/`, run with `dotnet test` from the repo root. `web/`: Vitest, run with `npm test` in `web/`. Run both before considering a change done.
```

### Structural Improvements

| Priority | Change | Where | Why |
|---|---|---|---|
| P1 | Add an xUnit test project and a first test | `api.Tests/` (or `api/tests/`) | No way to verify agent changes to the backend, where the business logic (profile extraction, ranking) will live. |
| P1 | Add a test runner (Vitest) and `npm test` script | `web/package.json` | Same for the frontend. |
| P1 | Add CI: build both components and run tests, with path filters (`api/**`, `web/**`) | `.github/workflows/ci.yml` | Currently `.github/` has no workflows; nothing checks that either component builds. |
| P2 | Make the first commit | repo root | No commits exist: no baseline for diffs, reviews or CI. |
| P2 | Add project-specific instruction content (blocks above) | root `CLAUDE.md`, below `END` marker | The managed block does not describe the project. |
| P2 | Add a root `README.md` with prerequisites and run commands for both components | repo root | The only README is the unmodified Astro template's. |
| P2 | Pin the .NET SDK | `global.json` | `net9.0` is set, but SDK 8 and 9 are both installed here; an unpinned SDK builds differently across machines. |
| P2 | Add `.editorconfig`; add `<TreatWarningsAsErrors>` (or at least analyzers) for `api/`; add ESLint + Prettier (or Biome) for `web/` | `.editorconfig`, `api/BrokerPulse.Api.csproj`, `web/` | No formatter or linter in either component. |
| P2 | Add `@astrojs/check` and `typescript` so `astro check` works | `web/package.json` | The README advertises `astro check`, which currently cannot run. |
| P2 | Decide how `web/` reaches `api/` (same origin via proxy, or CORS policy) and generate TypeScript types from the OpenAPI document | `api/Program.cs`, `web/` | OpenAPI is produced but unused; no CORS is configured for separate origins. |
| P2 | Give `api/` a layout for endpoints, models and services before the first feature | `api/` | Everything is inline in `Program.cs`. |
| P3 | Remove template demo code when the first real feature lands | `api/Program.cs` (weather forecast), `web/src/components/Welcome.astro` | Demo code misleads agents about the project's domain. |

P1 = blocks verification of agent work (or a secret in the repo); P2 = quality gap; P3 = tidiness. No secrets were found.

## Not Assessed

- Runtime behaviour: build, tests and dev servers were not run; this assessment is static.
- Git history: the repository has no commits.
- `context/`, `.claude/`, `.agents/`, `.github/skills/`: excluded from code metrics; read only to locate instruction files.
- Deployment: no configuration exists (target AWS/containers is not represented in files).

## Summary

**Readiness: ready-with-compensation.** Both technology stacks pass all four agent-friendly criteria (14 of 14 assessed): C# and TypeScript are typed, ASP.NET Core and Astro are convention-based, mainstream within their language families and well documented. The weakness is not the stack but the project around it: 3 of 10 structure criteria are satisfied, 5 partly, 2 not at all.

Strengths: clean component boundaries, strict typing configured on both sides, complete `.gitignore` and no secrets, no oversized files.

Key gaps: no tests, no CI, an `api/` with no internal layout yet, and instruction files that describe the course toolkit rather than the project.

First steps: (1) add project-specific blocks to `CLAUDE.md` (below the `END` marker), (2) add a test project for `api/` and a runner for `web/`, (3) add a CI workflow with path filters. A first commit should come before all three.
