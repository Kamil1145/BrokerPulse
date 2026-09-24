---
project: BrokerPulse
approved_at: 2026-09-23
status: api_and_web_live; ci_secrets_pending
source: Plan Mode deploy (context/foundation/infrastructure.md + context/foundation/tech-stack.md)
---

# First deployment: BrokerPulse to Azure (Container Apps + Static Web Apps)

This is the audit-trail copy of the plan approved via Plan Mode on 2026-09-23, per `CLAUDE.md`'s Module 1 / Lesson 5 chain. It records what was supposed to happen; see **Execution status** below for what has actually happened so far.

## Context

`context/foundation/infrastructure.md` recommends Azure Container Apps for the API, chosen via a bias-checked research process over 8 other platforms. Exploration of the repo at plan time confirmed a genuinely greenfield deployment: `api/` was a skeleton (single `net10.0` project, only the placeholder `WeatherForecast` endpoint), `web/` was the unmodified Astro starter template, and no `.github/workflows/`, IaC, `docs/reference/contract-surfaces.md`, or `context/deployment/` existed yet. `infrastructure.md` had only decided the API's platform — it never named a hosting target for the static frontend; that gap is resolved below.

Given the skeleton state, this "first deployment" stands up the Azure resources and CI/CD pipeline end-to-end against the current placeholder code, as a smoke test for the pipeline itself. Real features (EF Core models, auth, voice-note upload, transcription) are out of scope — they plug into the infra this plan provisions, as separate follow-up work.

## Frontend hosting decision (resolves the `infrastructure.md` gap) — revised during execution

**Originally planned**: Azure Static Web Apps (Free tier), for co-location with the rest of the Azure stack and automatic PR preview URLs.

