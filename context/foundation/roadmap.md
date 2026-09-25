---
project: BrokerPulse
version: 1
status: draft
created: 2026-09-25
updated: 2026-09-25
prd_version: 1
main_goal: market-feedback
top_blocker: capacity
milestone_id: voice-note-to-matched-listings
milestone_seq: 1
milestone_status: open
---

# Roadmap: BrokerPulse

> Derived from `context/foundation/prd.md` (v1) + `tech-stack.md` + `infrastructure.md` + `deploy-plan.md` + an auto-researched codebase baseline.
> Edit-in-place; archive when superseded.
> Slices below are listed in dependency order. The "At a glance" table is the index.

## Milestone

**M-1: Voice note to matched listings** — Status: open

- **Intent:** A solo agent can log in, record a voice note after a client meeting, confirm the profile the system extracted from it, and see a ranked list of matching listings with a visible reason for each match, from an inventory the agent (or an admin) maintains. This is the PRD's whole MVP loop, delivered so the most doubtful step (turning speech into a correct profile) is tested first.
- **Source materials:** `context/foundation/prd.md` (v1)
- **Done when:** every F-NN and S-NN below is `done`.
- **Scope anchors:** FR-001 to FR-011 and US-01 of the PRD.

## Vision recap

A field real-estate agent loses time re-typing conversations into a CRM after every meeting. BrokerPulse turns a dictated voice note directly into a structured client profile and matches it against the listings inventory, so the spoken account becomes the record without a manual transcription step.

## North star

**S-02: Agent records a voice note and confirms the extracted client profile** — this is the step of the product that is most likely to fail (speech recognition and field extraction quality), so proving it first is what the "market-feedback" goal asks for.

> "North star" here means the smallest end-to-end slice whose successful delivery would prove the core product hypothesis — placed as early as Prerequisites allow because everything else only matters if this works.

## At a glance

| ID   | Change ID                              | Outcome (user can …)                                                        | Prerequisites | PRD refs                      | Status   |
| ---- | -------------------------------------- | --------------------------------------------------------------------------- | ------------- | ----------------------------- | -------- |
| F-01 | database-access-and-first-migration    | (foundation) API reaches the database and migrations apply from CI          | —             | NFR (privacy), FR-003         | ready    |
| F-02 | test-and-verification-baseline         | (foundation) both components run tests in CI on every PR                    | —             | Guardrails (accuracy)         | ready    |
| S-01 | agent-login-and-own-clients            | log in and see only their own (initially empty) client list                 | F-01          | FR-002, FR-003                | proposed |
| S-02 | voice-note-to-confirmed-profile        | record a voice note and confirm or correct the extracted client profile     | S-01, F-02    | US-01, FR-007, FR-008, FR-009 | proposed |
| S-03 | create-listing-with-photos             | add a listing with property details and photos                              | S-01          | FR-004                        | proposed |
| S-04 | listing-edit-and-archive               | edit a listing or archive it without losing its match history               | S-03          | FR-005                        | proposed |
| S-05 | ranked-matches-with-rationale          | see a ranked list of matching listings, each with a score and the reason    | S-02, S-03, F-02 | US-01, FR-010, FR-011      | proposed |
| S-06 | profile-edit-and-rematch               | edit a saved profile and re-run matching when inventory or profile changes  | S-05, S-04    | US-01, FR-009, FR-011         | proposed |
| S-07 | admin-creates-agent-accounts           | (admin) create agent accounts inside the brokerage                          | S-01          | FR-001                        | proposed |
| S-08 | admin-oversight-and-client-reassignment| (admin) see all agents' clients and reassign a client to another agent      | S-07, S-02    | FR-003                        | proposed |
| S-09 | browse-and-search-listings             | browse and search the listings inventory outside the per-client flow        | S-03          | FR-006                        | proposed |

## Streams

Navigation aid — groups items that share a Prerequisites chain. Canonical ordering still lives in the dependency graph below; this table is the proposed reading order across parallel tracks.

