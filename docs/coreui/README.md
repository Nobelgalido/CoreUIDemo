# CoreUI Template Handbook

A reference for using and customizing the CoreUI Free Bootstrap Admin Template v5.5.0 on the OneMasaito technology stack: what each of the dist's HTML pages demonstrates, the markup pattern behind it, the JavaScript it needs, and how the same thing is written as a Razor view + AngularJS controller pair that follows OneMasaito's folder and file structure. It is for a reader who has already set up CoreUI in this project (per `../COREUI_GUIDE.md`) and wants to build new pages with the template's components or adjust the theme using only what the template ships with.

## What this handbook is not

- Custom CSS, SCSS builds, `--cui-*` variable overrides — see [`../COREUI_GUIDE.md` § 2](../COREUI_GUIDE.md#2-why-hand-vendored-not-nuget-or-npm) (why the project is hand-vendored, with no build step to hook custom CSS into)
- Installing/vendoring the template — see [`../COREUI_GUIDE.md`](../COREUI_GUIDE.md)
- User-module business logic — see [`../BUILD_GUIDE.md`](../BUILD_GUIDE.md)
- Vendors not in the dist: DataTables, Font Awesome, moment, jQuery UI — see [`../BUILD_GUIDE.md` § 7](../BUILD_GUIDE.md#-7--vendoring-coreui-and-bundles) (bundle list showing what was left out and why)
- CoreUI Pro components, CoreUI for React/Angular/Vue — see [`../BUILD_GUIDE.md` § 0](../BUILD_GUIDE.md#-0--prerequisites) (the dist edition this project uses)
- ASP.NET Core — see [The stack every example fits](#the-stack-every-example-fits) below (this handbook targets .NET Framework/ASP.NET MVC 5 only)

## The stack every example fits

Every snippet in this handbook is written to run in this project's actual stack. If something needs anything outside this table, it is out of scope and is not written.

| Layer | Exactly |
|---|---|
| Runtime | .NET Framework 4.7.2, ASP.NET MVC 5.3.0, Razor, `System.Web.Optimization` bundles rendered in `<head>` |
| Views | `Views/<Controller>/<Page>.cshtml`; shared shell `Views/Shared/_Layout.cshtml`; `Layout = null` only for the login page |
| AngularJS page pattern | `angular.module("<page>", ["app"]).controller("<x>Controller", function ($scope, $location, $http, growl) { var vm = this; ... })`; views open with `<div ng-controller="<x>Controller as vm">` + `ng-init="Init()"` |
| Data calls | `$http({ method: "POST", url: "/<Controller>/<Action>", data: {...} })` |
| Messages | `growl.success` / `growl.error` |
| Modals | global `ShowModal(id)` / `HideModal(id)` (defined in `App/App.js`, wrapping `coreui.Modal.getOrCreateInstance`) |

jQuery is loaded but has no caller left — its only users, the two letters-only keypress helpers in `UserAccounts.js`, were commented out on 2026-09-22. Never use it for Bootstrap plugins (`$(...).modal()` does not exist under CoreUI).

## Map of the dist

Every top-level item in `coreui-free-bootstrap-admin-template-v5.5.0-dist/`, and which chapter covers it:

| Dist item | What it is | Covered in |
|---|---|---|
| `index.html` | Full dashboard page showing the complete sidebar/header/body shell | [01-layout.md](./01-layout.md) |
| `blank.html` | Empty content-area shell — the starting point for a new page | [01-layout.md](./01-layout.md) |
| `authentication/` | Login, register, reset-password, check-email, change-password, password-changed pages | [02-authentication.md](./02-authentication.md) |
| `components/` | One demo page per Bootstrap/CoreUI component (accordion through tooltips) | [03-components.md](./03-components.md) |
| `forms/` | Form control demo pages (checks-radios, chip-input, floating-labels, form-control, input-group, layout, range, select, validation) | [04-forms.md](./04-forms.md) |
| `icons/` | Icon catalogue pages: free, brand, flag | [05-icons.md](./05-icons.md) |
| `widgets.html` | Stat/card widget examples | [06-widgets-charts.md](./06-widgets-charts.md) |
| `charts.html` | Chart.js chart examples | [06-widgets-charts.md](./06-widgets-charts.md) |
| `error-pages/` | 404 and 500 templates | [07-error-pages.md](./07-error-pages.md) |
| `css/` | Compiled `style.css` and vendor CSS | assets, see [`../COREUI_GUIDE.md` § 1](../COREUI_GUIDE.md#1-what-the-dist-contains-and-what-we-use) |
| `js/` | Compiled `coreui.bundle.min.js` and page scripts | assets, see [`../COREUI_GUIDE.md` § 1](../COREUI_GUIDE.md#1-what-the-dist-contains-and-what-we-use) |
| `vendors/` | Third-party libraries the template ships (Chart.js, simplebar, `@coreui/icons`, etc.) | assets, see [`../COREUI_GUIDE.md` § 1](../COREUI_GUIDE.md#1-what-the-dist-contains-and-what-we-use) |
| `assets/` | Images used by the dist demo pages | assets, see [`../COREUI_GUIDE.md` § 1](../COREUI_GUIDE.md#1-what-the-dist-contains-and-what-we-use) |

## How to read an entry

Every page/component entry in chapters 01–08 follows the same five-part shape:

1. **What the dist page shows** — one paragraph plus the dist path to open in a browser.
2. **Essential markup** — the dist's HTML trimmed to the smallest working example, with comments on the classes that carry the behaviour.
3. **Behaviour** — the `data-coreui-*` attributes or `coreui.<Component>` calls it needs, or "CSS only".
4. **In CoreUIDemo** — the same thing written OneMasaito's way: a Razor view snippet inside `<div ng-controller="… as vm">` and a controller snippet in `App/Controller/<Page>.js`, with `ng-*` bindings (`ng-repeat`, `ng-show`, `ng-class`, `ng-model`, `ng-click`) replacing the static demo content, data via `$http` POST, `growl` for messages, `ShowModal`/`HideModal` for modals.
5. **Gotchas** — only included when real (for example: initializing tooltips after `ng-repeat`; `data-coreui-*` not `data-bs-*`).

Code blocks are labelled with the target file path on the line above, e.g. **`Views/Reports/Index.cshtml`**; quoted dist code is labelled with its dist path, e.g. **`components/tables.html`**.

## Chapters

1. [01-layout.md](./01-layout.md) — The sidebar, header and body shell (`index.html`, `blank.html`); ends with the worked example "Adding a new page".
2. [02-authentication.md](./02-authentication.md) — Login, register, reset/change-password pages and their `Layout = null` views.
3. [03-components.md](./03-components.md) — One entry per component demo page; the tables + modals entries form the worked "CRUD grid page" example.
4. [04-forms.md](./04-forms.md) — Form controls with their `ng-model` bindings, plus the dist's client-side validation vs OneMasaito's growl convention.
5. [05-icons.md](./05-icons.md) — The `cil-*`/`cib-*`/`cif-*` icon sets, sizing classes, and how to look up an icon.
6. [06-widgets-charts.md](./06-widgets-charts.md) — Stat/card widgets and Chart.js charts, with the bundle entries needed to enable them.
7. [07-error-pages.md](./07-error-pages.md) — 404/500 markup and the `Web.config` mapping.
8. [08-customizing.md](./08-customizing.md) — Sidebar/header variants, colour modes, and the component colour utilities the dist ships.

## Working with the dist while you build

- Open the dist HTML page you're working from in a browser, side by side with Visual Studio, so you can see the live component while you port its markup.
- Use the browser's DevTools "Copy element" on the piece you need, then port it with [`../COREUI_GUIDE.md` § 10](../COREUI_GUIDE.md#10-bootstrap-4--bootstrap-5-class-renames)'s rename table when the source you're comparing against is OneMasaito's older Bootstrap 4 markup.
- Use `icons/coreui-icons-free.html` as the icon catalogue — it lists every free `cil-*` icon with its class name.
