# CoreUI Template Handbook — Design

**Date:** 2026-09-19
**Status:** approved in conversation, awaiting written-spec review
**Deliverable:** `docs/coreui/` — nine Markdown files (docs only, no code changes)

---

## 1. Purpose

A reference handbook for **using and customizing the CoreUI Free Bootstrap Admin Template v5.5.0** on the OneMasaito technology stack. It teaches what each of the dist's 46 HTML pages demonstrates, the markup pattern behind it, the JavaScript it needs, and how the same thing is written as a Razor view + AngularJS controller pair that follows OneMasaito's folder and file structure.

The reader has already set up CoreUI in the project (per `docs/COREUI_GUIDE.md`) and wants to understand the template well enough to build new pages with its components and adjust the theme using what the template ships with.

## 2. Target stack (every entry must be compatible with this)

| Layer | Exactly |
|---|---|
| Runtime | .NET Framework **4.7.2**, ASP.NET MVC **5.3.0**, Razor 3.3.0, `System.Web.Optimization` 1.1.3 bundles |
| Server pattern | `Controllers/<Name>Controller.cs`; page action returns `View()`; data actions return `Json(...)` from `POST`; static `Services/*Service.cs` over EF6 6.5.1 |
| Views | `Views/<Controller>/<Page>.cshtml` with `@{ ViewBag.Title = "..."; }`; shared shell in `Views/Shared/_Layout.cshtml`; `Layout = null` only for the login page |
| Client framework | **AngularJS 1.8.2**. Root module `app` in `App/App.js` lists every page module; each page is `App/Controller/<Page>.js` = `angular.module("<page>", ["app"]).controller("<x>Controller", function ($scope, $location, $http, growl) { var vm = this; ... })`; views open with `<div ng-controller="<x>Controller as vm">` and `ng-init="Init()"` |
| Data calls | `$http({ method: "POST", url: "/<Controller>/<Action>", data: {...} }).then(function (data) { ... data.data ... })` |
| Messages | `angular-growl` (`growl.success` / `growl.error`, `<div growl class="fading">` in the layout) |
| jQuery | 3.7.x loaded first in `~/bundles/scripts`; used only for keypress helpers; **never** for Bootstrap plugins (`$(...).modal()` does not exist under CoreUI) |
| Theme | CoreUI v5.5.0 dist, hand-vendored: `Content/build/css/style.css`, `Content/vendor/@coreui/coreui/js/coreui.bundle.min.js`, `Content/vendor/@coreui/icons/{css,fonts}`, `Content/vendor/simplebar/`, `Scripts/js/color-modes.js`, images in `Src/Image/` |
| Bundles | `~/Content/css`, `~/bundles/scripts` (with `Transforms.Clear()`), `~/bundles/angular` — all rendered in `<head>` |
| Modal helpers | Global `ShowModal(id)` / `HideModal(id)` in `App/App.js` wrapping `coreui.Modal.getOrCreateInstance(...)` |
| Assets policy | Only what the dist ships. No npm, no SCSS, no custom CSS overrides, no CoreUI Pro, no DataTables/Font Awesome/moment (mentioned only as "not in the dist") |

Anything in the handbook that would need something outside this table is out of scope and is not written.

## 3. Boundary with existing documents

| Document | Owns | Handbook does |
|---|---|---|
| `docs/COREUI_GUIDE.md` | Installing the template: dist asset inventory, vendoring, bundle rules, `_Layout` and `Login` mapping, `coreui.*` JS API vs jQuery, icon font, `color-modes.js`, BS4→BS5 class renames | **Links** to its sections (`../COREUI_GUIDE.md#…`); never repeats them |
| `docs/BUILD_GUIDE.md` | Building the OneMasaito user-module mirror step by step, all code | Links where a worked example already exists in full |
| `docs/ARCHITECTURE.md` | Runtime structure of CoreUIDemo | Links for "where does a page/controller live" |
| `README.md` | Entry point | Gains one line pointing to `docs/coreui/README.md` (only change outside `docs/coreui/`) |

The handbook adds **no source files**, so the docs-sync Stop hook is not triggered.

## 4. Files

Location: `CoreUIDemo/docs/coreui/`.