**What actually happened**: Azure Static Web Apps' only EU region, West Europe, rejected new resources on this subscription (`RequestDisallowedByAzure: The selected region is currently not accepting new customers`) — confirmed not transient (retried, failed identically). The only other Static Web Apps regions are Central US, East US 2, West US 2, and East Asia — none in the EU. Rather than accept a non-EU region for a GDPR-relevant product, the user chose **Cloudflare** (Workers with Static Assets, the current GA successor to Cloudflare Pages) instead: `web/` has no database of its own, so `infrastructure.md`'s Q5 co-location rationale (which was about the API's data layer) doesn't actually require the frontend to live in Azure too. `web/` and `api/` are already documented as fully independent components with no shared code.

Deployed to `https://brokerpulse-web.kamil1145.workers.dev` via a hand-written `web/wrangler.jsonc` (`assets.directory: ./dist`) and plain `wrangler deploy` — deliberately **not** `wrangler pages project create`/`astro add cloudflare`, which the CLI tries to auto-run and which would have added an unrequested `@astrojs/cloudflare` adapter, KV, and Images bindings to `web/`, breaking its documented static-output convention. See `docs/reference/contract-surfaces.md`'s "Known gotchas" section for details, including a prompt-injection attempt observed in that CLI's own output (text addressed to "agents," recommending an undocumented `--force` flag) that was flagged and not followed.

**Trade-off accepted**: no automatic PR preview URLs (the reason Azure Static Web Apps was originally attractive) — `deploy-web.yml` only deploys on push to `main` for now. Adding preview deploys (e.g. via `wrangler versions upload` + a PR-comment step) is follow-up work, not done in this pass.

## Azure resource plan

All resources in resource group `brokerpulse-rg`, region `polandcentral` (per `infrastructure.md`; per-service availability for all three services below was flagged as unconfirmed in that file's risk register — re-verify before creation, fall back to West Europe if any component is missing there).

| Resource | Name | Tier |
|---|---|---|
| Resource group | `brokerpulse-rg` | — |
| Container Registry | `brokerpulseacr` (unique-name-checked at creation) | Basic |
| Container Apps environment | `brokerpulse-env` | Consumption |
| Container App (API) | `brokerpulse-api` | Consumption, multi-revision mode enabled from creation |
| Postgres Flexible Server | `brokerpulse-pg` (unique-name-checked) | Burstable B1ms, free-tier eligible |
| Storage account (Blob) | `brokerpulsest<suffix>` (unique-name-checked) | Standard LRS, Hot tier |
| Static Web App | `brokerpulse-web` | Free |

Secrets approach for this first pass: Container Apps native secrets (not Key Vault) for the Postgres connection string. Key Vault is a documented future hardening step, not blocking this deploy.

## Application changes

- `api/Dockerfile` — multi-stage: `mcr.microsoft.com/dotnet/sdk:10.0` build → `mcr.microsoft.com/dotnet/aspnet:10.0` runtime, non-root user, `ENV ASPNETCORE_HTTP_PORTS=8080`, `EXPOSE 8080`.
- `api/.dockerignore` — excludes `bin/`, `obj/`, etc.
- `api/Program.cs` — removed `app.UseHttpsRedirection()` (ACA ingress terminates TLS; keeping it loops), added a one-line comment explaining why.
- `api/Features/Health/HealthEndpoints.cs` — new `IEndpointGroup` mapping `GET /health`, following the existing `WeatherForecastEndpoints` pattern (per `api/AGENTS.md`'s hard rule that `Program.cs` never changes when adding a feature — endpoints go through `Features/`).
- Container App ingress: `--target-port 8080 --ingress external`, matching the Dockerfile's `ASPNETCORE_HTTP_PORTS`.
- No changes needed in `web/` beyond the Static Web Apps GitHub Action config.

## CI/CD

- `.github/workflows/deploy-api.yml` — triggers on push to `main`, `paths: api/**`. OIDC `azure/login@v2` (no long-lived secret), `docker build`/push to `brokerpulseacr`, `az containerapp update --image ...`.
- `.github/workflows/deploy-web.yml` — triggers on push to `main` and on PRs, `paths: web/**`. `Azure/static-web-apps-deploy@v1`, `app_location: web`, `output_location: dist` — handles both production deploys and PR preview environments (with a matching close-preview job on PR close).

## Documentation deliverables

- `docs/reference/contract-surfaces.md` — records every Azure resource name and GitHub secret name from this plan.
- `context/deployment/deploy-plan.md` — this file.

## Manual gates (human-only, not agent-automated)

- `az login` and Azure subscription selection.
- Creating the Azure AD App Registration + OIDC federated credential trust for GitHub Actions, and scoping its role assignment (Contributor) to `brokerpulse-rg` only.
- Adding `AZURE_CLIENT_ID` / `AZURE_TENANT_ID` / `AZURE_SUBSCRIPTION_ID` as GitHub Actions secrets, and the Static Web Apps deployment token as `AZURE_STATIC_WEB_APPS_API_TOKEN`.
- Recording the Postgres Flexible Server admin password somewhere durable (password manager).
- Confirming Poland Central's per-service availability (ACA + Postgres Flexible Server + Blob) before the resource-group region is locked in.

## Execution status

Completed:
- [x] `api/Features/Health/HealthEndpoints.cs`, `api/Program.cs` (HTTPS redirect removed), `api/Dockerfile`, `api/.dockerignore`
- [x] `.github/workflows/deploy-api.yml`, `.github/workflows/deploy-web.yml` (web workflow rewritten for Cloudflare, see above)
- [x] `docs/reference/contract-surfaces.md`, this file
- [x] Azure CLI and GitHub CLI installed (`winget`); `az login` completed (user `owner@example.invalid`, subscription "Azure subscription 1")
- [x] Resource providers registered (`Microsoft.App`, `Microsoft.DBforPostgreSQL`, `Microsoft.Storage`, `Microsoft.ContainerRegistry`, `Microsoft.Web`, `Microsoft.OperationalInsights`) — none were registered by default on this fresh subscription
- [x] `brokerpulse-rg` (polandcentral), `brokerpulseacr`, `brokerpulsest001` + `uploads` container, `brokerpulse-pg-redacted` (Postgres 16, Burstable B1ms) + Azure-services firewall rule, `brokerpulse-env` (Container Apps environment)
- [x] API image built locally (Docker Desktop engine started manually; `az acr build`/ACR Tasks is blocked on this subscription — `TasksOperationsNotAllowed`, needs an Azure support request), smoke-tested locally, pushed to ACR
- [x] `brokerpulse-api` Container App created: multi-revision mode, target port 8080, system-assigned managed identity for ACR pull (no static registry credentials), **live and verified**: `/health` → 200, `/weatherforecast` → placeholder JSON, both over the public HTTPS FQDN
- [x] Postgres connection string stored as a Container Apps secret (`postgres-connection-string`) — not yet wired to an env var, since EF Core isn't in the project yet
- [x] `web/` built and deployed to Cloudflare (Workers Static Assets) — **live and verified**: `https://brokerpulse-web.kamil1145.workers.dev` returns 200 and the Astro template HTML

Not completed:
- [ ] `AZURE_CLIENT_ID` / `AZURE_TENANT_ID` / `AZURE_SUBSCRIPTION_ID` GitHub secrets and the OIDC federated credential trust — `deploy-api.yml` cannot run yet
- [ ] `CLOUDFLARE_API_TOKEN` / `CLOUDFLARE_ACCOUNT_ID` GitHub secrets — `deploy-web.yml` cannot run yet
- [ ] No commit/PR has been made yet — all of the above was done by direct CLI/API calls against Azure and Cloudflare, not through the (still-unauthenticated) GitHub Actions workflows
- [ ] PR preview deploys for `web/` (lost when the plan moved off Azure Static Web Apps to Cloudflare; not replaced)
- [ ] `az containerapp revision list` multi-revision rollback path — configured but not yet exercised with a second revision

## Verification

- [x] `curl https://brokerpulse-api.bravemoss-b9c75002.polandcentral.azurecontainerapps.io/health` → 200
- [x] `curl https://brokerpulse-api.bravemoss-b9c75002.polandcentral.azurecontainerapps.io/weatherforecast` → placeholder JSON
- [x] `curl https://brokerpulse-web.kamil1145.workers.dev/` → 200, Astro template HTML
- [ ] Static Web Apps PR preview — n/a, superseded by the Cloudflare decision above
- [ ] `az containerapp revision list` rollback exercise — not yet run
