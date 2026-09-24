---
project: BrokerPulse
researched_at: 2026-09-22
recommended_platform: Azure Container Apps (Poland Central)
runner_up: Google Cloud Run (europe-central2, Warsaw)
context_type: mvp
tech_stack:
  language: C# (.NET 10) + TypeScript
  framework: ASP.NET Core minimal API (Vertical Slice Architecture) + Astro 7 (React islands, static output)
  runtime: .NET 10 Linux container on Azure Container Apps; Node >= 22.12 at build time only
---

## Recommendation

**Deploy on Azure Container Apps in `polandcentral`, running the .NET 10 API as a Consumption-plan container app, PostgreSQL via Azure Database for PostgreSQL Flexible Server, and voice recordings/photos in Azure Blob Storage. Trigger the transcription step as an ACA Job, not an always-on worker.**

This is a clean win, not a close call: Azure Container Apps is the only platform researched that scores full marks on all five agent-friendly criteria (CLI-first, managed/serverless, agent-readable docs, stable deploy API, MCP integration) *and* matches both soft-weighted interview signals that matter most this round — hyperscaler familiarity (Q3: AWS/GCP/Azure) and a preference for co-located managed services (Q5). The Consumption free grant plus a 12-month free Postgres tier keep this MVP at effectively $0 compute cost for the first year. The decisive factor versus the prior research pass is Q1: this time the developer confirmed **no persistent server-side connections or always-on workers are required** — background work (transcription) can be a short-lived triggered job — which reopens fully managed, scale-to-zero platforms that a prior interview answer had ruled out.

## Decision History

This is the second `/10x-infra-research` pass for BrokerPulse, run the same day as the first (2026-09-22). The two passes reached different conclusions because the developer's Q1 answer changed:

1. **First pass** (superseded by this file): Q1 = "Yes, persistent connections/always-on background workers required." That hard-filtered out every fully managed serverless platform and drove the decision toward **AWS Lightsail**, a self-managed single instance — a knowing trade of agent-friendliness for cost-neutrality and existing AWS familiarity, chosen specifically because the always-on requirement ruled out Cloud Run/Container Apps-style scale-to-zero platforms.
2. **This pass**: Q1 = "No — request/response is fine; background work can be a short-lived triggered job." This reopened the full candidate pool, including scale-to-zero container platforms and the three hyperscalers' managed-container products (AWS ECS Express Mode, Google Cloud Run, Azure Container Apps), which were added to the default six-platform pool given the strong hyperscaler-familiarity signal (Q3) and co-location preference (Q5). Azure Container Apps won outright on both the scoring matrix and the soft weights.
3. Mid-review, the developer surfaced a new fact not previously on record: an existing **$100 AWS credit**. This was evaluated explicitly — it covers only ~1–4 months of AWS ECS Express Mode's realistic cost (~$25–100/mo) or ~6–10 months of AWS Lightsail's self-managed cost (~$10–15/mo) — against Azure Container Apps, which needs no credit at all and is genuinely free for the first ~12 months. The developer chose to **ignore the AWS credit** and keep Azure Container Apps as the recommendation; the credit remains unused and is noted here in case future cost pressure makes AWS worth revisiting.

If a future review changes the persistent-connection answer back to "Yes" (e.g., a real-time notifications feature is added), re-run this research — it will likely reopen the AWS Lightsail / Fly.io Machines branch of this decision tree.

## Platform Comparison

Hard filters applied first: **.NET 10 must actually run** (tech stack). Netlify has no .NET runtime path of any kind — not native, not experimental/beta, not even via containers — and is eliminated before scoring. No platform was filtered on persistent connections this round (Q1 = No).