| File | Dist sources | Content |
|---|---|---|
| `README.md` | — | What the handbook is; how to read an entry (§5); dist folder map (`index.html`, `blank.html`, `authentication/`, `components/`, `forms/`, `icons/`, `widgets.html`, `charts.html`, `error-pages/`, `css/`, `js/`, `vendors/`, `assets/`); the target stack table (§2, abbreviated); explicit out-of-scope list (§7); table of contents linking the eight chapters |
| `01-layout.md` | `index.html`, `blank.html` | The page shell region by region: `div.sidebar` (`sidebar-header` + `sidebar-brand` full/narrow images, `ul.sidebar-nav[data-coreui="navigation"][data-simplebar]` with `nav-item`, `nav-title`, `nav-group`/`nav-group-toggle`/`nav-group-items`, badges in nav links, `sidebar-footer` + `sidebar-toggler`), `div.wrapper` (`header.header-sticky` with `header-toggler`, `header-nav`, notification/task/message dropdowns, theme dropdown, avatar dropdown, `breadcrumb` row), `div.body > div.container-lg`, `footer.footer`. Which parts CoreUIDemo keeps/drops (link to COREUI_GUIDE §5). Ends with the worked example **"Adding a new page"** (§6.1) |
| `02-authentication.md` | `authentication/login.html`, `register.html`, `reset-password.html`, `check-email.html`, `change-password.html`, `password-changed.html` | The shared centred-card frame (`bg-body-tertiary min-vh-100 d-flex …`, `container` at 32rem, `card p-4`); per page: fields, show-password button, links, the "or / social" block; how each maps to a `Layout = null` view with its own `ng-app` module like `Login.cshtml` (link to COREUI_GUIDE §6); reset-password → `Views/Home/ResetPassword.cshtml` sketch |
| `03-components.md` | `components/*.html` (22 pages) | One entry per page in dist order: accordion, alerts, badge, breadcrumb, button-group, buttons, cards, carousel, chip, collapse, dropdowns, list-group, modals, navs-tabs, pagination, placeholders, popovers, progress, spinners, tables, toasts, tooltips. The **tables + modals** entries together form the worked example **"CRUD grid page"** (§6.2). Popovers/tooltips/toasts entries cover initialising `coreui.<X>` after AngularJS renders (`$timeout` after data load) |
| `04-forms.md` | `forms/*.html` (9 pages) | checks-radios, chip-input, floating-labels, form-control, input-group, layout, range, select, validation. Every control shown with its `ng-model`; validation entry explains the dist's `needs-validation` + `was-validated` classes and how OneMasaito instead validates in the controller with growl (both shown; growl is the project convention) |
| `05-icons.md` | `icons/coreui-icons-free.html`, `-brand.html`, `-flag.html` | Font (`cil-*`, vendored) vs SVG sprite (`svg/free.svg` with `<use>`, not vendored — how it would be); `cib-*` / `cif-*` sets (not vendored; what to copy if wanted); sizing classes `icon icon-sm/lg/xl/xxl`, `nav-icon`; how to look up a name; the icons CoreUIDemo already uses (link to COREUI_GUIDE §8) |
| `06-widgets-charts.md` | `widgets.html`, `charts.html` | Widget card patterns (`card text-white bg-primary`, stat + sparkline, `progress-group`, social/brand cards, `widgets.js` data shapes); charts: what `vendors/chart.js`, `@coreui/chartjs`, `@coreui/utils` provide, the bundle entries needed to enable them (`~/bundles/scripts` additions, in order), and an AngularJS pattern for creating a Chart after `$http` resolves. Marked as *optional vendors* — the dist ships them, so they are in scope |
| `07-error-pages.md` | `error-pages/404.html`, `500.html` | Markup; mapping to `Views/Shared/Error.cshtml` (link to BUILD_GUIDE) and a `Views/Shared/NotFound.cshtml` via `<customErrors>` in `Web.config` — shown as Web.config XML only, no C# |
| `08-customizing.md` | `index.html` variants, `css/style.css` class inventory, `js/color-modes.js` | Only what the template exposes: sidebar variants (`sidebar-dark` vs default light, `sidebar-fixed`, `sidebar-narrow`, `sidebar-narrow-unfoldable`, `sidebar-overlaid`, `sidebar-sm/md/lg/xl`, `sidebar-end`), header variants (`header-sticky`, `header-fixed`, `border-bottom`), color modes (`data-coreui-theme` on `<html>` or any element — e.g. dark sidebar in light page), brand image swap (`Src/Image/`), component colour variants (`btn-*`, `bg-*`, `text-bg-*`, `border-*`, `text-*`), Bootstrap 5 utilities the template relies on (`gap-*`, `d-flex`, `container-*`), footer and breadcrumb toggles, per-page `container-fluid` vs `container-lg`. Explicitly *not*: `--cui-*` overrides, SCSS |

