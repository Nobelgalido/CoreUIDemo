# CoreUI Template Handbook Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Write `docs/coreui/` — a nine-file handbook teaching how to use and customize the CoreUI Free Bootstrap Admin Template v5.5.0 on the OneMasaito stack (ASP.NET MVC 5 + Razor + AngularJS 1.8.2).

**Architecture:** Documentation only. One Markdown file per dist area, every page/component written in the same five-part entry shape (dist page → essential markup → behaviour → in CoreUIDemo → gotchas). Facts are taken from the dist files by `grep`, never from memory; cross-references link to `docs/COREUI_GUIDE.md` / `docs/BUILD_GUIDE.md` instead of repeating them.

**Tech Stack:** Markdown; verification by `grep`/`bash` against the dist and the repo. No source files change.

**Spec:** `docs/superpowers/specs/2026-09-19-coreui-template-handbook-design.md`

## Global Constraints

- Repo: `C:\Users\monst\OneDrive\Desktop\WORK\CoreUIDemo\CoreUIDemo` (git, branch `main`). Dist: `C:\Users\monst\OneDrive\Desktop\WORK\CoreUIDemo\coreui-free-bootstrap-admin-template-v5.5.0-dist`. OneMasaito reference: `C:\Users\monst\OneDrive\Desktop\WORK\OneMasaito\OneMasaito`.
- Output folder: `docs/coreui/`. Files: `README.md`, `01-layout.md`, `02-authentication.md`, `03-components.md`, `04-forms.md`, `05-icons.md`, `06-widgets-charts.md`, `07-error-pages.md`, `08-customizing.md`.
- Stack every snippet must fit: .NET Framework 4.7.2, ASP.NET MVC 5.3.0, Razor, `System.Web.Optimization` bundles rendered in `<head>`, AngularJS 1.8.2 with `angular.module("<page>", ["app"]).controller("<x>Controller", function ($scope, $location, $http, growl) { var vm = this; ... })`, views open with `<div ng-controller="<x>Controller as vm">` + `ng-init="Init()"`, data via `$http({ method: "POST", url: "/<Controller>/<Action>", data: {...} })`, messages via `growl.success` / `growl.error`, modals via global `ShowModal(id)` / `HideModal(id)` (defined in `App/App.js`, wrapping `coreui.Modal.getOrCreateInstance`). jQuery only for keypress helpers.
- Assets policy: only what the dist ships. **No** custom CSS, SCSS, `--cui-*` overrides, npm, CoreUI Pro, DataTables, Font Awesome, moment.
- JS API: `coreui.<Component>` and `data-coreui-*` attributes. Never `bootstrap.*`, `data-bs-*`, or `$(...).modal()`.
- Every class name, `data-coreui-*` attribute, `coreui.*` component name and `cil-*` icon written into the handbook must be confirmed present in the dist by the check script below before the task is marked done.
- Every relative link in the handbook must resolve to an existing file (anchors to `COREUI_GUIDE.md` must match an existing heading).
- Entry shape (identical everywhere): **1. What the dist page shows** (paragraph + dist path) → **2. Essential markup** (trimmed dist HTML, annotated) → **3. Behaviour** (`data-coreui-*` / `coreui.*` / "CSS only") → **4. In CoreUIDemo** (Razor view + `App/Controller/<Page>.js` snippets with `ng-*` bindings) → **5. Gotchas** (only when real).
- Code fences are labelled with the target path on the line above, e.g. `**`Views/Reports/Index.cshtml`**`; quoted dist code is labelled with its dist path.
- Facts already established (do not re-derive): dist `css/style.css` sidebar classes = `sidebar`, `sidebar-backdrop`, `sidebar-body`, `sidebar-brand`(`-full`/`-narrow`), `sidebar-dark`, `sidebar-end`, `sidebar-fixed`, `sidebar-footer`, `sidebar-form`, `sidebar-header`, `sidebar-narrow`, `sidebar-narrow-unfoldable`, `sidebar-nav` (+`.compact`), `sidebar-overlaid`, `sidebar-sm`/`-lg`/`-xl`, `sidebar-toggler`, `sidebar.hide`. Header classes = `header`, `header-brand`, `header-divider`, `header-nav`, `header-sticky`, `header-text`, `header-toggler`, `header-toggler-icon` (there is **no** `header-fixed`). JS components in `coreui.bundle.min.js` = alert, button, carousel, chip, chip-input, collapse, dropdown, modal, navigation, offcanvas, popover, scrollspy, sidebar, tab, toast, tooltip. `free.min.css` defines 562 `cil-*` icons. `vendors/@coreui/icons/svg/` contains **only** `flag/cif-*.svg` files — there is no free/brand SVG sprite in this dist; free and brand icons are font-only.
- The user commits. **Never run `git add`/`git commit`.** Each task ends with a "Checkpoint" step that tells the user what is ready to commit.

## The check script

Save once per session to the scratchpad directory as `check-handbook.sh` and run after writing each file. It fails (exit 1) on any unknown class / icon / attribute / broken link.