| Stream | Theme                        | Chain                                                | Note                                                                  |
| ------ | ---------------------------- | ---------------------------------------------------- | --------------------------------------------------------------------- |
| A      | Voice note to matches        | `F-01` → `S-01` → `S-02` → `S-05` → `S-06`           | The north star and the loop it feeds; the fastest way to test the core assumption. |
| B      | Listings inventory           | `S-03` → `S-04` → `S-09`                             | Joins Stream A at `S-01`; supplies the inventory that `S-05` matches against. |
| C      | Admin and multi-agent basics | `S-07` → `S-08`                                      | Joins Stream A at `S-01` and `S-02`; kept late because the MVP starts with one user. |
| D      | Verification                 | `F-02`                                               | Standalone; runs in parallel with `F-01`, needed before `S-02` and `S-05`. |

## Baseline

What's already in place in the codebase as of `2026-09-25` (auto-researched + user-confirmed).
Foundations below assume these are present and do NOT re-scaffold them.

- **Frontend:** partial — Astro 7 with React 19 islands is configured and deployed (Cloudflare Workers static assets), but only the template pages exist (`web/src/pages/index.astro`); no islands, no API calls, no audio recording.
- **Backend / API:** partial — ASP.NET Core minimal API with a vertical-slice convention, `/health` and a template `WeatherForecast` sample (`api/Features/`), deployed on Azure Container Apps; no domain endpoints.
- **Data:** partial — PostgreSQL 16 provisioned (Microsoft Entra authentication only), database `brokerpulse` and an app role exist, plus an Azure Blob container `uploads`; but there is no EF Core, no schema, no migration path, and no code that connects to the database.
- **Auth:** absent — no user authentication in the code (Entra covers only the app's access to the database).
- **Deploy / infra:** present — CI on pull requests, deploys gated by CI, verify-then-shift API deploys, OIDC with narrow roles, protected `main`, secret scanning (`context/deployment/deploy-plan.md`).
- **Observability:** absent — only `/health`; no structured logging, error tracking, metrics or alerts.
- **Tests:** absent — no test project in `api/`, no test framework in `web/`.
- **AI / background work:** absent — no transcription, extraction or background processing (the stack expects both).

## Foundations

### F-01: Database access and first migration

- **Outcome:** (foundation) the API connects to PostgreSQL with a token (no password), a first migration applies from CI through a dedicated migration role, and `/health` also reports database reachability.
- **Change ID:** database-access-and-first-migration
- **PRD refs:** NFR (privacy), FR-003
- **Unlocks:** `S-01` (users and roles need storage), and through it every later slice; also the named verification path "a revision that cannot reach the database must not receive traffic".
- **Prerequisites:** —
- **Parallel with:** F-02
- **Blockers:** —
- **Unknowns:**
  - Can the GitHub runner obtain the Entra token needed to run migrations with the pipeline identity, or is a different step needed? — Owner: user. Block: no.
- **Risk:** Sequenced first because every slice stores something, and the access model was decided but never wired; the smallest possible slice of it (one table, one migration) proves the path without designing the whole schema.
- **Status:** ready

### F-02: Test and verification baseline

- **Outcome:** (foundation) `api/` has a test project and `web/` a test or type-check step, both run by CI on every pull request, each with one passing example test.
- **Change ID:** test-and-verification-baseline
- **PRD refs:** Guardrails (accuracy)
- **Unlocks:** `S-02` (extraction quality needs a way to be checked against fixed samples), `S-05` (ranking needs repeatable tests); it also closes the "CI proves only that it builds" gap.
- **Prerequisites:** —
- **Parallel with:** F-01, S-01
- **Blockers:** —
- **Unknowns:** —
- **Risk:** Without it the two most doubtful slices (`S-02`, `S-05`) can only be judged by eye; kept minimal so it does not grow into a test framework project of its own.
- **Status:** ready

## Slices

### S-01: Agent login and own clients

- **Outcome:** user can log in (email and password or OAuth) and see only their own client list, which starts empty, with the admin and agent roles recognised.
- **Change ID:** agent-login-and-own-clients
- **PRD refs:** FR-002, FR-003
- **Prerequisites:** F-01
- **Parallel with:** F-02
- **Blockers:** —
- **Unknowns:**
  - Which login providers ship first (email/password, OAuth, or both)? — Owner: user. Block: no.
  - How does the very first admin account come to exist? — Owner: user. Block: no.
- **Risk:** Sequenced right after F-01 because every later slice needs an identity to attach data to; the ownership rule ("agent sees only their own") is set here so later slices inherit it instead of retrofitting it.
- **Status:** proposed

### S-02: Voice note to confirmed profile

- **Outcome:** user can record a voice note after a meeting and confirm or correct the client profile the system extracted from it.
- **Change ID:** voice-note-to-confirmed-profile
- **PRD refs:** US-01, FR-007, FR-008, FR-009
- **Prerequisites:** S-01, F-02
- **Parallel with:** S-03, S-07
- **Blockers:** —
- **Unknowns:**
  - Which service transcribes and extracts, and what processing terms apply to recordings and transcripts, which are personal data? — Owner: user. Block: no.
  - Does browser audio recording work reliably on the phones the agent actually uses? — Owner: user. Block: no.
  - Which profile fields are extracted, and do they line up with the listing attributes? — Owner: user. Block: no.
- **Risk:** This is the north star: the product's central assumption lives here, so it is placed as early as its prerequisites allow; the extraction step must never invent data, which is why the confirm-or-edit step is part of the slice, not a later addition.
- **Status:** proposed

### S-03: Create listing with photos

- **Outcome:** user can add a listing with property details and photos to the inventory.
- **Change ID:** create-listing-with-photos
- **PRD refs:** FR-004
- **Prerequisites:** S-01
- **Parallel with:** S-02, S-07
- **Blockers:** —
- **Unknowns:**
  - Which listing attributes are required for matching, using the same vocabulary as the profile fields? — Owner: user. Block: no.
- **Risk:** Independent of the voice path, so it can run beside S-02 and give S-05 something to match against; kept to creation only so it stays small.
- **Status:** proposed

### S-04: Listing edit and archive

- **Outcome:** user can edit an existing listing or archive it so it leaves active matching but its match history is kept.
- **Change ID:** listing-edit-and-archive
- **PRD refs:** FR-005
- **Prerequisites:** S-03
- **Parallel with:** S-02, S-05, S-07, S-09
- **Blockers:** —
- **Unknowns:** —
- **Risk:** Archiving instead of deleting is a deliberate PRD decision (match history must survive); only re-matching in S-06 depends on it, so it stays off the north-star path and can run beside S-02 and S-05.
- **Status:** proposed

### S-05: Ranked matches with rationale

- **Outcome:** user can see a ranked list of matching listings for a confirmed profile, each with a visible score and the reason it matched, and a clear empty state when nothing matches.
- **Change ID:** ranked-matches-with-rationale
- **PRD refs:** US-01, FR-010, FR-011
- **Prerequisites:** S-02, S-03, F-02
- **Parallel with:** S-04, S-07, S-09
- **Blockers:** —
- **Unknowns:**
  - How is the match computed and scored (fixed rules, a model, or both), and what scale does the agent see? — Owner: user. Block: no.
- **Risk:** The guardrail says the system must not present wildly mismatched listings without a way to verify them, so the visible reason is part of the outcome rather than polish; F-02 gives the repeatable tests that make ranking checkable.
- **Status:** proposed

### S-06: Profile edit and rematch

- **Outcome:** user can edit a saved client profile and re-run matching, including after the inventory changes, so the list never silently goes stale.
- **Change ID:** profile-edit-and-rematch
- **PRD refs:** US-01, FR-009, FR-011
- **Prerequisites:** S-05, S-04
- **Parallel with:** S-07, S-08, S-09
- **Blockers:** —
- **Unknowns:** —
- **Risk:** Sequenced after S-04 so archived listings drop out of matching correctly; a stale list is the failure the PRD calls out, which is why re-triggering is agent-driven and explicit.
- **Status:** proposed

### S-07: Admin creates agent accounts

- **Outcome:** user (an admin) can create agent accounts inside their brokerage.
- **Change ID:** admin-creates-agent-accounts
- **PRD refs:** FR-001
- **Prerequisites:** S-01
- **Parallel with:** S-02, S-03, S-04, S-05, S-06, S-09
- **Blockers:** —
- **Unknowns:**
  - How does a new agent receive access (an emailed invitation or an admin-set credential), given no email service is in the stack yet? — Owner: user. Block: no.
- **Risk:** The role split is a deliberate growth-path decision, but the MVP starts with a single user, so this slice is off the north-star path and safe to run in parallel when capacity allows.
- **Status:** proposed

### S-08: Admin oversight and client reassignment

- **Outcome:** user (an admin) can see all agents' clients within the brokerage and reassign a client to a different agent.
- **Change ID:** admin-oversight-and-client-reassignment
- **PRD refs:** FR-003
- **Prerequisites:** S-07, S-02
- **Parallel with:** S-04, S-05, S-06, S-09
- **Blockers:** —
- **Unknowns:** —
- **Risk:** The admin view must never widen what an agent can see; sequenced after there are real clients (S-02) so the isolation rule can be tested against actual data.
- **Status:** proposed

### S-09: Browse and search listings

- **Outcome:** user can browse and search the listings inventory outside the per-client matching flow.
- **Change ID:** browse-and-search-listings
- **PRD refs:** FR-006
- **Prerequisites:** S-03
- **Parallel with:** S-02, S-04, S-05, S-06, S-07, S-08
- **Blockers:** —
- **Unknowns:** —
- **Risk:** The only nice-to-have in the PRD; kept last and independent so it can be dropped without touching the core loop if capacity runs out.
- **Status:** proposed

## Backlog Handoff

| Roadmap ID | Change ID                               | Suggested issue title                                         | Ready for `/10x-plan` | Notes |
| ---------- | --------------------------------------- | ------------------------------------------------------------- | --------------------- | ----- |
| F-01       | database-access-and-first-migration     | Wire API to Postgres with token auth and a first CI migration | yes                   | Recommended first move; unlocks S-01 and the north star |
| F-02       | test-and-verification-baseline          | Add test projects and CI test steps for api and web           | yes                   | Can run in parallel with F-01 |
| S-01       | agent-login-and-own-clients             | Agent login with roles and own-client list                    | no                    | Waits for F-01 |
| S-02       | voice-note-to-confirmed-profile         | Voice note to confirmed client profile (north star)           | no                    | Waits for S-01 and F-02 |
| S-03       | create-listing-with-photos              | Create a listing with details and photos                      | no                    | Waits for S-01 |
| S-04       | listing-edit-and-archive                | Edit and archive listings, keeping match history              | no                    | Waits for S-03 |
| S-05       | ranked-matches-with-rationale           | Ranked listing matches with score and rationale               | no                    | Waits for S-02, S-03, F-02 |
| S-06       | profile-edit-and-rematch                | Edit a saved profile and re-run matching                      | no                    | Waits for S-05, S-04 |
| S-07       | admin-creates-agent-accounts            | Admin creates agent accounts                                  | no                    | Waits for S-01 |
| S-08       | admin-oversight-and-client-reassignment | Admin views all clients and reassigns a client                | no                    | Waits for S-07, S-02 |
| S-09       | browse-and-search-listings              | Browse and search the listings inventory                      | no                    | Waits for S-03; nice-to-have |

## Open Roadmap Questions

1. **Timeline is acknowledged as a sustained-effort commitment, not a hard deadline.** — Owner: user. Block: roadmap-wide (informational). Note: copied from the PRD; `shape-notes.md` records the 15-week estimate as accepted and no hard deadline was set.
2. **Profile fields and listing attributes must share one vocabulary** (for example budget, size, location, rooms), or matching has nothing to compare. — Owner: user. Block: gates S-02, S-03 and S-05 at planning time, but is decided inside their plans rather than before them.
3. **When do real client data and voice recordings first enter the system, and what is the retention and deletion rule for recordings and transcripts?** — Owner: user. Block: none for planning; must be settled before S-02 is used with a real client. A private-networking review of the database belongs to the same moment (see `deploy-plan.md`).

## Parked

- **Analyzing incoming SMS or email as another source for the client profile** — Why parked: PRD §Non-Goals; only the voice note is the profile source in this MVP.
- **Sending matched offers to the client by SMS or email** — Why parked: PRD §Non-Goals; the MVP ends at showing the agent the ranked list.
- **Sharing one client between several agents at the same time** — Why parked: considered and rejected in the FR-003 discussion; the admin can reassign a client instead.
- **A native mobile app** — Why parked: the PRD requires only that the core flow work in a mobile browser; the tech stack chose a responsive web app.
- **Structured logging, error tracking and alerts** — Why parked: the goal is to test the core assumption early, and the baseline shows no operational need yet; revisit when real users start recording notes (it appears as an open gap in `deploy-plan.md`).

## Milestone History

(Append-only. Carried forward verbatim into each successor milestone's roadmap; empty on the very first milestone.)

## Done

(Empty on first generation. `/10x-archive` appends an entry here — and flips that item's `Status` to `done` — when a change whose `Change ID` matches the item is archived. Do NOT pre-populate.)