| Platform | CLI-first | Managed/Serverless | Agent-readable docs | Stable deploy API | MCP / Integration | Score (Pass=2, Partial=1, Fail=0) |
|---|---|---|---|---|---|---|
| Netlify | — | — | — | — | — | filtered (no .NET runtime, any status) |
| Cloudflare (Workers + Containers) | Pass | Pass | Pass | Pass | Pass | 10 |
| Vercel | Pass | Partial | Pass | Pass | Pass | 9 |
| Fly.io | Pass | Partial | Pass | Pass | Partial | 8 |
| Railway | Pass | Pass | Pass | Pass | Partial | 9 |
| Render | Partial | Pass | Pass | Pass | Pass | 9 |
| AWS (ECS Express Mode) | Pass | Partial | Pass | Pass | Pass | 9 |
| Google Cloud Run | Pass | Pass | Partial | Pass | Pass | 9 |
| **Azure Container Apps** | **Pass** | **Pass** | **Pass** | **Pass** | **Pass** | **10** |

Soft weights applied: cost (neutral — Q2 "no strong preference," but GCP's mandatory ~$50/mo Cloud SQL floor with no free tier stood out against Azure's genuinely free first year); hyperscaler familiarity (heavy tie-break — Q3, breaks the Cloudflare/Azure 10-10 tie in Azure's favor, since Cloudflare isn't one of the named familiar platforms); edge/global reach not rewarded (Q4, single region is sufficient); co-location (heavy — Q5, penalizes Cloudflare and Vercel in practice despite their raw scores, since neither has a genuinely co-located managed Postgres).

**Cloudflare (Workers + Containers)** — .NET cannot run on Workers itself (V8-isolate JS/Wasm runtime; .NET-on-Wasm via WASI is experimental/community-only). The real path is Cloudflare Containers, GA since 2026-04-13 on the Workers Paid plan ($5/mo base), running the .NET 10 image as a normal Linux process behind a Durable Object — strong on all five criteria (wrangler CLI, llms.txt docs, official MCP server + Agents SDK). The load-bearing gap is co-location: D1 is SQLite (not Postgres-compatible) and Hyperdrive is only a connection pooler for an externally hosted Postgres, so satisfying Q5 means adding a third vendor for the database alone — a real cost against the interview's stated preference despite the tied top score.

**Vercel** — no native .NET runtime; ASP.NET Core only runs via a documented Docker container-function path (GA, not beta). The practical dealbreaker for this app: background/triggered work "stops running between requests" once the container scales to zero, so even a short transcription step must be request-bound or routed through the newer Workflows primitive; the 4.5MB request-body cap also blocks proxying voice/photo uploads through the API directly. Native managed Postgres/KV were deprecated in December 2024, replaced by marketplace-brokered third parties (Neon, Upstash) — not true co-location.

**Fly.io** — genuine long-lived Linux VMs ("Machines"), auto-generated Dockerfile via `fly launch`, scriptable deploy/rollback, llms.txt docs. Managed Postgres (MPG) still lists several features as "under development" (automated patching, alerting, migration tooling) and starts at $38/mo; legacy unmanaged Postgres is explicitly unsupported. The official MCP server is marked experimental. No Warsaw region — Frankfurt/Amsterdam are the EU options.

**Railway** — fully managed compute, GA Postgres with point-in-time recovery, GA S3-compatible Buckets (since 2025-11-28), CLI-scriptable rollback within the retention window, EU West (Amsterdam) region with no caveats. Railpack's auto-detection is unreliable for a multi-project (Vertical Slice Architecture) .NET solution, so a hand-written Dockerfile is the realistic build path. The Claude connector can manage config/monitoring but "cannot trigger deployments" out of the box.

**Render** — Background Worker and Cron Job service types are GA and a clean fit for a triggered transcription step; managed Postgres is GA; Frankfurt region is GA; an official MCP server plus a dedicated `render-oss/skills` GitHub repo make it one of the strongest agent-integration stories researched. Rollback is dashboard/REST-API only, not CLI-scriptable — a real gap against the CLI-first criterion. Native Object Storage remains alpha (external S3 needed for now), and pricing was volatile through August 2026 (three repricing events in one month).