```bash
#!/usr/bin/env bash
# usage: check-handbook.sh <handbook-file.md>
set -u
REPO="C:/Users/monst/OneDrive/Desktop/WORK/CoreUIDemo/CoreUIDemo"
DIST="C:/Users/monst/OneDrive/Desktop/WORK/CoreUIDemo/coreui-free-bootstrap-admin-template-v5.5.0-dist"
F="$1"; fail=0

# 1. cil-* icons must exist in free.min.css
for ic in $(grep -oE 'cil-[a-z0-9-]+' "$F" | sort -u); do
  grep -q "\.$ic:" "$DIST/vendors/@coreui/icons/css/free.min.css" || { echo "UNKNOWN ICON: $ic"; fail=1; }
done

# 2. data-coreui-* attributes must appear somewhere in the dist HTML
for at in $(grep -oE 'data-coreui(-[a-z]+)*' "$F" | sort -u); do
  grep -rqE "$at[= ]" "$DIST" --include='*.html' || { echo "UNKNOWN ATTR: $at"; fail=1; }
done

# 3. coreui.<Component> names must be JS components in the bundle
for c in $(grep -oE 'coreui\.[A-Z][A-Za-z]+' "$F" | sed 's/coreui\.//' | sort -u); do
  # vendor globals set by the UMD wrappers in vendors/@coreui/{utils,chartjs}, not bundle components
  case "$c" in Utils|ChartJS) continue;; esac
  n=$(echo "$c" | sed 's/\([A-Z]\)/-\L\1/g; s/^-//')
  grep -q "return\"$n\"\|=\"$n\",[A-Za-z]*=\".coreui.$n\"" "$DIST/vendors/@coreui/coreui/js/coreui.bundle.min.js" || { echo "UNKNOWN JS COMPONENT: coreui.$c"; fail=1; }
done

# 4. sidebar-*/header-*/nav-*/card-*/btn-*/form-*/table-*/modal-*/dropdown-*/badge/alert-*/toast-*/list-group-* classes must exist in style.css
for cl in $(sed -E 's/\(#[^)]*\)//g' "$F" | grep -oE '\b(sidebar|header|nav|card|btn|form|table|modal|dropdown|alert|toast|list-group|input-group|accordion|breadcrumb|pagination|progress|spinner|placeholder|carousel|chip|popover|tooltip|widget|progress-group|avatar|icon)(-[a-z0-9]+)*\b' | sort -u); do
  grep -q "\.$cl[ ,:.{>)]" "$DIST/css/style.css" || grep -q "\.$cl\$" "$DIST/css/style.css" || { echo "UNKNOWN CLASS: $cl"; fail=1; }
done

# 5. relative links must resolve
dir=$(dirname "$F")
for l in $(grep -oE '\]\(([^)#:]+\.md)' "$F" | sed 's/](//' | sort -u); do
  [ -e "$dir/$l" ] || { echo "BROKEN LINK: $l"; fail=1; }
done

[ $fail -eq 0 ] && echo "OK: $F"
exit $fail
```

Known false positives to expect and ignore after eyeballing: generic words caught by rule 4 that are not classes (e.g. `icon` inside prose like "icon font", `card` in "card pattern"). Everything else must be fixed in the handbook text or confirmed by a manual `grep` on `style.css` and noted in the task's checkpoint.

---

### Task 1: `docs/coreui/README.md` (index) + check script

**Files:**
- Create: `docs/coreui/README.md`
- Create (scratchpad, not repo): `check-handbook.sh` from the Global Constraints section

**Interfaces:**
- Produces: the chapter file names `01-layout.md` … `08-customizing.md` that every other task links back to via `[…](README.md)`; the "How to read an entry" section that every chapter assumes.

- [ ] **Step 1: Save the check script**

Write the script from "The check script" above to `<scratchpad>/check-handbook.sh`. Run `bash <scratchpad>/check-handbook.sh docs/COREUI_GUIDE.md` from the repo root.
Expected: it prints either `OK: docs/COREUI_GUIDE.md` or a short list of `UNKNOWN CLASS:` lines for prose words only (e.g. `icon`). Any `UNKNOWN ICON` or `UNKNOWN ATTR` means the script is broken — fix it before continuing.

- [ ] **Step 2: Confirm the dist folder map**

Run from the dist root: `find . -maxdepth 1 | sort; find authentication components forms icons error-pages -name '*.html' | sort | wc -l`
Expected: top level = `assets/ authentication/ blank.html charts.html components/ css/ error-pages/ forms/ icons/ index.html js/ vendors/ widgets.html`; 42 HTML files in the subfolders (+ 4 at top level = 46).

- [ ] **Step 3: Write `docs/coreui/README.md`** with exactly these sections:

1. `# CoreUI Template Handbook` — two-sentence purpose (from spec §1) and who it is for.
2. `## What this handbook is not` — the spec §7 bullet list verbatim, each with the link to where it *is* covered (`../COREUI_GUIDE.md`, `../BUILD_GUIDE.md`).
3. `## The stack every example fits` — spec §2 table condensed to 6 rows: Runtime, Views, AngularJS page pattern (with the one-line `angular.module(...)` shape), Data calls, Messages, Modals.
4. `## Map of the dist` — a table of every top-level item of the dist with one line each and which chapter covers it (`index.html`/`blank.html` → 01; `authentication/` → 02; `components/` → 03; `forms/` → 04; `icons/` → 05; `widgets.html`/`charts.html` → 06; `error-pages/` → 07; `css/`, `js/`, `vendors/`, `assets/` → "assets, see COREUI_GUIDE §1").
5. `## How to read an entry` — the five-part shape from Global Constraints, one line each, plus the code-fence labelling rule.
6. `## Chapters` — numbered links to the eight files with one-line descriptions.
7. `## Working with the dist while you build` — three tips: open the dist HTML in a browser next to Visual Studio; use browser DevTools "Copy element" then port with COREUI_GUIDE §10's rename table when the source is OneMasaito; use `icons/coreui-icons-free.html` as the icon catalogue.

- [ ] **Step 4: Verify**

Run: `bash <scratchpad>/check-handbook.sh docs/coreui/README.md`
Expected: `OK` (links to `01-…08-` files will be BROKEN until later tasks — that is expected only in this task; re-run after Task 9).

- [ ] **Step 5: Checkpoint**

Tell the user: `docs/coreui/README.md` is ready; chapter links will resolve as chapters land.

---

### Task 2: `docs/coreui/01-layout.md`

**Files:**
- Create: `docs/coreui/01-layout.md`
- Read: dist `index.html` (lines 1–~200 shell, sidebar and header; the rest is dashboard content), dist `blank.html`, repo `Views/Shared/_Layout.cshtml`, repo `App/App.js`, repo `App_Start/BundleConfig.cs`, repo `Controllers/HomeController.cs` (for the `Index()` shape)

**Interfaces:**
- Produces: the "Adding a new page" recipe (§6.1 of the spec) with the exact names `ReportsController`, `Views/Reports/Index.cshtml`, `App/Controller/Reports.js`, module `"reports"`, controller `reportsController`, that Task 4's CRUD example and Task 7's auth pages refer to.

- [ ] **Step 1: Extract the shell**

Run from dist root: `grep -nE '<(div|header|footer|ul|nav|main) class="(sidebar|wrapper|header|body|footer|sidebar-nav|breadcrumb|container)[^"]*"' index.html blank.html`
Expected: line numbers for `div.sidebar`, `ul.sidebar-nav`, `div.wrapper`, `header.header`, `div.body`, `footer.footer` in both files. Read those regions.

- [ ] **Step 2: Confirm the JS calls the dist uses for the sidebar**

