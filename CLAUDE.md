# CoreUIDemo — project instructions

This repo is a strict mirror of OneMasaito's login + user-account module on the `loginDemo` schema with the CoreUI v5.5.0 theme. The design is in `docs/superpowers/specs/2026-09-17-onemasaito-mirror-design.md`; do not add structure OneMasaito does not have (no DI, no interfaces, no ViewModels, no anti-forgery, no `[Authorize]`) unless the user asks.

## Documentation must stay in sync with the code

The four documents below are the source of truth and must be updated **in the same turn** as any change to `Controllers/`, `Services/`, `Helpers/`, `App_Start/`, `App/`, `Views/`, `Models/*.cs` (hand-written), `Database/`, `Content/Site.css`, `Web.config`, `Global.asax.cs`, `packages.config` or the csproj:

| Doc | Update when |
|---|---|
| `docs/BUILD_GUIDE.md` | Any file it lists changes — its code blocks are complete files; replace the block verbatim. Endpoint/message changes also update § 10's expected strings and Appendix A. |
| `docs/ARCHITECTURE.md` | Folder map, request flows, endpoint table, data model/sprocs, mapping tables, known limitations. |
| `docs/COREUI_GUIDE.md` | Theme vendoring, bundles, layout anatomy, JS API rules, icons, dark mode. |
| `README.md` | Stack, prerequisites, quick start, doc index. |

A `Stop` hook (`.claude/hooks/docs-sync.sh`) blocks the end of a turn if source files changed this session and none of these docs did. When that fires, reconcile the docs, or state explicitly why no doc change is needed and add a one-line note to the relevant doc.

## Git: the user commits, not Claude

- **Never run `git commit`, `git push`, `git add`, `git reset` or any history-changing command** in this repo unless the user explicitly asks for that exact action in the current message. This includes commits that a skill or plan says to make (design specs, plan files, checkpoints).
- When work reaches a checkpoint named in `docs/BUILD_GUIDE.md`, stop and tell the user: which files changed, and the checkpoint's commit message, so they can commit it themselves.
- Read-only git (`status`, `diff`, `log`) is fine.

## Conventions

- Mirror OneMasaito's idioms: static services, `out string message`, sequential `if` validation with growl, `UniversalHelpers.CurrentUser` checks on GET view actions only.