**AWS (ECS Express Mode)** — App Runner closed to new customers (2026-04-30) and Copilot CLI is archived (2026-06), so ECS Express Mode is now AWS's actual agent-friendly path: one command provisions Fargate, ALB, TLS, and autoscaling, deployable via CLI or the AWS MCP Server (GA since 2026-05-06, Frankfurt region added). Aurora Serverless v2 can scale to zero (GA) for genuine after-hours savings, but a NAT Gateway (~$32-35/mo) is typically required for outbound calls to an external transcription API, and realistic continuous cost lands at $70-100/mo — the highest of the three hyperscalers, and the reason the developer's $100 credit only stretches 1-4 months here.

**Google Cloud Run (runner-up)** — Cloud Run Jobs (GA since 2023) is an excellent fit for a short triggered transcription step, compute is effectively free at this scale under the request-based free tier, and the Cloud Run MCP server reached GA in April 2026. Two real gaps versus Azure: Cloud SQL for Postgres has no free tier and runs a flat ~$50/mo regardless of usage — the single largest line item of any platform researched — and .NET 10 support in Google's buildpacks is ambiguous in current public sources (fully mitigated by shipping a self-built container image instead of relying on the buildpack, which is what this file's Getting Started assumes for Azure too).

**Azure Container Apps (Recommended)** — GA across every relevant component: ACA itself, ACA Jobs (GA since 2023, a clean fit for the triggered transcription step), `az containerapp up`/`logs show`/revision-based rollback, Azure Database for PostgreSQL Flexible Server, Blob Storage, and Azure MCP Server 2.0 (GA April 2026, 276 tools). The Consumption free grant (180k vCPU-s, 360k GiB-s, 2M requests/month) and a 12-month free Postgres B1ms tier keep this MVP at effectively $0 compute cost for the first year. Two genuine costs to plan for: Log Analytics log ingestion bills separately from ACA compute and is easy to undercount, and Blob Storage is not S3-compatible, so any S3-first tooling or tutorial needs manual translation.

### Shortlisted Platforms

#### 1. Azure Container Apps (Recommended)

Full marks on all five agent-friendly criteria, matches the developer's hyperscaler familiarity (Q3) and co-location preference (Q5) with a genuinely native Postgres + Blob Storage story, and is effectively free for the first year. ACA Jobs maps directly onto the "short triggered job, no always-on worker" answer from Q1.

#### 2. Google Cloud Run (runner-up)

Matches the same familiarity and co-location signals, with near-zero compute cost and GA Jobs/MCP. Loses to Azure specifically on Cloud SQL's mandatory ~$50/mo floor (no free tier) and an unresolved .NET 10 buildpack status (workaround: ship a custom container image). Revisit this option if Azure's Log Analytics costs or Poland Central service-parity turn out to be a problem in practice.

#### 3. Railway

Cheapest all-in managed bundle (~$14-20/mo) with zero IAM/VPC/resource-group surface to learn — the lowest-effort fallback if Azure's Microsoft-specific tooling (ARM, Log Analytics, Key Vault) becomes a bigger drag on a solo after-hours schedule than its cost advantage is worth.

## Anti-Bias Cross-Check: Azure Container Apps

### Devil's Advocate — Weaknesses

1. **Log Analytics ingestion is a hidden, unbounded-by-default cost line.** ACA ships console and system logs to Log Analytics by default, billed per-GB separately from compute — not reflected in ACA's own cost estimate, and easy to underestimate if ASP.NET Core logging verbosity creeps up during debugging.
2. **Poland Central's per-service availability wasn't independently confirmed as a complete matrix.** ACA, Postgres Flexible Server, and Blob Storage each have their own regional rollout history; the region being GA overall doesn't guarantee every dependent service is available there on day one.
3. **The Postgres Flexible Server B1ms free tier is a 12-month promotional offer, not a permanent SKU.** A real recurring bill (~$12+/mo minimum, region-dependent) lands at month 13 — easy to under-budget for if the "free" framing in Getting Started isn't revisited.
4. **Revision-based rollback only works if multi-revision mode is deliberately enabled ahead of time.** ACA's default single-revision mode has no traffic-weight safety net; a developer who deploys in the default mode discovers this gap only when they need to roll back and the command doesn't apply.
5. **Azure Blob Storage is not S3-compatible.** Every AI/transcription SDK or tutorial that assumes an S3 API (the de facto standard) needs manual translation to the Azure Blob SDK — a recurring tax through the whole build, not a one-time setup cost.