Run: `grep -n 'coreui.Sidebar\|data-coreui-toggle="unfoldable"\|data-coreui="navigation"\|data-simplebar' index.html | head`
Expected: `onclick="coreui.Sidebar.getInstance(document.querySelector('#sidebar')).toggle()"` on the header toggler and the mobile close button; `data-coreui-toggle="unfoldable"` on `button.sidebar-toggler`; `data-coreui="navigation" data-simplebar` on `ul.sidebar-nav`.

- [ ] **Step 3: Write `01-layout.md`** with these sections (each a five-part entry where it is a component):

1. `# 01 · Layout` + TOC.
2. `## The shell at a glance` — an ASCII tree of `body > div.sidebar + div.wrapper(header, div.body, footer)` and the sentence: in CoreUIDemo the whole shell is wrapped by `<div ng-controller="mainController as main" ng-init="Init()">` (link `../COREUI_GUIDE.md#5-layout-anatomy--indexhtml--_layoutcshtml`).
3. `## Sidebar` — entries for: `sidebar-header` + `sidebar-brand` (full/narrow `<img>`; note `.sidebar-brand-full` / `.sidebar-brand-narrow` visibility rules), `nav-item` link (with `nav-icon` and optional `badge`), `nav-title`, `nav-group` (toggle + `nav-group-items`, the `compact` variant, nesting one level), mobile `btn-close`, `sidebar-footer` + `sidebar-toggler`. In CoreUIDemo: `ng-show` gating on `main.CurrentUser.<Field>` exactly as `_Layout.cshtml` does, and the "active item" rule (the dist marks `a.nav-link.active` by hand — show an `ng-class="{active: main.Path === '/Reports/Index'}"` pattern using `$location.path()` is **not** available under MVC routing without html5Mode, so use `window.location.pathname` set in `mainController.Init` as `main.Path`).
4. `## Header` — entries for: `header-toggler`, `header-nav` links with `icon icon-lg`, the notification/tasks/messages dropdowns from `index.html` (marked "demo — replace or drop"), the theme dropdown (link `../COREUI_GUIDE.md#9-dark-mode`), the avatar dropdown, `div.vr` divider, the breadcrumb row (`container-fluid px-4` + `nav[aria-label=breadcrumb] > ol.breadcrumb.my-0`). In CoreUIDemo: the search `input-group` bound to `main.SearchBox` that OneMasaito has instead of the icon links.
5. `## Body and footer` — `div.body.flex-grow-1 > div.container-lg.px-4` vs CoreUIDemo's `container-fluid px-4` around `@RenderBody()`; `footer.footer.px-4` content.
6. `## blank.html` — what it is (the shell with an empty body) and why every new page starts from it.
7. `## Worked example — adding a "Reports" page` — complete code, in this order:
   - **`Controllers/ReportsController.cs`**: `public class ReportsController : Controller { public ActionResult Index() { return View(); } [HttpPost] public JsonResult GetReports() { return Json(new { reportList = new[] { new { Id = 1, Name = "Monthly" } } }); } }` (with `using System.Web.Mvc;`).
   - **`Views/Reports/Index.cshtml`**: `@{ ViewBag.Title = "Reports"; }` then `<div ng-controller="reportsController as vm" ng-init="Init()">` → `<div class="card mb-4"><div class="card-header">Reports</div><div class="card-body"><ul class="list-group"><li class="list-group-item" ng-repeat="r in vm.ReportList">{{r.Name}}</li></ul></div></div></div>`.
   - **`App/Controller/Reports.js`**: `angular.module("reports", ["app"]).controller("reportsController", function ($scope, $location, $http, growl) { var vm = this; vm.ReportList = []; $scope.Init = function () { $http({ method: "POST", url: "/Reports/GetReports" }).then(function (data) { vm.ReportList = data.data.reportList; }); }; });`
   - **`App/App.js`**: add `"reports"` to the dependency array of `angular.module('app', [...])`.
   - **`App_Start/BundleConfig.cs`**: add `"~/App/Controller/Reports.js"` to the `~/bundles/angular` `Include(...)`.
   - **`Views/Shared/_Layout.cshtml`**: add `<li class="nav-item"><a class="nav-link" href="/Reports/Index"><i class="nav-icon cil-chart"></i> Reports</a></li>` after the Dashboard item (verify `cil-chart` exists — if not, use `cil-bar-chart`).
   - Verify checklist: build succeeds; `/Reports/Index` renders inside the shell; sidebar shows the item; the Network tab shows `POST /Reports/GetReports` → 200 with `reportList`; no console errors.
8. `## Gotchas` — (a) a bundle virtual path must not be a real folder (link COREUI_GUIDE §4 rule 1); (b) `data-coreui="navigation"` wires `nav-group` toggles — do not add `data-coreui-toggle="collapse"` to them; (c) `ng-show` on a `nav-group` hides the whole group, on a `nav-item` only that item.

- [ ] **Step 4: Verify**

Run: `bash <scratchpad>/check-handbook.sh docs/coreui/01-layout.md`
Expected: `OK` (or only prose false positives).
Also run: `grep -c '' docs/coreui/01-layout.md` — expected 350–550 lines.

- [ ] **Step 5: Checkpoint** — tell the user `01-layout.md` is ready to commit.

---

### Task 3: `docs/coreui/08-customizing.md`

**Files:**
- Create: `docs/coreui/08-customizing.md`
- Read: dist `index.html` (`<html>` and `div.sidebar` opening tags), dist `js/color-modes.js`, dist `css/style.css` (grep only), repo `Views/Shared/_Layout.cshtml`, repo `Src/Image/`

**Interfaces:**
- Consumes: sidebar/header vocabulary from Task 2.
- Produces: the variant class table that Task 2's entries and README refer to as "see 08".

- [ ] **Step 1: Confirm each variant class has rules**

Run from dist root, for each of `sidebar-narrow sidebar-narrow-unfoldable sidebar-overlaid sidebar-fixed sidebar-end sidebar-sm sidebar-lg sidebar-xl sidebar-dark header-sticky`: `grep -c "\.$cls" css/style.css`
Expected: every count > 0. Also run `grep -o 'data-coreui-theme="[a-z]*"' index.html | sort | uniq -c` — expected `dark` on `div.sidebar`-related elements and on the mobile `btn-close`.

- [ ] **Step 2: Confirm the colour-mode mechanics**

