---
project: BrokerPulse
approved_at: 2026-09-23
status: api_and_web_live; api_ci_verified; web_ci_verified
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
| Postgres Flexible Server | `brokerpulse-pg` (unique-name-checked) — actually created under a different, suffixed name, see `docs/reference/contract-surfaces.md` | Burstable B1ms, free-tier eligible |
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

- `.github/workflows/deploy-api.yml` — triggers on push to `main`, `paths: api/**` (and the workflow file itself). OIDC `azure/login@v2` (no long-lived secret), `docker build`/push to `brokerpulseacr`, `az containerapp update --image ...`. **Verified end-to-end on 2026-09-24** (first successful run, 1m16s; the run itself was later deleted because its log printed the full app JSON incl. the owner's e-mail): new revision `brokerpulse-api--0000002` took 100% traffic, `/health` → 200.
- `.github/workflows/deploy-web.yml` — triggers on push to `main` only, `paths: web/**` (and the workflow file itself). Rewritten for Cloudflare: `npm ci` + `npm run build` + `cloudflare/wrangler-action@v3` (`command: deploy`). **Verified green on 2026-09-24** (run `36048068040`, after the Wrangler 4 pin and the Cloudflare secrets); `https://brokerpulse-web.kamil1145.workers.dev/` → 200. (The original Azure Static Web Apps design with PR previews was dropped.)

### OIDC trust (created 2026-09-24)

- Azure AD App Registration `brokerpulse-github` (appId `f31864fb-59fa-45cb-be72-ca46bd9ce67a`) + service principal.
- Role: **Contributor**, scoped to `brokerpulse-rg` only (not the subscription).
- Federated credential `github-main`, issuer `https://token.actions.githubusercontent.com`, audience `api://AzureADTokenExchange`, subject `repo:Kamil1145@46505054/BrokerPulse@1386369400:ref:refs/heads/main`. **The subject uses GitHub's immutable-ID format** (`owner@<id>/repo@<id>`), because the repo has `use_immutable_subject: true`. The first attempt used the plain `repo:Kamil1145/BrokerPulse:...` form and failed with `AADSTS700213`.
- Only `main` may log in; PR branches and forks cannot.

## Documentation deliverables

- `docs/reference/contract-surfaces.md` — records every Azure resource name and GitHub secret name from this plan.
- `context/deployment/deploy-plan.md` — this file.

## Manual gates (human-only, not agent-automated)

- `az login` and Azure subscription selection.
- Creating the Azure AD App Registration + OIDC federated credential trust for GitHub Actions, and scoping its role assignment (Contributor) to `brokerpulse-rg` only.
- Adding `AZURE_CLIENT_ID` / `AZURE_TENANT_ID` / `AZURE_SUBSCRIPTION_ID` as GitHub Actions secrets (done 2026-09-24). The Static Web Apps token (`AZURE_STATIC_WEB_APPS_API_TOKEN`) is no longer needed; `CLOUDFLARE_API_TOKEN` / `CLOUDFLARE_ACCOUNT_ID` replace it (set 2026-09-24). The Cloudflare token is scoped to one account with only Workers Scripts: Edit and Account Settings: Read, and **expires 2026-12-01** — rotate it before then or `Deploy Web` will start failing on auth.
- Recording the Postgres Flexible Server admin password somewhere durable (password manager).
- Confirming Poland Central's per-service availability (ACA + Postgres Flexible Server + Blob) before the resource-group region is locked in.

## Execution status

Completed:
- [x] `api/Features/Health/HealthEndpoints.cs`, `api/Program.cs` (HTTPS redirect removed), `api/Dockerfile`, `api/.dockerignore`
- [x] `.github/workflows/deploy-api.yml`, `.github/workflows/deploy-web.yml` (web workflow rewritten for Cloudflare, see above)
- [x] `docs/reference/contract-surfaces.md`, this file
- [x] Azure CLI and GitHub CLI installed (`winget`); `az login` completed (project owner's account, subscription "Azure subscription 1")
- [x] Resource providers registered (`Microsoft.App`, `Microsoft.DBforPostgreSQL`, `Microsoft.Storage`, `Microsoft.ContainerRegistry`, `Microsoft.Web`, `Microsoft.OperationalInsights`) — none were registered by default on this fresh subscription
- [x] `brokerpulse-rg` (polandcentral), `brokerpulseacr`, `brokerpulsest001` + `uploads` container, the Postgres Flexible Server (Postgres 16, Burstable B1ms) + Azure-services firewall rule, `brokerpulse-env` (Container Apps environment)
- [x] API image built locally (Docker Desktop engine started manually; `az acr build`/ACR Tasks is blocked on this subscription — `TasksOperationsNotAllowed`, needs an Azure support request), smoke-tested locally, pushed to ACR
- [x] `brokerpulse-api` Container App created: multi-revision mode, target port 8080, system-assigned managed identity for ACR pull (no static registry credentials), **live and verified**: `/health` → 200, `/weatherforecast` → placeholder JSON, both over the public HTTPS FQDN
- [x] Postgres connection string stored as a Container Apps secret (`postgres-connection-string`) — not yet wired to an env var, since EF Core isn't in the project yet
- [x] `web/` built and deployed to Cloudflare (Workers Static Assets) — **live and verified**: `https://brokerpulse-web.kamil1145.workers.dev` returns 200 and the Astro template HTML

Completed 2026-09-24 (CI wiring for the API):
- [x] App Registration `brokerpulse-github`, service principal, Contributor on `brokerpulse-rg`, federated credential `github-main` (see "OIDC trust" above)
- [x] GitHub secrets `AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID` set on `Kamil1145/BrokerPulse`
- [x] First commit pushed to `main`; the `Deploy API` run succeeded on re-run after fixing the federated-credential subject (first attempt: `AADSTS700213`)

Not completed:
- [x] `Deploy Web` green (run `36048068040`) after fixing the Wrangler version and setting `CLOUDFLARE_API_TOKEN` / `CLOUDFLARE_ACCOUNT_ID`. The first token was pasted into chat by mistake and had to be rolled; the secret was first set with an empty value by a non-interactive `gh secret set` (see gotchas in `docs/reference/contract-surfaces.md`)
- [ ] PR preview deploys for `web/` (lost when the plan moved off Azure Static Web Apps to Cloudflare; not replaced)
- [x] Manual rollback exercised 2026-09-24: `az containerapp ingress traffic set -n brokerpulse-api -g brokerpulse-rg --revision-weight brokerpulse-api--0000002=100` moved 100% traffic to the previous revision, `/health` → 200 (after a cold start from `ScaledToZero`); rolled forward to `--0000003` the same way, `/health` → 200

## Verification

- [x] `curl https://brokerpulse-api.bravemoss-b9c75002.polandcentral.azurecontainerapps.io/health` → 200
- [x] `curl https://brokerpulse-api.bravemoss-b9c75002.polandcentral.azurecontainerapps.io/weatherforecast` → placeholder JSON
- [x] `curl https://brokerpulse-web.kamil1145.workers.dev/` → 200, Astro template HTML
- [x] `Deploy API` workflow green; revision `brokerpulse-api--0000002` (image tagged with commit SHA) at 100% traffic; `/health` → 200 (2026-09-24)
- [ ] Static Web Apps PR preview — n/a, superseded by the Cloudflare decision above
- [x] `Deploy Web` workflow green; site → 200 (2026-09-24)
- [x] Rollback exercise — run 2026-09-24 (see above)

## Repository visibility and history (2026-09-24)

- The project first lived in a private repo (now archived privately as `BrokerPulse-old`). Before going public its history was rewritten with `git filter-repo` (course tooling removed, author e-mail replaced with the GitHub noreply address, personal e-mails and the Postgres server name/admin login replaced, session trailers stripped; the final tree hash was unchanged).
- A force-push alone did not remove the old data, because GitHub keeps pre-rewrite commits reachable by SHA and through merged PRs. The public repository was therefore **recreated** from the clean history: new repo ID `1386369400`, so the OIDC federated-credential subject and all Actions secrets were redone (see "OIDC trust" above). Old Actions runs and PR discussions did not carry over; the decisions they contained are recorded in this file.
- The new repo is public, with secret scanning, push protection and branch protection on `main` (required checks `API (build, format, image)` and `Web (build)`, PR required with 0 approvals, no force-push or deletion; admins are not enforced, so the owner can bypass in an emergency).

## Gaps

Ordered by how soon they bite.

**Closed on 2026-09-24**
1. ~~**`Deploy Web` fails with `Missing entry-point`.**~~ Cause: `cloudflare/wrangler-action@v3` installs an old Wrangler 3.x by default, which does not understand an assets-only `web/wrangler.jsonc` (no `main`). Fixed with `wranglerVersion: "4"` on the action; verified in CI.
2. ~~**Cloudflare secrets missing.**~~ `CLOUDFLARE_API_TOKEN` (one account, Workers Scripts: Edit + Account Settings: Read, expires 2026-12-01) and `CLOUDFLARE_ACCOUNT_ID` are set; verified in CI. Follow-up: token rotation before 2026-12-01.

**Pipeline has no safety net**
3. ~~**No post-deploy verification in `deploy-api.yml`.**~~ Fixed: a final `Verify /health` step resolves the app FQDN with `az containerapp show` and runs `curl --fail` with retries; verified green on 2026-09-24 (and again on the post-history-rewrite run). Caveat: it checks the app's public FQDN, which in multi-revision mode serves the revision holding traffic, so it confirms the new revision only while it gets 100%.
4. **Rollback is manual only.** The manual path is now tested (see Execution status): one `az containerapp ingress traffic set` command switches traffic in seconds, and revisions are kept. What is still missing is automation: nothing shifts traffic back when a new revision fails. Also note `Verify /health` runs after `az containerapp update` has already moved 100% traffic to the new revision, so a bad deploy is live until someone rolls back by hand. Safer variant: deploy with 0% traffic, verify the new revision's own FQDN, then shift traffic. A stale `--75nsd88` quickstart revision (`ActivationFailed`, 0% traffic) is left over from the initial creation and can be deactivated.
5. **CI on pull requests — workflow added, pending first run on GitHub.** `.github/workflows/ci.yml` runs on every PR to `main` (no path filter, so its jobs can become required checks in gap 6): `api` = `dotnet build -warnaserror` (enforces the "don't suppress warnings" rule in `api/AGENTS.md`) + `dotnet format --verify-no-changes` + a `docker build` of `api/` without pushing (catches Dockerfile breakage before deploy); `web` = `npm ci` + `npm run build`. All of these were run locally against the current code and pass. **Remaining holes:** there are no tests in either component and no `astro check`/lint in `web/`, so CI only proves "it builds and is formatted", not "it works"; add `dotnet test` and a web test/type-check step as soon as the first real feature lands. `web/` PR preview deploys (dropped with Static Web Apps) are still not replaced. Branch protection that makes these checks mandatory is gap 6.
6. ~~**Deploys are not gated.**~~ **Closed on 2026-09-24 (two layers).** (a) `ci.yml` gate: deploys run only after both CI jobs pass and only for the component whose files changed (`changes` job; unknown `before` deploys both). (b) The repo was made public, which unlocked **branch protection on `main`**: required checks `API (build, format, image)` and `Web (build)`, PR required (0 approvals, single-owner repo), no force-push, no branch deletion; `enforce_admins` is off so the owner can bypass in an emergency. Still open: no human approval before production (a GitHub Environment with a required reviewer would add one), and `workflow_dispatch` on the deploy workflows remains a deliberate manual override that skips CI.

**Least-privilege posture**
7. **Contributor on the resource group is broader than the pipeline needs.** The pipeline only pushes an image and updates one Container App, but the identity could also delete Postgres or the storage account in `brokerpulse-rg`. That conflicts with the "destructive actions are human-only" stance in `CLAUDE.md`. Options: a custom role limited to `Microsoft.App/containerApps/*` + ACR push, or resource-level assignments (AcrPush on the registry, Container Apps Contributor on the app).
8. **Actions are pinned to major tags (`@v4`, `@v2`, `@v3`), not SHAs**, and GitHub warns that Node 20 actions are being forced to Node 24 (`actions/checkout@v4`, `azure/login@v2`). Low risk for MVP, worth bumping.

**Data and secrets**
9. **Postgres secret is not wired to the app.** `postgres-connection-string` exists as a Container Apps secret but no env var references it; EF Core is not in the project yet. **Firewall (changed 2026-09-24):** the `0.0.0.0` "Azure services" rule was **removed** before the repo went public, because it admitted any Azure tenant's traffic; only the owner's client-IP rule remains, so the API cannot reach Postgres yet. Whitelisting the app's outbound IPs is not viable: the Consumption environment has ~160 shared outbound addresses that can change. **Before wiring EF Core, choose a real access model**: a VNet-integrated Container Apps environment with Postgres private access, or Entra ID authentication with the app's managed identity plus a private path. Public access on the server is still `Enabled`.
10. **No backup/restore or migration story.** Nothing yet runs EF migrations on deploy; Postgres Burstable backup retention and point-in-time restore have not been checked or tested.
11. **Secrets live as Container Apps native secrets, not Key Vault.** Accepted for the first pass in this plan; Key Vault is still the hardening step.

**Observability and reproducibility**
12. **No monitoring or alerting.** Only the auto-created Log Analytics workspace exists; no alerts on failed revisions, 5xx or restarts, and no uptime check on `/health`.
13. **Infrastructure was created by hand-run CLI, with no IaC.** The only record of how Azure resources were made is this file and `contract-surfaces.md`. Rebuilding the environment would be manual.
14. **Web has no custom domain, and the API has no CORS/auth story yet.** `web/` on `*.workers.dev` calling `*.azurecontainerapps.io` will need CORS configured in the API when the first real endpoint is called from the browser.
15. **Docker image build happens on the GitHub runner with no layer cache, no image scanning, and no cleanup of old tags in ACR** (Basic tier storage will fill slowly with SHA-tagged images).
