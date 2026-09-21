---
bootstrapped_at: 2026-09-21T19:11:53Z
starter_id: dotnet
starter_name: ".NET (ASP.NET Core webapi)"
project_name: broker-pulse
language_family: dotnet
package_manager: dotnet
cwd_strategy: subdir-then-move
bootstrapper_confidence: verified
phase_3_status: ok
audit_command: "dotnet list package --vulnerable --include-transitive"
---

## Hand-off

Frontmatter copied from `context/foundation/tech-stack.md`:

```yaml
starter_id: dotnet
package_manager: dotnet
project_name: broker-pulse
hints:
  language_family: multi
  team_size: solo
  deployment_target: self-host
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
```

Run-time override (this run only, hand-off file on disk unchanged): `hints.language_family` `multi` -> `dotnet`, so that the post-scaffold audit runs for the .NET backend. With `multi` the audit would have been skipped.

The hand-off was read from `context/foundation/tech-stack.md`; the invocation argument `@tech-stack.md` pointed at a root-level file that does not exist.

### Why this stack

A solo developer with seven years of .NET experience is building BrokerPulse after hours over roughly 15 weeks, with role-based auth, an AI transcription and profile-extraction step, and background processing of voice notes. Custom path: ASP.NET Core webapi passes all four agent-friendly gates and has verified bootstrapper confidence, and it plays to the strongest skill on the team, with PostgreSQL via EF Core planned as the data store. The frontend is a separately scaffolded Astro app with React islands (recording, profile review, ranked matches), chosen deliberately despite the registry's note that Astro is not a SPA framework; the author accepts the learning curve and cannot yet judge agent output on the Astro side. Mobile is covered by a responsive web app, since the PRD requires the core flow to work in a mobile browser rather than a native app. Deployment is containers on AWS (recorded as self-host), with files on S3, and CI runs on GitHub Actions with auto-deploy on merge.

## Pre-scaffold verification

| Signal      | Value   | Severity | Notes                                                                 |
| ----------- | ------- | -------- | --------------------------------------------------------------------- |
| npm package | not run | n/a      | non-JS starter; `cmd_template` does not invoke an npm-distributed CLI |
| GitHub repo | not run | n/a      | card `docs_url` is `learn.microsoft.com/aspnet/core`, not a GitHub URL; no recency signal available |

## Scaffold log

**Resolved invocation**: `dotnet new webapi -n .bootstrap-scaffold --no-restore`
**Strategy**: subdir-then-move
**Exit code**: 0
**Files moved**: 6 (`.bootstrap-scaffold.csproj`, `.bootstrap-scaffold.http`, `appsettings.json`, `appsettings.Development.json`, `Program.cs`, `Properties/launchSettings.json`)
**Conflicts (.scaffold siblings)**: none
**.gitignore handling**: absent in scaffold (and absent in cwd)
**.bootstrap-scaffold cleanup**: deleted

Notes:

- The template derives the project file names from the `-n` value, so the project landed in cwd as `.bootstrap-scaffold.csproj` / `.bootstrap-scaffold.http` (dot-prefixed, not `broker-pulse`). `project_name` is not a substitution input in v1. Rename before building further (project file, `.http` file, and the assembly name follows the project file name).
- No `.gitignore` was shipped by the template. `dotnet restore` created `obj/` in cwd; add a `.gitignore` covering `bin/` and `obj/` before the first commit.
- Only the .NET backend was scaffolded. The Astro frontend named in the hand-off rationale is not covered by this run and must be created separately.

## Post-scaffold audit

**Tool**: `dotnet list package --vulnerable --include-transitive`
**Summary**: 0 CRITICAL, 0 HIGH, 0 MODERATE, 0 LOW
**Direct vs transitive**: not distinguished by this tool

Output: no vulnerable packages found for project `.bootstrap-scaffold` against the configured sources (`https://api.nuget.org/v3/index.json`).

Extra step outside the template: the first audit attempt failed (exit 1, "no assets file found ... run restore") because the template is run with `--no-restore`. `dotnet restore` was run in cwd (exit 0) and the audit was repeated; the result above is from the second run.

## Hints recorded but not acted on

| Hint                    | Value                                                                     |
| ----------------------- | ------------------------------------------------------------------------- |
| bootstrapper_confidence | verified                                                                  |
| quality_override        | false                                                                     |
| path_taken              | custom                                                                    |
| self_check_answers      | typed: true, from_official_starter: true, conventions: true, docs_current: true, can_judge_agent: false |
| team_size               | solo                                                                      |
| deployment_target       | self-host                                                                 |
| ci_provider             | github-actions                                                            |
| ci_default_flow         | auto-deploy-on-merge                                                      |
| has_auth                | true                                                                      |
| has_payments            | false                                                                     |
| has_realtime            | false                                                                     |
| has_ai                  | true                                                                      |
| has_background_jobs     | true                                                                      |

## Next steps

Next: a future skill will set up agent context (CLAUDE.md, AGENTS.md). For now, your project is scaffolded and verified — happy hacking.

Useful manual steps in the meantime:
- `git init` (if you have not already) to start your own repo history.
- Review any `.scaffold` siblings the conflict policy created and decide which version of each file to keep (none this run).
- Address audit findings per your project's risk tolerance — the full breakdown is in this log (none this run).
