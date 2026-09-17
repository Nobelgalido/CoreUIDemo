# CoreUIDemo

A learning replica of **OneMasaito's login and user-account module** — the same architecture, the same classes, the same Angular topology and the same security posture — rebuilt as a standalone ASP.NET MVC 5 project on the `loginDemo` database and themed with the CoreUI Free Bootstrap Admin Template v5.5.0.

## Stack

- .NET Framework 4.7.2 · ASP.NET MVC 5.2.7 · Entity Framework 6.5.1 (Database-First EDMX)
- AngularJS 1.8.2 (one module per page) · angular-growl-v2 0.7.3 · jQuery 3.7.1
- CoreUI Free Bootstrap Admin Template v5.5.0 (Bootstrap 5, CoreUI Icons Free), hand-vendored
- SQL Server — one table, one view, four stored procedures (`Database/script.sql`)

## Prerequisites

- Visual Studio with the *ASP.NET and web development* workload and the *.NET Framework 4.7.2 targeting pack*
- SQL Server (default instance on `localhost`, or adjust the connection string) and SSMS
- Git
- `coreui-free-bootstrap-admin-template-v5.5.0-dist` one folder up (`..\`)
- The OneMasaito project two folders up (`..\..\OneMasaito\OneMasaito\`) — only to copy the angular-growl files, or download them instead

## Quick start

1. Clone, open `CoreUIDemo.slnx` (or the `.csproj`) in Visual Studio, let NuGet restore.
2. In SSMS run `Database/script.sql`, then seed the first admin:
   ```sql
   USE [loginDemo];
   INSERT INTO dbo.USERS_ACCOUNTS (USERNAME, [PASSWORD], FIRST_NAME, LAST_NAME, IS_ACTIVE, [ROLE])
   VALUES ('admin', 'admin123', 'System', 'Administrator', 1, 'admin');
   ```
3. Check the `loginDemoEntities` connection string in `Web.config` (instance name, credentials).
4. Build, press **F5**, log in as `admin` / `admin123`.

If the repo is still in its pre-conversion state (an `AccountController`, `UsersController`, `Services/IUserService.cs` exist), follow `docs/BUILD_GUIDE.md` from § 1 first.

## Documentation

| Document | What it is |
|---|---|
| [`docs/BUILD_GUIDE.md`](docs/BUILD_GUIDE.md) | Step-by-step guide to convert this repo into the OneMasaito mirror: cleanup, database, EDMX, every backend file, CoreUI vendoring, every view and Angular file, end-to-end checks, troubleshooting |
| [`docs/COREUI_GUIDE.md`](docs/COREUI_GUIDE.md) | How the CoreUI v5.5.0 template is used: what to vendor, bundles, layout anatomy, login card, `coreui.*` JS rules, icons, dark mode, Bootstrap 4 → 5 renames |
| [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) | Reference for the finished system: folder map, request flows, endpoints, data model, OneMasaito → CoreUIDemo mapping, known limitations |
| [`docs/superpowers/specs/2026-09-17-onemasaito-mirror-design.md`](docs/superpowers/specs/2026-09-17-onemasaito-mirror-design.md) | The approved design the guides implement |

## Security notice

This project reproduces OneMasaito's security posture on purpose: plaintext passwords, the password inside the auth cookie, no anti-forgery tokens, JSON endpoints with no auth check. Every item is listed in [`docs/ARCHITECTURE.md` §7](docs/ARCHITECTURE.md#7-known-limitations-inherited-by-design). **Do not deploy it anywhere reachable from a network you do not control.**