### Pre-Mortem — How This Could Fail

The team picked Azure Container Apps because the developer already knew Azure and the free tier made the first year cost nothing. Everything ran smoothly through the free Postgres Flexible Server year — until month 13, when a forgotten free-tier expiry turned into a real bill the developer hadn't budgeted for, arriving the same month AI transcription usage also spiked. Around the same time, an agent had bumped logging verbosity to help debug a matching bug, and nobody caught that Log Analytics ingestion — billed separately from compute — had quietly become the largest line on the Azure bill, invisible in the cost estimate anyone had checked at launch. The app had been deployed in default single-revision mode, so when a bad EF Core migration shipped, there was no traffic-weight rollback available — recovery meant manually redeploying the last known-good image and replaying the migration by hand, at 11pm, before the next morning's client meetings. None of this was a platform failure; it was a series of defaults nobody had revisited after the initial "it just works" launch.

### Unknown Unknowns

- Default single-revision mode has no built-in rollback safety net unless multi-revision mode is enabled ahead of time — not obvious until you need it and the traffic-weight command doesn't apply.
- Log Analytics costs are billed through a separate Azure service, so ACA's own pricing calculator undercounts real spend.
- Azure Blob's lack of S3-API compatibility is invisible until the first S3-first library or tutorial needs manual translation.
- A region being GA doesn't guarantee every dependent service (Postgres Flexible Server, specific Blob redundancy tiers) is available there on day one — needs a direct per-service check against Azure's "Products available by region" page before committing.
- .NET 8/9 go EOL November 10, 2026 — .NET 10 is the only safe LTS choice at this point, a trap for anyone tempted to pin an older, "more battle-tested" TFM out of caution.

## Operational Story