Run: `grep -n "localStorage\|data-coreui-theme\|prefers-color-scheme" js/color-modes.js`
Expected: storage key `coreui-free-bootstrap-admin-template-theme`; attribute set on `document.documentElement`; `auto` follows `prefers-color-scheme`.
Run: `grep -c '\[data-coreui-theme=dark\]' css/style.css` — expected > 0 (this is how per-element dark theming works).

- [ ] **Step 3: Write `08-customizing.md`** with these sections:

1. `# 08 · Customizing with what the template ships` + TOC + a boxed note: "No custom CSS, no SCSS, no `--cui-*` overrides in this project (decision 2026-09-19). Everything below is a class or attribute the dist already provides."
2. `## Sidebar variants` — table: class → effect → how to apply in `_Layout.cshtml` (`<div class="sidebar sidebar-dark sidebar-fixed …">`) for: default (light) vs `sidebar-dark`; `sidebar-fixed`; `sidebar-narrow`; `sidebar-narrow-unfoldable` (and that `button.sidebar-toggler[data-coreui-toggle="unfoldable"]` toggles it); `sidebar-overlaid` (+ `.show`, backdrop); `sidebar-end`; `sidebar-sm` / `sidebar-lg` / `sidebar-xl`; `sidebar-nav.compact`. State which combination CoreUIDemo uses today (`sidebar sidebar-dark sidebar-fixed border-end`).
3. `## Header variants` — `header-sticky` (the only positioning variant), `border-bottom` on the inner container, `header-brand`, `header-divider`, `header-text`, `header-toggler-icon`; one-line each.
4. `## Colour modes` — `data-coreui-theme` on `<html>` (global, driven by `color-modes.js`) vs on any element (local: e.g. `data-coreui-theme="dark"` on `div.sidebar` for a dark sidebar in a light page — show the exact markup); `light` / `dark` / `auto`; how to remove switching (link COREUI_GUIDE §9); which utility classes follow the theme (`bg-body`, `bg-body-tertiary`, `text-body`, `text-body-secondary`, `border`) and which do not (`bg-white`, `text-dark`).
5. `## Brand and favicon` — swap images in `Src/Image/`, the two `<img>` tags (`sidebar-brand-full` / `sidebar-brand-narrow`), the `<link rel="icon">`; what CoreUIDemo currently uses (list the four `masaito-*.svg` files present in `Src/Image/` — verify with `ls Src/Image`).
6. `## Component colour variants` — the eight theme colours (`primary secondary success danger warning info light dark`) and where they apply: `btn-*`, `btn-outline-*`, `bg-*`, `text-bg-*`, `text-*`, `border-*`, `alert-*`, `badge` + `text-bg-*`, `list-group-item-*`, `table-*`; plus `btn-ghost-*` (CoreUI-only — verify `grep -c '\.btn-ghost-primary' css/style.css` > 0). Show one `ng-class` example choosing a badge colour from data: `<span class="badge" ng-class="acc.Status === 1 ? 'text-bg-success' : 'text-bg-secondary'">`.
7. `## Page width` — `container-lg` (dist) vs `container-fluid` (CoreUIDemo) vs `container-xl`; per-page override by wrapping the view's content instead of changing `_Layout`.
8. `## Spacing and layout utilities the template leans on` — `d-flex`, `gap-*`, `mb-4` between cards, `row g-3`, `card-header d-flex justify-content-between align-items-center` (from the dist), one example each.
9. `## Turning things off` — breadcrumb row, footer, theme dropdown (why it cannot just be deleted → COREUI_GUIDE §9), sidebar footer toggler.

- [ ] **Step 4: Verify** — `bash <scratchpad>/check-handbook.sh docs/coreui/08-customizing.md`; expected `OK`. Length 250–400 lines.

- [ ] **Step 5: Checkpoint** — `08-customizing.md` ready.

---

### Task 4: `docs/coreui/03-components.md`

**Files:**
- Create: `docs/coreui/03-components.md`
- Read: dist `components/*.html` (22 files), repo `Views/Settings/UserAccounts.cshtml`, repo `App/Controller/UserAccounts.js`, repo `App/App.js` (for `ShowModal`/`HideModal`)

**Interfaces:**
- Consumes: `ShowModal(id)` / `HideModal(id)` from `App/App.js` (verify they exist: `grep -n "ShowModal\|HideModal" App/App.js`).
- Produces: the CRUD grid worked example (§6.2) that README links as the main example.

- [ ] **Step 1: Per page, extract the first example**

For each of the 22 files run: `awk '/<div class="example">/{c++} c==1' components/<name>.html | head -60` — the dist wraps every demo in `div.example`; the first one is the canonical form.
Expected: a short HTML block per page. Also `grep -o 'data-coreui-[a-z-]*' components/<name>.html | sort -u` to list the attributes that page uses.

- [ ] **Step 2: Confirm which components need JS vs CSS only**

From the JS component list in Global Constraints: JS-backed = accordion (collapse), alerts (dismiss), button-group (button toggle), carousel, chip, collapse, dropdowns, modals, navs-tabs (tab), popovers, toasts, tooltips. CSS-only = badge, breadcrumb, buttons (plain), cards, list-group, pagination, placeholders, progress, spinners, tables. Double-check `chip`: `grep -c 'data-coreui-toggle="chip"\|class="chip' components/chip.html` > 0.

- [ ] **Step 3: Write `03-components.md`**

`# 03 · Components` + TOC listing all 22 in dist order, then one five-part entry per page. Required specifics:

