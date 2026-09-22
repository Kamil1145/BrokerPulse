# web/ Agent Guidelines

Astro frontend (TypeScript strict) with React islands, npm. Serves the core BrokerPulse flow: voice recording, profile review, ranked matches.

## Hard rules

- React (`@astrojs/react`) is the established framework for interactive islands — use `.tsx` components with Astro's client directives (`client:load`, `client:visible`, etc.) for anything needing client-side state. Keep `.astro` files for static structure only, per Astro's zero-JS-by-default model.
- TypeScript is strict (`tsconfig.json` extends `astro/tsconfigs/strict`) — no `any` without a comment explaining why.

## Commands

`npm run dev` (or `astro dev --background` to run detached — manage with `astro dev stop|status|logs`), `npm run build`, `npm run preview`.

## Current state

`src/pages/`, `src/layouts/`, `src/components/` hold one file each (`index.astro`, `Layout.astro`, `Welcome.astro`) — the unmodified Astro template, not real features yet. No test framework is configured (no Vitest, no `npm test`).

## See also

`@../CLAUDE.md` for repo-wide rules (repository layout, `context/` conventions). Full Astro docs: https://docs.astro.build