- **Preview deploys**: ACA has no automatic per-PR preview URL the way Vercel/Netlify do. The practical pattern is multi-revision mode: deploy a new revision with 0% production traffic, validate it via its direct revision FQDN, then shift traffic weight once confirmed — rely on this plus code review before merge, not ephemeral per-PR URLs.
- **Secrets**: Container Apps secrets (encrypted, referenced by env var) for simple values; Azure Key Vault + managed identity for anything sensitive (transcription API keys, Postgres connection string) — never in the Dockerfile or repo. Rotation = update the Container App secret or Key Vault entry, then trigger a new revision.
- **Rollback**: with multi-revision mode enabled, `az containerapp update --traffic-weight <previous-revision>=100` shifts traffic back in seconds, no rebuild. In default single-revision mode, rollback means `az containerapp update --image <previous-tag>` — a redeploy, not an instant traffic shift. EF Core migrations don't auto-rollback with either method; a down-migration or restore is still manual.
- **Approval**: merge to `main` = approval for production, per the existing auto-deploy-on-merge CI flow (`tech-stack.md`'s `ci_default_flow: auto-deploy-on-merge`). An agent may unattended: read logs, check revision health, redeploy the current `main` image, shift traffic between existing revisions. Human-only: enabling/disabling multi-revision mode, Key Vault access-policy changes, resource-group/subscription-level changes, deleting the Postgres server or storage account.
- **Logs**: `az containerapp logs show --name <app> -g <rg> --follow --type console` for live tailing; Log Analytics/Azure Monitor for aggregated queries. Azure MCP Server 2.0 (GA) exposes structured tool access across ACA/Postgres/Blob for an agent doing multi-step discovery instead of parsing CLI output.

## Risk Register

| Risk | Source | Likelihood | Impact | Mitigation |
|---|---|---|---|---|
| Log Analytics ingestion becomes a hidden, undercounted cost line | Devil's advocate | M | M | Configure Basic Logs tier or reduce ASP.NET Core production log verbosity before launch; set a Log Analytics-specific budget alert separate from the general ACA budget alert. |
| Poland Central service-parity across ACA + Postgres Flexible Server + Blob Storage not independently confirmed | Devil's advocate / Unknown unknowns | L | M | Check Azure's "Products available by region" page for all three services before committing the region; fall back to West Europe if any component is unavailable in Poland Central. |
| Postgres Flexible Server free B1ms tier expires after 12 months, becoming a real recurring bill (~$12+/mo) | Devil's advocate / Research finding | H | L | Set a reminder at month 11 to budget the transition; the amount is small but should not arrive as a surprise. |
| Default single-revision mode has no built-in rollback safety net | Devil's advocate / Unknown unknowns | M | M | Enable multi-revision mode from the first deployment, not after an incident; document the traffic-weight rollback command in the deploy runbook. |
| Azure Blob Storage is not S3-compatible | Devil's advocate / Unknown unknowns | M | L | Use the Azure Blob SDK directly for voice-recording/photo uploads; don't assume an AI/transcription library's default S3 integration will work unmodified. |
| .NET 8/9 reach end of life 2026-11-10 | Unknown unknowns | L | M | Already mitigated by pinning .NET 10 in `tech-stack.md`; no action needed beyond staying on .NET 10 for the MVP's lifetime. |
| Existing $100 AWS credit goes unused under this recommendation | Research finding | — | L | Not a platform defect — the developer explicitly chose Azure's agent-friendliness and free-tier economics over spending the AWS credit. Revisit AWS (ECS Express Mode, ~$25-100/mo) as a fallback only if Azure costs exceed expectations. |

## Getting Started

1. **CLI and login** (human, once): install the Azure CLI and Container Apps extension — `az extension add --name containerapp --upgrade`; `az login`.
2. **Provision the app**: `az containerapp up --name brokerpulse-api --resource-group brokerpulse-rg --location polandcentral --source ./api` (or `--image` once a Dockerfile/ACR image exists) — single command auto-creates the resource group, Container Apps environment, and Log Analytics workspace. Multi-stage Dockerfile: .NET 10 SDK → ASP.NET 10 runtime, Kestrel bound to `0.0.0.0` reading the injected `PORT`.
3. **Enable multi-revision mode immediately**: `az containerapp revision set-mode --name brokerpulse-api --resource-group brokerpulse-rg --mode multiple` — so traffic-weight rollback is available from day one, not bolted on after an incident.
4. **Database**: provision Azure Database for PostgreSQL Flexible Server (Burstable B1ms, free-tier eligible) in the same resource group/region; store the connection string as a Container Apps secret or in Key Vault with managed identity — never in the Dockerfile or repo.
5. **Storage**: provision a Blob Storage account (Hot tier) for voice recordings and property photos; use the Azure Blob SDK directly (not an S3-compatible client) for uploads.
6. **Background job**: create an ACA Job for the transcription step, triggered on new voice-note upload (Event Grid/Blob trigger, or a simple scheduled poll) rather than an always-on worker — matches the Q1 interview answer.
7. **CI/CD and cost guardrails**: wire the existing GitHub Actions auto-deploy-on-merge flow to `az containerapp update --image <new-tag>`; set a Log Analytics-specific budget alert (separate from the general ACA budget alert) before real client data lands on the app.

## Out of Scope

The following were not evaluated in this research:
- Docker image configuration
- CI/CD pipeline setup (GitHub Actions + `az containerapp update` is only referenced)
- Production-scale architecture (multi-region, HA, DR)