- **accordion**: `div.accordion > div.accordion-item > h2.accordion-header > button.accordion-button[data-coreui-toggle=collapse][data-coreui-target=#id]` + `div.accordion-collapse.collapse[data-coreui-parent=#accordion]`; `accordion-flush`; AngularJS: generate items with `ng-repeat` and `id="item{{$index}}"` / `data-coreui-target="#item{{$index}}"` — interpolation inside attribute values works because AngularJS compiles before the user clicks.
- **alerts**: `alert alert-<colour>` + `alert-dismissible fade show` + `button.btn-close[data-coreui-dismiss=alert]`; note OneMasaito uses growl for transient messages, alerts for persistent page-level notes (`ng-show="vm.Warning"`).
- **badge**: `badge text-bg-<colour>`, `rounded-pill`, positioned badge on a button; `ng-class` example (same as 08 §6).
- **breadcrumb**: `nav[aria-label=breadcrumb] > ol.breadcrumb > li.breadcrumb-item(.active)`; where it goes in the header (link 01).
- **button-group**: `btn-group`, `btn-group-vertical`, `btn-toolbar`, checkbox/radio toggle groups with `btn-check` + `ng-model`.
- **buttons**: sizes, `btn-outline-*`, `btn-ghost-*`, `disabled`, `ng-disabled="vm.Saving"`; the OneMasaito icon-button pattern `<button class="btn btn-info" ng-click="EditAccount(acc)"><i class="cil-pencil"></i></button>`.
- **cards**: `card > card-header/card-body/card-footer`, `card-title`, `card-text`, `text-bg-*` cards, `card-group`, grid of cards with `row row-cols-1 row-cols-md-3 g-4` + `ng-repeat`.
- **carousel**: `carousel slide[data-coreui-ride=carousel]` + `carousel-inner > carousel-item.active`, controls with `data-coreui-slide`; AngularJS caveat: the first `carousel-item` must have `active` — use `ng-class="{active: $first}"`.
- **chip**: `div.chip` markup and `data-coreui-toggle="chip"` as in the dist; `coreui.Chip`; note it is a CoreUI-only component (not Bootstrap).
- **collapse**: `data-coreui-toggle=collapse data-coreui-target=#id` + `div.collapse`; prefer `ng-show` when the state lives in the controller — show both and say when each applies.
- **dropdowns**: `dropdown > button[data-coreui-toggle=dropdown] + ul.dropdown-menu(.dropdown-menu-end) > li > a.dropdown-item`; `ng-repeat` items with `ng-click`; `dropdown-header`, `dropdown-divider`.
- **list-group**: `list-group > list-group-item(-action)(.active)`, badges in items, `ng-repeat` + `ng-class="{active: item === vm.Selected}"`.
- **modals**: `modal fade[id][tabindex=-1] > modal-dialog(.modal-lg/.modal-dialog-centered/.modal-dialog-scrollable) > modal-content > modal-header (h5.modal-title + btn-close[data-coreui-dismiss=modal]) / modal-body / modal-footer`; opening from markup (`data-coreui-toggle=modal data-coreui-target=#id`) vs from the controller (`ShowModal("AccountModal")`); `data-coreui-backdrop="static"`; `data-coreui-keyboard="false"`. Gotcha: `$(...).modal()` throws (link COREUI_GUIDE §7).
- **navs-tabs**: `ul.nav.nav-tabs > li.nav-item > button.nav-link[data-coreui-toggle=tab][data-coreui-target=#pane]` + `div.tab-content > div.tab-pane.fade(.show.active)`; `nav-pills`, `nav-underline`; AngularJS: keep `data-coreui-toggle` for the visual switch, or replace with `ng-click="vm.Tab='a'"` + `ng-class` + `ng-show` when panes need controller state — show both.
- **pagination**: `ul.pagination > li.page-item(.active/.disabled) > a.page-link`; client-side paging with `ng-repeat="p in vm.Pages"` and `limitTo` filter: `ng-repeat="acc in vm.AccountList | limitTo: vm.PageSize : (vm.Page-1)*vm.PageSize"`.
- **placeholders**: `placeholder col-*`, `placeholder-glow` / `placeholder-wave`; use with `ng-show="!vm.Loaded"` instead of OneMasaito's `.loader`.
- **popovers**: `data-coreui-toggle=popover data-coreui-content=… data-coreui-placement=…`; must be initialised: `new coreui.Popover(el)` — show the `$timeout(function(){ document.querySelectorAll('[data-coreui-toggle="popover"]').forEach(function(el){ coreui.Popover.getOrCreateInstance(el); }); })` pattern after data loads (`$timeout` injected in the controller signature).
- **progress**: `div.progress > div.progress-bar` with `style="width: {{vm.Percent}}%"`, `progress-bar-striped progress-bar-animated`, `progress-thin` (verify in style.css), `progress-group` from widgets (link 06).
- **spinners**: `spinner-border` / `spinner-grow`, sizes, inside buttons with `ng-show="vm.Saving"`.
- **tables**: `table`, `table-striped`, `table-hover`, `table-bordered`, `table-sm`, `table-responsive`, `table-<colour>` rows, `align-middle`, `table-group-divider`; the OneMasaito grid shape with `ng-repeat` and the `filter: main.SearchBox` pipe (header search box from 01). Then the **worked example "CRUD grid page"** (§6.2 of the spec): view = `card` → `card-body` → `table-responsive` → `table table-striped` with a header `btn btn-success` "New" and rows with Edit/Deactivate buttons gated by `ng-show`; edit modal `#AccountModal` bound to `vm.Modal` with `{{vm.ModalHeader}} Account` title; controller = `NewAccount()` (`vm.ModalHeader="New"; vm.Modal={}; ShowModal("AccountModal")`), `EditAccount(value)` (`vm.Modal = angular.copy(value); ShowModal(...)`), `Save()` (growl validation → `$http` POST `/Settings/SaveAccount` → on success `growl.success`, `HideModal("AccountModal")`, `$scope.Init()`), all in OneMasaito's style. Link `../BUILD_GUIDE.md` for the real `UserAccounts` files.
- **toasts**: `div.toast-container.position-fixed.top-0.end-0.p-3 > div.toast` + `coreui.Toast.getOrCreateInstance(el).show()`; state plainly that CoreUIDemo uses angular-growl for the same purpose and toasts are optional.
- **tooltips**: same init pattern as popovers with `coreui.Tooltip`; `data-coreui-title` vs `title` (`grep -c 'data-coreui-title' components/tooltips.html` — use whichever the dist uses).

- [ ] **Step 4: Verify** — `bash <scratchpad>/check-handbook.sh docs/coreui/03-components.md`; expected `OK`. Length 800–1,100 lines.

- [ ] **Step 5: Checkpoint** — `03-components.md` ready.

---

### Task 5: `docs/coreui/04-forms.md`

**Files:**
- Create: `docs/coreui/04-forms.md`
- Read: dist `forms/*.html` (9 files), repo `Views/Settings/UserAccounts.cshtml` (modal form), repo `Views/Home/Login.cshtml`

**Interfaces:**
- Consumes: modal markup from Task 4.
- Produces: nothing other tasks depend on.

- [ ] **Step 1: Extract first examples** — same `awk '/<div class="example">/{c++} c==1'` per file; and `grep -o 'data-coreui-[a-z-]*' forms/chip-input.html | sort -u`.

