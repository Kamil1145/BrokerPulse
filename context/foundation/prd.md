---
project: "BrokerPulse"
version: 1
status: draft
created: 2026-09-20
context_type: greenfield
product_type: web-app
target_scale:
  users: small
  qps: low
  data_volume: small
timeline_budget:
  mvp_weeks: 15
  hard_deadline: null
  after_hours_only: true
---

## Vision & Problem Statement

A solo real estate agent working in the field loses time and focus to manual paperwork: after a client meeting or property viewing, they must translate the conversation into structured CRM data by hand. Today that data lives scattered across spreadsheets and local disk folders for photos — no coherent, structured source of truth. Existing CRM tools in this space (e.g. Galaktyka Virgo) are clunky, desktop/form-oriented, and not built for fieldwork where the agent has neither the time nor the setting to fill out forms manually.

The insight: field agents already narrate their client and property observations out loud or in their heads during and after a viewing — that spoken account can become the structured record directly, without a manual transcription step, if the system can turn a dictated voice note into a structured client profile and match it against listings automatically.

## User & Persona

Solo real estate agent / sole proprietor, managing their own portfolio of clients and listings without a team. Works primarily in the field — between property viewings and client meetings — and reaches for this product right after a conversation with a client or a walkthrough of a property, to capture what just happened before it's lost to a notebook, a spreadsheet, or memory.

## Success Criteria

### Primary
- Agent records a voice note after a client meeting; the system turns it into a structured client profile without manual data entry.
- The system matches the client profile against the listings inventory and surfaces a ranked list of matching offers to the agent.

### Secondary
- Agent can edit the client profile after voice transcription, to correct speech-recognition errors or add missing details manually.

### Guardrails
- Client data privacy: voice recordings and personal client data must not leak or be accessible to the wrong agent.
- Transcription/matching accuracy: the system must not fabricate client data, and must not present wildly mismatched listings without the agent being able to verify and correct results.

## User Stories

### US-01: Agent turns a post-meeting voice note into matched listings

- **Given** a logged-in agent with at least one listing in the brokerage's inventory
- **When** they record a voice note describing a client's needs, preferences, and budget right after a meeting
- **Then** the system transcribes the note into a structured client profile, the agent can review/correct it, and the system returns a ranked list of matching listings from the inventory

#### Acceptance Criteria
- Client profile fields (needs, preferences, budget) are extracted without the agent manually filling a form
- Agent can edit any extracted field before matching runs, or re-trigger matching after edits
- Ranked list shows at least the top matches with visible basis for the match (e.g. matched criteria), not an opaque score
- Empty or no-match inventory shows an explanatory empty state, not a 0-result list with no context

## Functional Requirements

### Accounts & Access
- FR-001: Admin can create agent accounts within their brokerage. Priority: must-have
  > Socratic: Counter-argument considered: none offered. Resolution: kept as written; role-based accounts from day one are a deliberate choice to support the product's growth into multi-agent teams.
- FR-002: Agent can log in with email/password or OAuth. Priority: must-have
  > Socratic: Counter-argument considered: none offered. Resolution: kept as written; both login methods are low-cost to support via standard auth providers.
- FR-003: Agent can only see and manage their own clients and voice notes; admin can see all agents' data within the brokerage. Admin can reassign a client to a different agent. Priority: must-have
  > Socratic: Counter-argument considered: "strict 1:1 isolation blocks teams that need to share a client on a complex deal." Resolution: modified — admin gains the ability to reassign a client to another agent, without full multi-agent sharing.

### Listings Management
- FR-004: Admin/agent can create a new listing (property details + photos). Priority: must-have
  > Socratic: Counter-argument considered: none offered. Resolution: kept as written; without listings in the system there is nothing to match against.
- FR-005: Admin/agent can archive an existing listing (removed from active matching but its match history is preserved); editing remains available. Priority: must-have
  > Socratic: Counter-argument considered: "permanently removing a listing breaks match history for clients it was already recommended to." Resolution: modified — removal is archiving, preserving match history.
- FR-006: Agent can browse/search the listings inventory. Priority: nice-to-have
  > Socratic: Counter-argument considered: none offered. Resolution: kept as written; agent sometimes needs to check inventory outside the automatic per-client matching flow.

### Voice Note → Client Profile
- FR-007: Agent can record a voice note describing a client, at any time after a meeting (not required to happen on-site). Priority: must-have
  > Socratic: Counter-argument considered: "agent may not have privacy/comfortable conditions to speak aloud in the field." Resolution: confirmed as written — recording is not tied to being on-site during the meeting, so the agent can record later when comfortable.
- FR-008: System transcribes the voice note and extracts a structured client profile (needs, preferences, budget, etc.). Priority: must-have
  > Socratic: Counter-argument considered: none offered. Resolution: kept as written; extraction-error risk is addressed by FR-009's confirmation step.
- FR-009: After transcription, the system prompts the agent to confirm the extracted profile is correct or edit it before proceeding. Priority: must-have
  > Socratic: Counter-argument considered: "if the agent must manually fix most fields every time, voice transcription isn't saving time — signal to improve extraction quality, not to drop the FR." Resolution: modified — the system actively prompts for confirmation/correction after transcription rather than passively waiting for an optional edit.

### Offer Matching
- FR-010: System matches a client profile against the listings inventory and produces a ranked list of matching offers, each with a visible match score/scale and rationale. Priority: must-have
  > Socratic: Counter-argument considered: "an unexplained ranking may not be trusted by the agent." Resolution: modified — ranking must surface a match score/scale and rationale, not just an ordering.
- FR-011: Agent can view the ranked list of matched offers for a client, and can trigger re-evaluation of the match when the inventory changes. Priority: must-have
  > Socratic: Counter-argument considered: "a static list from one point in time can go stale as new listings appear." Resolution: modified — agent can re-trigger matching to refresh the list.

## Non-Functional Requirements

- Personal client data and voice recordings are accessible only to the owning/assigned agent and the brokerage's admin (per FR-003), consistent with GDPR requirements for personal data.
- The product remains usable both in a web browser and on a mobile device, with the core flow (recording, profile review, matches) available on both.

## Business Logic

The system ranks listings from the inventory by how well they match the client's preferences and budget extracted from the voice note.

The rule consumes two user-facing inputs: the structured client-preference profile extracted from the voice note (needs, preferences, budget) and the current inventory of available listings. Its output is a ranked list of listings, each carrying a visible match score/scale and a rationale. The agent encounters this rule immediately after confirming the client profile — the ranking appears right away, and the agent can re-trigger it later if the inventory has changed.

## Access Control

Login required (email/password or OAuth). Two roles from day one: `admin` and `agent`. Agents see and manage only their own clients and voice notes; admins see all agents' data within the brokerage and can reassign a client to a different agent. The product is built to grow into multi-agent teams under one brokerage; MVP usage starts with a single user occupying either role, but the role split is load-bearing for the product's growth path, not a placeholder.

## Non-Goals

- Avoid: analyzing incoming SMS/email as an additional source for building the client profile — only the voice note is the profile source in this MVP; SMS/email as an alternate input is a deliberately deferred extension.
- Avoid: automatically sending matched-offer recommendations to the client via SMS/email — MVP ends at showing the agent the ranked matches; outbound sending to the client is a post-MVP step.

## Open Questions

1. **Timeline is acknowledged as a sustained-effort commitment, not a hard deadline.** — Owner: user. Note: shape-notes.md records the 15-week estimate (mid-September to December 2026) as accepted after scope-cost disclosure; no hard deadline was set (`hard_deadline: null`).
