---
bootstrapped_at: 2026-09-22T17:49:04Z
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
| Katalog        | `web`                                                                       |
| Nazwa projektu | broker-pulse (szablon Astro nie używa jej w komendzie)                     |
| Komenda        | `npm create astro@latest -- web --template basics --install --no-git --yes` |

`web/` zawierał już szkielet z poprzedniego uruchomienia (2026-09-21T19:26:25Z, patrz historia git). Na polecenie użytkownika katalog został opróżniony przed tym uruchomieniem, więc CLI zastało pusty katalog docelowy zamiast trafić na ochronę przed niepustym katalogiem z Kroku 0.4.

## Aktualność

| Sygnał      | Wartość                                  | Waga  | Uwagi                                      |
| ----------- | ---------------------------------------- | ----- | ------------------------------------------ |
| npm package | create-astro v5.2.4, zmodyfikowany 2026-08-24 | fresh | wyprowadzone z `cmd_template`              |
| GitHub repo | not run                                  | n/a   | `docs_url` to `docs.astro.build`, nie GitHub |

## Szkielet

**Resolved invocation**: `npm create astro@latest -- web --template basics --install --no-git --yes`
**Strategy**: native-target (CLI tworzy `web/` bezpośrednio)
**Exit code**: 0
**Pliki utworzone**: 16 (bez `node_modules/`); zależności zainstalowane przez `--install` (Astro ^7.3.3)
**Konflikty (.scaffold)**: none (katalog docelowy był pusty — opróżniony ręcznie przed uruchomieniem)
**.gitignore**: dostarczony przez szablon w `web/.gitignore`; root `.gitignore` bez zmian
**Zagnieżdżone .git**: nie powstało (`--no-git` zamiast `--git` z karty rejestru)

Szablon dołożył `web/CLAUDE.md` i `web/AGENTS.md` (pliki instrukcji agenta dostarczone przez template Astro). Zgodnie z kontraktem skilla — nie tworzy ani nie modyfikuje plików instrukcji agenta — oba pozostały nietknięte tym razem; poprzednie uruchomienie scaliło je z korzeniowym `CLAUDE.md` na wyraźne polecenie użytkownika, ale to wykracza poza domyślny zakres skilla i nie zostało powtórzone automatycznie.

## Audyt

**Tool**: `npm audit --json` (uruchomiony w `web/`)
**Summary**: 0 CRITICAL, 0 HIGH, 0 MODERATE, 0 LOW (0 INFO)
**Direct vs transitive**: brak ustaleń; zależności: 287 łącznie (prod 178, optional 110, peer 2)

## Kolejne kroki

- `web/CLAUDE.md` i `web/AGENTS.md` czekają na decyzję: scalić z korzeniowym `CLAUDE.md` (jak poprzednio) czy usunąć jako duplikaty — do rozstrzygnięcia przez użytkownika, nie przez ten skill.
- Zaplanowano użycie React (wyspy) dla nagrywania, edycji profilu i listy dopasowań: `npx astro add react` z katalogu `web/`. Nie wykonano w ramach tego bootstrapu — dodanie biblioteki do istniejącego projektu wykracza poza zakres tego skilla.
- Karta `astro` ostrzega: „not a SPA — full SaaS apps fit better in Next/T3”. Wybór Astro dla tej aplikacji został świadomie zaakceptowany podczas doboru stosu.