- [ ] **Step 2: Write `04-forms.md`** — `# 04 · Forms` + TOC + nine entries:

- **form-control**: `form-label` + `form-control`, sizes, `readonly`/`disabled` (`ng-readonly`, `ng-disabled`), `form-control-plaintext`, `form-text`, file input, `textarea`; every input shown with `ng-model="vm.Modal.<Field>"`; the OneMasaito keypress filter (`$("#firstName").keypress(...)`) noted as the one jQuery use — link BUILD_GUIDE.
- **select**: `form-select`, sizes, `multiple`; AngularJS: `ng-options="d.Id as d.Name for d in vm.DepartmentList"` with a `<option value="">Select…</option>` placeholder; why not `ng-repeat` on `<option>` (loses object binding).
- **checks-radios**: `form-check` / `form-check-input` / `form-check-label`, `form-switch`, inline, reverse; `ng-model` boolean; radios with `ng-value`; the button-style `btn-check`.
- **range**: `form-range` with `ng-model` + `min`/`max`/`step`; live value `{{vm.Level}}`.
- **input-group**: `input-group > input-group-text + form-control`, buttons inside, sizing; the header search box from 01 as the CoreUIDemo example; BS4→BS5 note (no `-append` wrappers, link COREUI_GUIDE §10).
- **floating-labels**: `form-floating > form-control[placeholder] + label` (placeholder attribute required); with `ng-model`.
- **layout**: `row g-3` + `col-md-*`, `col-form-label`, horizontal forms, inline `row row-cols-lg-auto g-3 align-items-center`; a two-column modal form rewritten from the UserAccounts modal.
- **chip-input**: dist markup and `data-coreui-*` attributes for `coreui.ChipInput`; note it is CoreUI-only and that the value is read from the element (`coreui.ChipInput.getInstance(el).value` — verify the API name by `grep -o 'ChipInput[^;]\{0,200\}' vendors/@coreui/coreui/js/coreui.bundle.min.js | head`; if unclear, document the `data-coreui-*` attributes only and say "read values via the element's hidden input").
- **validation**: the dist's `form.needs-validation[novalidate]` + `was-validated` + `invalid-feedback` / `valid-feedback` with the JS snippet the dist includes (quote it from `forms/validation.html`), then the OneMasaito way (controller checks + `growl.error`, quote `Save()` shape from Task 4) and a middle path: `ng-class="{'is-invalid': vm.Submitted && !vm.Modal.Username}"` + `invalid-feedback` with `ng-show`. State which one CoreUIDemo uses (growl).

- [ ] **Step 3: Verify** — `bash <scratchpad>/check-handbook.sh docs/coreui/04-forms.md`; expected `OK`. Length 400–600 lines.

- [ ] **Step 4: Checkpoint** — `04-forms.md` ready.

---

### Task 6: `docs/coreui/05-icons.md`

**Files:**
- Create: `docs/coreui/05-icons.md`
- Read: dist `icons/coreui-icons-free.html` (structure only), `vendors/@coreui/icons/css/{free,brand,flag}.min.css` (grep), repo `Content/vendor/@coreui/icons/`, repo `App_Start/BundleConfig.cs`

- [ ] **Step 1: Facts**

Run from dist root: `grep -oE '\.cil-[a-z0-9-]+' vendors/@coreui/icons/css/free.min.css | sort -u | wc -l` (expected 562); same for `\.cib-` in `brand.min.css` and `\.cif-` in `flag.min.css`; `grep -o 'icon-[a-z]*' css/style.css | sort -u` (expected `icon-sm icon-lg icon-xl icon-xxl` and possibly `icon-custom-size`); `grep -c '\.nav-icon' css/style.css` > 0; `grep -m3 -o '<svg class="icon[^"]*"' index.html` to show the dist's SVG usage; confirm `ls vendors/@coreui/icons/svg` shows only `flag/`.

- [ ] **Step 2: Write `05-icons.md`** — `# 05 · Icons` + TOC:

1. `## What the three catalogue pages are` — free (562, vendored), brand (`cib-*`, not vendored), flag (`cif-*`, not vendored; the only set with SVG files in this dist). How to open the catalogue and read a name.
2. `## Font usage` — `<i class="cil-user"></i>`; sizing `icon icon-sm/lg/xl/xxl`; colour via `text-*`; in the sidebar `nav-icon`; in header `icon icon-lg`; in buttons; `me-2` spacing convention. Link COREUI_GUIDE §8 for the table of icons CoreUIDemo already uses.
3. `## The dist uses inline SVG; CoreUIDemo uses the font — why` — the dist's `<svg class="icon"><use xlink:href="…/free.svg#cil-…">` needs a sprite file this dist does not ship (only flag SVGs are included), so the font is the practical choice; both render identically at the sizes used.
4. `## Adding brand or flag icons` — copy `brand.min.css` + `CoreUI-Icons-Brand.*` fonts (or `flag.min.css` + flag fonts) from the dist to `Content/vendor/@coreui/icons/{css,fonts}/`, add to `~/Content/css` after `free.min.css`, keep `css/` and `fonts/` siblings (link COREUI_GUIDE §3 for why).
5. `## Icons with AngularJS` — `ng-class` to pick an icon (`<i ng-class="acc.Status === 1 ? 'cil-lock-locked' : 'cil-lock-unlocked'"></i>` — verify both names), `ng-show` pairs as OneMasaito does; caution: never interpolate `class="cil-{{x}}"` on an element that also has static classes needing CoreUI init.
6. `## Reference: 40 icons an admin app needs` — a table of common names verified against `free.min.css` (menu, search, user, people, settings, lock-locked, lock-unlocked, account-logout, plus, pencil, trash, check, x, ban, check-circle, warning, info, bell, envelope-closed, home, speedometer, list, folder, file, cloud-download, cloud-upload, print, calendar, clock, chart, bar-chart, dollar, credit-card, cart, truck, map, phone, filter, sort-ascending, sort-descending — replace any that fail the grep with a verified alternative).

- [ ] **Step 3: Verify** — `bash <scratchpad>/check-handbook.sh docs/coreui/05-icons.md`; expected `OK` with **zero** `UNKNOWN ICON` lines. Length 150–250 lines.

- [ ] **Step 4: Checkpoint** — `05-icons.md` ready.