## 5. Entry shape (identical for every page/component)

1. **What the dist page shows** — one paragraph + the dist path to open in a browser.
2. **Essential markup** — the dist's HTML trimmed to the smallest working example; comments on the classes that carry the behaviour.
3. **Behaviour** — the `data-coreui-*` attributes or `coreui.<Component>` calls it needs, or "CSS only".
4. **In CoreUIDemo** — the same thing following OneMasaito's structure: view snippet inside `<div ng-controller="… as vm">`, controller snippet in `App/Controller/<Page>.js`, AngularJS bindings replacing static demo content (`ng-repeat`, `ng-show`, `ng-class`, `ng-model`, `ng-click`), data via `$http` POST, growl for messages, `ShowModal`/`HideModal` for modals.
5. **Gotchas** — only when real (e.g. tooltips after `ng-repeat`; `data-coreui-toggle="modal"` + `ng-click` order; `data-coreui-*` not `data-bs-*`).

Code blocks are labelled with the target file path; quoted dist code is labelled with its dist path. Class names match the dist exactly. Cross-references use relative links to `../COREUI_GUIDE.md#…`, `../BUILD_GUIDE.md#…`.

## 6. Worked examples

### 6.1 Adding a new page (`01-layout.md`)

`blank.html` → a "Reports" page: `Controllers/ReportsController.cs` (`Index()` returning `View()`, `GetReports()` returning `Json`), `Views/Reports/Index.cshtml`, `App/Controller/Reports.js` (`angular.module("reports", ["app"])`), add `"reports"` to the module list in `App/App.js`, add `~/App/Controller/Reports.js` to `~/bundles/angular` in `App_Start/BundleConfig.cs`, add a `nav-item` (or an item inside a `nav-group`) in `_Layout.cshtml` with a `cil-*` icon and optional `ng-show` gate. Ends with a verify checklist (page renders, sidebar item active, JSON call succeeds).

### 6.2 CRUD grid page (`03-components.md`, tables + modals)

OneMasaito `UserAccounts` style: `card` → `table-responsive` → `table table-striped` with header action button and `ng-repeat` rows with per-row action buttons and `ng-show` gates → edit modal bound to `vm.Modal` with `vm.ModalHeader` → `Save()` validating with growl then `$http` POST → `HideModal("…")` and reload. Links to BUILD_GUIDE for the complete real file.

## 7. Out of scope (stated in README)

- Custom CSS, SCSS builds, `--cui-*` variable overrides
- Installing/vendoring the template (→ `COREUI_GUIDE.md`)
- User-module business logic (→ `BUILD_GUIDE.md`)
- Vendors not in the dist: DataTables, Font Awesome, moment, jQuery UI
- CoreUI Pro components, CoreUI for React/Angular/Vue
- ASP.NET Core

## 8. Accuracy rules for writing

- Every class name, `data-coreui-*` attribute and `coreui.*` API is verified by `grep` against the dist (`css/style.css`, `vendors/@coreui/coreui/js/coreui.bundle.min.js`, the HTML page in question). Nothing from memory.
- Every `cil-*` icon named is verified in `vendors/@coreui/icons/css/free.min.css`.
- Every statement "CoreUIDemo does X" is verified against the current repo file.
- AngularJS snippets use only 1.8.2 APIs already used by OneMasaito (`$scope`, `$http`, `$timeout`, `growl`, `ng-*` directives).
- Snippet file paths follow OneMasaito naming exactly (`Views/<Controller>/<Page>.cshtml`, `App/Controller/<Page>.js`).

## 9. Size and order of writing

Approximately 2,500–3,500 lines total. Each file has its own table of contents. Writing order: `README.md`, `01`, `08` (so the layout and variants vocabulary is fixed first), `03`, `04`, `05`, `02`, `06`, `07`, then the one-line link in the repo `README.md`.

## 10. Process

Spec (this file) → user review → implementation plan (`docs/superpowers/plans/2026-09-19-coreui-template-handbook.md`, one task per file) → write files. The user commits; Claude does not.
