---
starter_id: dotnet
package_manager: dotnet
project_name: broker-pulse
hints:
  language_family: multi
  team_size: solo
  deployment_target: "azure-container-apps (api) + cloudflare-workers (web)"
  ci_provider: github-actions
  ci_default_flow: auto-deploy-on-merge
  bootstrapper_confidence: verified
  path_taken: custom
  quality_override: false
  self_check_answers:
    typed: true
    from_official_starter: true
    conventions: true
    docs_current: true
    can_judge_agent: false
  has_auth: true
  has_payments: false
  has_realtime: false
  has_ai: true
  has_background_jobs: true
---

## Why this stack

A solo developer with seven years of .NET experience is building BrokerPulse after hours over roughly 15 weeks, with role-based auth, an AI transcription and profile-extraction step, and background processing of voice notes. Custom path: ASP.NET Core webapi passes all four agent-friendly gates and has verified bootstrapper confidence, and it plays to the strongest skill on the team, with PostgreSQL via EF Core planned as the data store. The frontend is a separately scaffolded Astro app with React islands (recording, profile review, ranked matches), chosen deliberately despite the registry's note that Astro is not a SPA framework; the author accepts the learning curve and cannot yet judge agent output on the Astro side. Mobile is covered by a responsive web app, since the PRD requires the core flow to work in a mobile browser rather than a native app. Deployment (updated after the infrastructure research and first deploy): the API runs as a container on Azure Container Apps, the Astro frontend on Cloudflare Workers static assets, data in PostgreSQL Flexible Server (Microsoft Entra authentication only, no passwords) with files in Azure Blob Storage, and CI runs on GitHub Actions with auto-deploy on merge behind a CI gate.