---

### Task 7: `docs/coreui/02-authentication.md`

**Files:**
- Create: `docs/coreui/02-authentication.md`
- Read: dist `authentication/*.html` (6 files), repo `Views/Home/Login.cshtml`, repo `App/Controller/Login.js`

- [ ] **Step 1: Facts** — `diff <(sed -n '1,40p' authentication/login.html) <(sed -n '1,40p' authentication/register.html)` to confirm the shared frame; `grep -n 'ng-app\|Layout = null\|data-coreui-theme' Views/Home/Login.cshtml` in the repo; `grep -o 'data-coreui-[a-z-]*' authentication/*.html | sort -u`.

- [ ] **Step 2: Write `02-authentication.md`** — `# 02 · Authentication pages` + TOC:

1. `## The shared frame` — `body.bg-body-tertiary.min-vh-100.d-flex.flex-row.align-items-center > div.container[style=max-width:32rem] > div.d-flex.flex-column.gap-4 (logo, card.p-4 > card-body.d-flex.flex-column.gap-4, footer text)`; why these pages are `Layout = null` with their own `ng-app` module (link COREUI_GUIDE §6); the theme-on-load one-liner (link COREUI_GUIDE §9).
2. One entry per page: **login** (link to COREUI_GUIDE §6 and the real `Login.cshtml`; do not repeat), **register** (fields, terms checkbox, `ng-model` for each, controller `Register()` posting to `/Home/Register`), **reset-password** (single email field → sketch `Views/Home/ResetPassword.cshtml` + `App/Controller/ResetPassword.js` with module `"resetpassword"`), **check-email** (static confirmation page — `Layout = null`, no controller), **change-password** (three fields; note CoreUIDemo does this in the `#PasswordModal` inside the layout instead — link `_Layout.cshtml` and `mainController.ChangePassword`), **password-changed** (static).
3. `## Show/hide password button` — the dist's `input-group` + `btn btn-outline-secondary` with an eye icon and the tiny toggle; AngularJS version: `type="{{vm.ShowPassword ? 'text' : 'password'}}"` + `ng-click="vm.ShowPassword = !vm.ShowPassword"`; note the tooltip on it needs `coreui.Tooltip` init (link 03 tooltips).
4. `## Gotchas` — the form must have no `action` and the button `type="button"` (Enter handled by the controller as `Login.js` does); `growl` container must be inside `<body>` on these pages because there is no layout.

- [ ] **Step 3: Verify** — check script `OK`. Length 250–350 lines.

- [ ] **Step 4: Checkpoint** — `02-authentication.md` ready.

---

### Task 8: `docs/coreui/06-widgets-charts.md`

**Files:**
- Create: `docs/coreui/06-widgets-charts.md`
- Read: dist `widgets.html`, `charts.html`, `js/widgets.js`, `js/charts.js`, `js/main.js`, `vendors/@coreui/chartjs/js/coreui-chartjs.js` (header comment only), `vendors/@coreui/utils/js/index.js` (exports only), repo `App_Start/BundleConfig.cs`

- [ ] **Step 1: Facts**

Run from dist root: `grep -oE 'class="card[^"]*"' widgets.html | sort | uniq -c | sort -rn | head -20`; `grep -n 'new Chart\|coreUI.Utils\|getStyle\|hexToRgba' js/widgets.js | head`; `head -5 vendors/chart.js/js/chart.umd.js` (version); `grep -n '<script' charts.html widgets.html | grep -v 'coreui.bundle\|simplebar\|color-modes'` (which extra scripts they load and in what order); `grep -c 'progress-group' css/style.css`; `grep -o 'class="[^"]*widget[^"]*"' widgets.html | sort -u`.

- [ ] **Step 2: Write `06-widgets-charts.md`** — `# 06 · Widgets and charts` + TOC:

