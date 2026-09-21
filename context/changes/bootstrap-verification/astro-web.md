---
bootstrapped_at: 2026-09-21T19:26:25Z
starter_id: astro
starter_name: "Astro"
project_name: broker-pulse
target_dir: web
language_family: js
strategy: native-target
bootstrapper_confidence: verified
status: ok
audit_command: "npm audit --json"
---

## Plan

| Pole           | Wartość                                                                    |
| -------------- | -------------------------------------------------------------------------- |
| Technologia    | astro, Astro (pewność: verified)                                           |
| Katalog        | `web` (argument `/web` znormalizowany do ścieżki względnej `web`)          |
| Nazwa projektu | broker-pulse (szablon Astro nie używa jej w komendzie)                     |
| Komenda        | `npm create astro@latest -- web --template basics --install --no-git --yes` |

Uspójnienie plików instrukcji agenta (na polecenie użytkownika, poza domyślnym zakresem skilla): wybrano wariant „jeden CLAUDE.md w korzeniu”.

## Aktualność

| Sygnał      | Wartość                                  | Waga  | Uwagi                                      |
| ----------- | ---------------------------------------- | ----- | ------------------------------------------ |
| npm package | create-astro v5.2.4, zmodyfikowany 2026-08-24 | fresh | wyprowadzone z `cmd_template`              |
| GitHub repo | not run                                  | n/a   | `docs_url` to `docs.astro.build`, nie GitHub |

## Szkielet

**Resolved invocation**: `npm create astro@latest -- web --template basics --install --no-git --yes`
**Strategy**: native-target (CLI tworzy `web/` bezpośrednio)
**Exit code**: 0
**Pliki utworzone**: 15 (bez `node_modules/`); zależności zainstalowane przez `--install` (Astro ^7.3.3)
**Konflikty (.scaffold)**: none (katalog docelowy był pusty)
**.gitignore**: dostarczony przez szablon w `web/.gitignore`; root `.gitignore` bez zmian
**Zagnieżdżone .git**: nie powstało (`--no-git` zamiast `--git` z karty rejestru)

Uspójnienie plików instrukcji agenta:

- Szablon dołożył `web/CLAUDE.md` i `web/AGENTS.md` (identyczne, 874 B: uruchamianie serwera dev w tle + linki do dokumentacji).
- Zawartość dopisano do korzeniowego `CLAUDE.md` jako sekcję `## web/ (Astro)` **poniżej** markera `<!-- END @przeprogramowani/10x-cli -->`, poza blokiem zarządzanym przez CLI 10x. Flagę `astro dev --background` oraz podkomendy `stop`, `status`, `logs` zweryfikowano w `astro dev --help` (Astro 7.3.3).
- Oba pliki z `web/` usunięto po potwierdzeniu, że są identyczne. W repo pozostał jeden `CLAUDE.md`.

## Audyt

**Tool**: `npm audit --json` (uruchomiony w `web/`)
**Summary**: 0 CRITICAL, 0 HIGH, 0 MODERATE, 0 LOW (0 INFO)
**Direct vs transitive**: brak ustaleń; zależności: 287 łącznie (prod 178, optional 110, peer 2)

## Kolejne kroki

- `git init` już wykonano w korzeniu; `web/` jest nieśledzony do pierwszego commita.
- Zaplanowano użycie React (wyspy) dla nagrywania, edycji profilu i listy dopasowań: `npx astro add react` z katalogu `web/`. Nie wykonano w ramach bootstrapu.
- Karta `astro` ostrzega: „not a SPA — full SaaS apps fit better in Next/T3”. Wybór Astro dla tej aplikacji został świadomie zaakceptowany podczas doboru stosu.
