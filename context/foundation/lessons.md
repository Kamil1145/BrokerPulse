# Lessons Learned

> Append-only register of recurring rules and patterns. Re-read at start by /10x-frame, /10x-research, /10x-plan, /10x-plan-review, /10x-implement, /10x-impl-review.

## Every feature flag must carry a kill date

- **Context**: Any phase that adds a feature flag in `api/` or `web/` (rolling out a feature behind a flag before full release).
- **Problem**: Flags left without an expiry date accumulate indefinitely — they multiply the paths to test, mask dead code, and make it hard for an agent to judge whether a flag is still needed.
- **Rule**: Whenever a feature flag is introduced, record a planned removal date or removal criterion next to its definition (or in the task/PR description); `impl-review` must check that this date/criterion exists.
- **Applies to**: plan, implement, impl-review
