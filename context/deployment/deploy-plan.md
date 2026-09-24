---
project: BrokerPulse
approved_at: 2026-09-23
status: api_and_web_live; api_ci_verified; web_ci_failing
updated_at: 2026-09-24
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
| Postgres Flexible Server | `brokerpulse-pg` (unique-name-checked) — actually created as `brokerpulse-pg-redacted`, see `docs/reference/contract-surfaces.md` | Burstable B1ms, free-tier eligible |
| Storage account (Blob) | `brokerpulsest<suffix>` (unique-name-checked) — actually `brokerpulsest001` | Standard LRS, Hot tier |
| ~~Static Web App~~ | ~~`brokerpulse-web`~~ — superseded, `web/` is on Cloudflare Workers Static Assets (see above) | — |

Secrets approach for this first pass: Container Apps native secrets (not Key Vault) for the Postgres connection string. Key Vault is a documented future hardening step, not blocking this deploy.

## Application changes

- `api/Dockerfile` — multi-stage: `mcr.microsoft.com/dotnet/sdk:10.0` build → `mcr.microsoft.com/dotnet/aspnet:10.0` runtime, non-root user, `ENV ASPNETCORE_HTTP_PORTS=8080`, `EXPOSE 8080`.
- `api/.dockerignore` — excludes `bin/`, `obj/`, etc.
- `api/Program.cs` — removed `app.UseHttpsRedirection()` (ACA ingress terminates TLS; keeping it loops), added a one-line comment explaining why.
- `api/Features/Health/HealthEndpoints.cs` — new `IEndpointGroup` mapping `GET /health`, following the existing `WeatherForecastEndpoints` pattern (per `api/AGENTS.md`'s hard rule that `Program.cs` never changes when adding a feature — endpoints go through `Features/`).
- Container App ingress: `--target-port 8080 --ingress external`, matching the Dockerfile's `ASPNETCORE_HTTP_PORTS`.
- No changes needed in `web/` beyond the Static Web Apps GitHub Action config.

## CI/CD

- `.github/workflows/deploy-api.yml` — triggers on push to `main`, `paths: api/**` (and the workflow file itself). OIDC `azure/login@v2` (no long-lived secret), `docker build`/push to `brokerpulseacr`, `az containerapp update --image ...`. **Verified end-to-end on 2026-09-24** (run `36046743163`, 1m16s): new revision `brokerpulse-api--0000002` took 100% traffic, `/health` → 200.
- `.github/workflows/deploy-web.yml` — triggers on push to `main` only, `paths: web/**` (and the workflow file itself). Rewritten for Cloudflare: `npm ci` + `npm run build` + `cloudflare/wrangler-action@v3` (`command: deploy`). **Currently failing**, see Gaps below. (The original Azure Static Web Apps design with PR previews was dropped.)

### OIDC trust (created 2026-09-24)

- Azure AD App Registration `brokerpulse-github` (appId `f31864fb-59fa-45cb-be72-ca46bd9ce67a`) + service principal.
- Role: **Contributor**, scoped to `brokerpulse-rg` only (not the subscription).
- Federated credential `github-main`, issuer `https://token.actions.githubusercontent.com`, audience `api://AzureADTokenExchange`, subject `repo:Kamil1145@46505054/BrokerPulse@1380481705:ref:refs/heads/main`. **The subject uses GitHub's immutable-ID format** (`owner@<id>/repo@<id>`), because the repo has `use_immutable_subject: true`. The first attempt used the plain `repo:Kamil1145/BrokerPulse:...` form and failed with `AADSTS700213`.
- Only `main` may log in; PR branches and forks cannot.

## Documentation deliverables

- `docs/reference/contract-surfaces.md` — records every Azure resource name and GitHub secret name from this plan.
- `context/deployment/deploy-plan.md` — this file.

## Manual gates (human-only, not agent-automated)

- `az login` and Azure subscription selection.
- Creating the Azure AD App Registration + OIDC federated credential trust for GitHub Actions, and scoping its role assignment (Contributor) to `brokerpulse-rg` only.
- Adding `AZURE_CLIENT_ID` / `AZURE_TENANT_ID` / `AZURE_SUBSCRIPTION_ID` as GitHub Actions secrets (done 2026-09-24). The Static Web Apps token (`AZURE_STATIC_WEB_APPS_API_TOKEN`) is no longer needed; `CLOUDFLARE_API_TOKEN` / `CLOUDFLARE_ACCOUNT_ID` replace it and are still pending.
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

Completed 2026-09-24 (CI wiring for the API):
- [x] App Registration `brokerpulse-github`, service principal, Contributor on `brokerpulse-rg`, federated credential `github-main` (see "OIDC trust" above)
- [x] GitHub secrets `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` set on `Kamil1145/BrokerPulse`
- [x] Commit `ad53d01` pushed to `main`; `Deploy API` run `36046743163` succeeded on re-run after fixing the federated-credential subject (first attempt: `AADSTS700213`)

Not completed:
- [ ] `Deploy Web` (run `36046743144`) **fails** — see Gaps
- [ ] `CLOUDFLARE_API_TOKEN` / `CLOUDFLARE_ACCOUNT_ID` GitHub secrets not set (not yet reached: the run failed earlier, on the wrangler version)
- [ ] PR preview deploys for `web/` (lost when the plan moved off Azure Static Web Apps to Cloudflare; not replaced)
- [ ] Rollback path not exercised. Revisions `--0000001` (`:initial`) and `--0000002` both exist, so it can be tested with `az containerapp ingress traffic set`

## Verification

- [x] `curl https://brokerpulse-api.bravemoss-b9c75002.polandcentral.azurecontainerapps.io/health` → 200
- [x] `curl https://brokerpulse-api.bravemoss-b9c75002.polandcentral.azurecontainerapps.io/weatherforecast` → placeholder JSON
- [x] `curl https://brokerpulse-web.kamil1145.workers.dev/` → 200, Astro template HTML
- [x] `Deploy API` workflow green; revision `brokerpulse-api--0000002` (image tagged with commit SHA) at 100% traffic; `/health` → 200 (2026-09-24)
- [ ] Static Web Apps PR preview — n/a, superseded by the Cloudflare decision above
- [ ] `Deploy Web` workflow green — failing
- [ ] Rollback exercise — not yet run

## Gaps

Ordered by how soon they bite.

**Broken now**
1. **`Deploy Web` fails with `Missing entry-point`.** `cloudflare/wrangler-action@v3` installs an old Wrangler 3.x by default, which does not understand an assets-only `web/wrangler.jsonc` (no `main`). `web/package.json` does not pin Wrangler. Fix applied: `wranglerVersion: "4"` on the action. **Pending verification in CI** (needs gap 2 first, since the run will then fail on auth until the Cloudflare secrets exist).
2. **Cloudflare secrets are missing.** After gap 1, the next failure will be auth. Needs `CLOUDFLARE_API_TOKEN` (scoped to Workers edit on one account, set by the human, not pasted into chat) and `CLOUDFLARE_ACCOUNT_ID`.

**Pipeline has no safety net**
3. **No post-deploy verification in `deploy-api.yml`.** The job ends after `az containerapp update`, so a broken image that starts but fails `/health` still shows a green run. Fix applied: a final `Verify /health` step resolves the app FQDN with `az containerapp show` and runs `curl --fail` with retries. **Pending verification in CI.** Note it checks the app's public FQDN, which in multi-revision mode serves the revision holding traffic, so it confirms the new revision only while it gets 100%.
4. **No automatic rollback.** Multi-revision mode is on, but nothing shifts traffic back on failure, and the rollback command has never been run. Documented in `infrastructure.md` as the operating model; still untested.
5. **No CI on pull requests.** Both workflows only run on push to `main`. There is no build/test/lint gate before merge, and `web/` PR previews were dropped with Static Web Apps.
6. **Deploys are not gated.** Anything merged to `main` that touches `api/**` goes straight to production. There is no GitHub Environment with required reviewers, and no branch protection configured (verify in repo settings).

**Least-privilege posture**
7. **Contributor on the resource group is broader than the pipeline needs.** The pipeline only pushes an image and updates one Container App, but the identity could also delete Postgres or the storage account in `brokerpulse-rg`. That conflicts with the "destructive actions are human-only" stance in `CLAUDE.md`. Options: a custom role limited to `Microsoft.App/containerApps/*` + ACR push, or resource-level assignments (AcrPush on the registry, Container Apps Contributor on the app).
8. **Actions are pinned to major tags (`@v4`, `@v2`, `@v3`), not SHAs**, and GitHub warns that Node 20 actions are being forced to Node 24 (`actions/checkout@v4`, `azure/login@v2`). Low risk for MVP, worth bumping.

**Data and secrets**
9. **Postgres secret is not wired to the app.** `postgres-connection-string` exists as a Container Apps secret but no env var references it; EF Core is not in the project yet. Postgres is reachable via the "Azure services" firewall rule (`0.0.0.0`), which allows any Azure tenant's traffic, not just this app — acceptable for the skeleton, not for real data.
10. **No backup/restore or migration story.** Nothing yet runs EF migrations on deploy; Postgres Burstable backup retention and point-in-time restore have not been checked or tested.
11. **Secrets live as Container Apps native secrets, not Key Vault.** Accepted for the first pass in this plan; Key Vault is still the hardening step.

**Observability and reproducibility**
12. **No monitoring or alerting.** Only the auto-created Log Analytics workspace exists; no alerts on failed revisions, 5xx or restarts, and no uptime check on `/health`.
13. **Infrastructure was created by hand-run CLI, with no IaC.** The only record of how Azure resources were made is this file and `contract-surfaces.md`. Rebuilding the environment would be manual.
14. **Web has no custom domain, and the API has no CORS/auth story yet.** `web/` on `*.workers.dev` calling `*.azurecontainerapps.io` will need CORS configured in the API when the first real endpoint is called from the browser.
15. **Docker image build happens on the GitHub runner with no layer cache, no image scanning, and no cleanup of old tags in ACR** (Basic tier storage will fill slowly with SHA-tagged images).