1. `## Widgets are cards` — the four patterns from `widgets.html`, each a five-part entry: stat card (`card text-white bg-primary` with value, label and a dropdown), stat card with sparkline (`card-body` + a `<canvas>` region — the chart itself belongs to §3), `progress-group` list (`progress-group > progress-group-header + progress-group-bars > progress progress-thin`), icon/brand cards (`card-body p-3 d-flex align-items-center` with a coloured icon box). AngularJS: `ng-repeat` over `vm.Stats` for a row of stat cards; values via `{{s.Value | number}}`.
2. `## Charts — what the dist ships` — table: `vendors/chart.js/js/chart.umd.js` (Chart.js UMD, version from the header), `vendors/@coreui/chartjs/{css,js}` (CoreUI plugin: custom tooltips + `coreui-chartjs.css`), `vendors/@coreui/utils/js/index.js` (`coreUI.Utils.getStyle`, `hexToRgba` … — list the ones `widgets.js` calls); which pages use them; that none of this is vendored in CoreUIDemo today.
3. `## Enabling charts in CoreUIDemo` — copy list (`Content/vendor/chart.js/js/chart.umd.js`, `Content/vendor/@coreui/chartjs/js/coreui-chartjs.js`, `Content/vendor/@coreui/chartjs/css/coreui-chartjs.css`, `Content/vendor/@coreui/utils/js/index.js`), exact `BundleConfig.cs` lines: `coreui-chartjs.css` appended to `~/Content/css` after `simplebar.css`; the three JS files appended to `~/bundles/scripts` **after** `coreui.bundle.min.js` and **before** `angular.min.js`, in the order `chart.umd.js`, `coreui-chartjs.js`, `index.js` (matches the dist's script order — confirm in Step 1). Note `Transforms.Clear()` already covers them (link COREUI_GUIDE §4 rule 3).
4. `## A chart driven by AngularJS` — `Views/Reports/Index.cshtml` gets `<div class="card mb-4"><div class="card-body"><canvas id="salesChart" height="300"></canvas></div></div>`; `App/Controller/Reports.js` gets `$timeout` injected and, inside the `.then` of `Init()`: `$timeout(function () { if (vm.Chart) { vm.Chart.destroy(); } vm.Chart = new Chart(document.getElementById("salesChart"), { type: "line", data: { labels: data.data.labels, datasets: [{ label: "Sales", data: data.data.values, borderColor: coreUI.Utils.getStyle("--cui-primary"), backgroundColor: "transparent" }] }, options: { plugins: { legend: { display: false } }, maintainAspectRatio: false } }); });` — note that reading `--cui-primary` via `getStyle` is *using* a variable the template defines, not overriding one, so it is within policy. Add a `$scope.$on("$destroy", ...)` that destroys the chart.
5. `## Theme-aware charts` — the dist re-renders charts on `ColorSchemeChange` (`grep -n 'ColorSchemeChange' js/*.js`); show the listener `document.documentElement.addEventListener("ColorSchemeChange", function () { $scope.Init(); })` and that it is optional.
6. `## Gotchas` — `canvas` inside `ng-show` regions has zero size until shown (create the chart after showing); `maintainAspectRatio: false` requires a fixed-height parent; don't put `new Chart` inside `ng-repeat` without unique ids.

- [ ] **Step 3: Verify** — check script `OK`; additionally `grep -c 'ColorSchemeChange' docs/coreui/06-widgets-charts.md` ≥ 1 only if Step 1 found it in the dist. Length 300–450 lines.

- [ ] **Step 4: Checkpoint** — `06-widgets-charts.md` ready.

---

### Task 9: `docs/coreui/07-error-pages.md` + repo README link + final link check

**Files:**
- Create: `docs/coreui/07-error-pages.md`
- Modify: `README.md` (repo root) — add one bullet under the existing docs list
- Read: dist `error-pages/404.html`, `500.html`, repo `Views/Shared/Error.cshtml`, repo `Web.config`, repo `App_Start/FilterConfig.cs`

- [ ] **Step 1: Facts** — `sed -n '/<body/,/<\/body>/p' error-pages/404.html` (the whole page is ~40 lines: `bg-body-tertiary min-vh-100 d-flex flex-row align-items-center`, `h1.float-start.display-3.me-4`, `h4.pt-3`, `p.text-body-secondary`, an `input-group` search). `grep -n 'customErrors\|httpErrors' Web.config`; `grep -n 'HandleErrorAttribute' App_Start/FilterConfig.cs`; `cat Views/Shared/Error.cshtml`.

- [ ] **Step 2: Write `07-error-pages.md`** — `# 07 · Error pages` + TOC:

1. `## 404 and 500 in the dist` — one entry covering both (identical structure, different number/text); which classes; the search `input-group` is decorative (drop it or bind to `main.SearchBox` — but there is no `mainController` on a `Layout = null` page, so drop).
2. `## 500 → Views/Shared/Error.cshtml` — what `HandleErrorAttribute` in `FilterConfig` does (only when `<customErrors mode="On">`), the current `Error.cshtml` (link `../BUILD_GUIDE.md`), and the same page rebuilt with the dist's 500 markup: complete `Error.cshtml` with `@{ Layout = null; }`, `@model System.Web.Mvc.HandleErrorInfo`, the dist body, a `btn btn-primary` "Back to Dashboard" link to `/Home/Index`.
3. `## 404 → Views/Shared/NotFound.cshtml` — complete view with the dist's 404 markup (`Layout = null`), and the `Web.config` change only: `<customErrors mode="On" defaultRedirect="~/Home/Index"><error statusCode="404" redirect="~/Views/Shared/NotFound.cshtml" /></customErrors>` is **not** valid (views aren't served directly) — instead document the route-based option: `<customErrors mode="On"><error statusCode="404" redirect="~/Home/NotFound" /></customErrors>` plus the one-line action `public ActionResult NotFound() { Response.StatusCode = 404; return View(); }` in `HomeController`, and `Views/Home/NotFound.cshtml`. Mark the action as the only C# in this chapter and that OneMasaito has no such page (optional addition).
4. `## Gotchas` — `customErrors` only handles ASP.NET-pipeline errors; static-file 404s need `<httpErrors>` under `<system.webServer>` (show the XML); `Layout = null` pages must load `~/Content/css` themselves (`@Styles.Render("~/Content/css")`).

- [ ] **Step 3: Add the README link** — in repo `README.md`, under the documentation list that mentions `docs/COREUI_GUIDE.md`, add: `- [docs/coreui/README.md](docs/coreui/README.md) — CoreUI template handbook: how to use and customize every page and component of the dist on this stack`. Verify the list exists first: `grep -n 'COREUI_GUIDE' README.md`.

- [ ] **Step 4: Verify everything**

Run: `for f in docs/coreui/*.md; do bash <scratchpad>/check-handbook.sh "$f"; done`
Expected: nine `OK` lines (prose false positives noted and dismissed).
Run: `grep -oE '\]\(\.\./[A-Z_]+\.md#[a-z0-9-]+' docs/coreui/*.md | sed 's/.*](//' | sort -u` and for each `file#anchor` confirm the heading exists: `grep -n '^## ' docs/COREUI_GUIDE.md docs/BUILD_GUIDE.md` and compare slugs (GitHub slug = lowercase, spaces→`-`, punctuation removed).
Expected: every anchor matches a heading.
Run: `wc -l docs/coreui/*.md | tail -1` — expected total 2,500–3,700.

- [ ] **Step 5: Checkpoint** — tell the user all nine files + the README line are ready to commit, with the suggested message `docs: CoreUI template handbook (docs/coreui)`.

---

## Self-review

- **Spec coverage:** §1 purpose → Task 1; §2 stack → Global Constraints + Task 1 §3; §3 boundary → link rules in every task + Task 9 README line; §4 files → Tasks 1–9 (all nine files); §5 entry shape → Global Constraints, enforced per task; §6.1 → Task 2 step 3.7; §6.2 → Task 4 tables entry; §7 out of scope → Task 1 §2; §8 accuracy → check script + per-task fact steps; §9 order → task order 1, 2 (01), 3 (08), 4 (03), 5 (04), 6 (05), 7 (02), 8 (06), 9 (07 + README).
- **Spec corrections applied here:** no `header-fixed` class exists (Task 3 lists only `header-sticky`); no free/brand SVG sprite exists in this dist (Task 6 §3 explains); `sidebar-md` does not exist (only `sm`/`lg`/`xl`).
- **Placeholders:** none — every section lists its required content; code that appears in the handbook is given in the step or must be quoted from a named dist/repo file.
- **Name consistency:** `ReportsController` / `Views/Reports/Index.cshtml` / `App/Controller/Reports.js` / module `"reports"` / `reportsController` are used identically in Tasks 2 and 8; `ShowModal`/`HideModal`/`AccountModal` identically in Tasks 4, 5; check script name `check-handbook.sh` everywhere.
