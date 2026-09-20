# 08 · Customizing with what the template ships

How far the look of the shell and the pages can be changed by *choosing* among the classes, attributes and files that already come with the CoreUI v5.5.0 dist — the sidebar and header variants that [01 · Layout](./01-layout.md) points here for, the colour-mode attribute, the brand images, the eight theme colours and the width and spacing utilities. This chapter is a reference of variants rather than a set of component entries, so it uses tables with one short sample per section instead of the five-part shape in [README · How to read an entry](./README.md#how-to-read-an-entry); code fences are still labelled with their target path.

- [Sidebar variants](#sidebar-variants)
- [Header variants](#header-variants)
- [Colour modes](#colour-modes)
- [Brand and favicon](#brand-and-favicon)
- [Component colour variants](#component-colour-variants)
- [Page width](#page-width)
- [Spacing and layout utilities the template leans on](#spacing-and-layout-utilities-the-template-leans-on)
- [Turning things off](#turning-things-off)

> **No custom CSS, no SCSS, no `--cui-*` overrides in this project (decision 2026-09-19). Everything below is a class or attribute the dist already provides.**
>
> Every class in this chapter has a rule in the dist's `css/style.css` (checked with `grep -c "\.<class>" css/style.css` on 2026-09-19; the counts are in the task report, not repeated here). Where the text explains that a variable such as `--cui-body-bg` *drives* a colour, that is a description of how `style.css` works, not an invitation to set it — the project has no build step and no stylesheet of its own to set it in ([`../COREUI_GUIDE.md` § 2](../COREUI_GUIDE.md#2-why-hand-vendored-not-nuget-or-npm)).

## Sidebar variants

The variants are classes on the one `div.sidebar` element in `Views/Shared/_Layout.cshtml`. Today it is:

**`Views/Shared/_Layout.cshtml`** — current (dist `index.html` line 49 is identical)
```html
<div class="sidebar sidebar-dark sidebar-fixed border-end" id="sidebar">
```

So **CoreUIDemo uses `sidebar sidebar-dark sidebar-fixed border-end`**: dark palette, fixed to the viewport's left edge, default 16rem width, expanded on load, with the footer toggler set to `unfoldable`. That is the dist's own choice for `index.html` and `blank.html`. Everything in the table below is a change to that one `class` attribute (plus, for the narrow modes, the toggler's `data-coreui-toggle` value). `coreui.Sidebar` reads the classes at run time, so no JavaScript changes with them.

| Class | Effect (from `style.css`) | How to apply |
|---|---|---|
| *(no colour class)* | Light sidebar: `.sidebar` paints itself with the page's `--cui-body-bg` / `--cui-body-color`, so it is white on a light page and dark only when the page theme is dark. | `class="sidebar sidebar-fixed border-end"` (drop `sidebar-dark`). See [Colour modes](#colour-modes) for a dark sidebar that stays dark. |
| `sidebar-dark` | Sets the body, secondary, tertiary and border colour variables *on the sidebar* to the dark palette (`#212631` background), whatever the page theme is. `.sidebar-dark .sidebar-toggler` gets its own colours. | **In use today.** |
| `sidebar-fixed` | Pins the sidebar to the viewport (`position: fixed; top: 0; bottom: 0; inset-inline-start: 0`). The wrapper's 16rem left padding (a custom property `style.css` sets from the sidebar's classes) is applied for any non-overlaid sidebar — it fires independently of this class, from `.sidebar:not(.hide):not(.sidebar-narrow)…:not(.sidebar-overlaid):not(.sidebar-end) ~ *`. | **In use today — keep it.** A non-fixed sidebar has no supported layout in this dist's markup: `body` is not a flex container, and all 38 dist pages carry `sidebar-fixed`. |
| `sidebar-narrow` | At ≥ 992px the sidebar is 4rem wide and stays so: labels, `nav-title` rows and `sidebar-brand-full` hidden, `sidebar-brand-narrow` and `nav-icon` shown. `.wrapper` pads 4rem. The click on `[data-coreui-toggle="narrow"]` inside the sidebar toggles this class. | `class="sidebar sidebar-dark sidebar-fixed sidebar-narrow border-end"` to start narrow; change the footer button to `data-coreui-toggle="narrow"` so the toggler operates the same mode. |
| `sidebar-narrow-unfoldable` | Same 4rem width, but `:hover` expands it to full width *over* the content — `.wrapper` stays padded 4rem, so nothing reflows. It is also `position: fixed` on its own, which is why the dist's toggler works with or without `sidebar-fixed`. | Toggled by the existing `button.sidebar-toggler[data-coreui-toggle="unfoldable"]` in the sidebar footer (see [01 · toggler](./01-layout.md#toggler-and-sidebar-footer)). Add the class to the `div.sidebar` to start collapsed on every load. |
| `sidebar-overlaid` | Off-canvas on desktop too: `position: fixed` with a negative start margin, and `.wrapper` never pads for it. `show` (added by `coreui.Sidebar` when the header toggler calls `toggle()`) slides it in with `--cui-box-shadow`; on desktop a click outside hides it again, with no backdrop — `sidebar-backdrop` is only created on mobile, where every sidebar already behaves this way. | `class="sidebar sidebar-dark sidebar-overlaid border-end"` (replace `sidebar-fixed`). Drop the footer toggler: narrow modes make no sense for a drawer. |
| `sidebar-end` | Sidebar on the inline-end (right) edge: `order: 99`, positioned from the end, and `.wrapper` pads its inline-end instead of its start. `hide`/`show` margins flip sides automatically. | `class="sidebar sidebar-dark sidebar-fixed sidebar-end border-start"` (`border-end` → `border-start`). Move the `header-toggler` to the right of the header to match. |
| `sidebar-sm` · `sidebar-lg` · `sidebar-xl` | Width 12rem · 20rem · 24rem instead of 16rem; `.wrapper` follows the same value. There is no medium variant — 16rem is the default. Narrow modes ignore the size (always 4rem). | Add one of the three, e.g. `class="sidebar sidebar-dark sidebar-fixed sidebar-lg border-end"`. |
| `sidebar-nav` + `compact` | Not on `div.sidebar`: `.sidebar-nav.compact .nav-link` and `.sidebar-nav .compact .nav-link` shorten every row to 0.5625rem vertical padding. The dist uses it on `ul.nav-group-items` (children only). | `<ul class="sidebar-nav compact" data-coreui="navigation" data-simplebar>` for the whole menu. |

Below 992px none of the positioning classes matter: `style.css` sets `--cui-is-mobile: true` on `.sidebar`, makes it fixed and off-canvas, and `coreui.Sidebar` slides it in with a backdrop (the mobile behaviour in [01 · mobile close button](./01-layout.md#mobile-close-button)). `sidebar-dark` and the size classes still apply.

The most common change — start collapsed to icons and let the toggler expand it — is two edits, both already sized for by `style.css`:

**`Views/Shared/_Layout.cshtml`** — sidebar that loads narrow; the footer toggler switches between narrow and full
```html
<div class="sidebar sidebar-dark sidebar-fixed sidebar-narrow border-end" id="sidebar">
    …
    <div class="sidebar-footer border-top d-none d-md-flex">
        <button class="sidebar-toggler" type="button" data-coreui-toggle="narrow"></button>
    </div>
</div>
```

Combining variants:

- Pick at most one from each group: colour (`sidebar-dark` or none) · position (`sidebar-fixed` or `sidebar-overlaid` — keep one of the two; a non-fixed, non-overlaid sidebar has no supported layout in this dist) · side (`sidebar-end` or none) · size (`sidebar-sm`, `sidebar-lg`, `sidebar-xl`, or none) · narrow mode (`sidebar-narrow`, `sidebar-narrow-unfoldable`, or none).
- `sidebar-narrow` and `sidebar-narrow-unfoldable` are mutually exclusive — the toggler adds one and removes the other, and the `toggle` value on the footer button should name the one the sidebar starts with.
- `sidebar-overlaid` plus a narrow class is pointless: a drawer is either open at full width or gone.
- `border-end` / `border-start` is just the 1px line between sidebar and content; it is a plain utility, not a sidebar class, and goes on whichever side faces the content.
- To try a combination before editing the view, open the dist's `index.html`, select `div#sidebar` in DevTools and edit its `class` attribute live — the CSS and `coreui.Sidebar` react immediately, which is the fastest way to see what `sidebar-end` or `sidebar-xl` looks like with real content.

## Header variants

The header is `<header class="header header-sticky p-0 mb-4">` with the row inside `<div class="container-fluid border-bottom px-4">` ([01 · header](./01-layout.md#header)). `style.css` defines these header classes and nothing else:

| Class | What it does | Used by |
|---|---|---|
| `header-sticky` | `.header.header-sticky { position: sticky; top: 0; z-index: 1020 }` — `.wrapper` is not itself a scroll container, so this sticks the header relative to the viewport as the page scrolls. It is the **only** positioning variant: there is no fixed variant for the header in this dist. Remove the class for a header that scrolls away. | Dist (all 38 pages) and CoreUIDemo. |
| `border-bottom` (on the inner `container-fluid`) | The line under the first row. The dist puts it on the row container rather than on `header.header` so that a second row (the breadcrumb) can sit *below* the line inside the same sticky header. | Dist and CoreUIDemo. |
| `header-brand` | A logo/site-name slot inside the header (`font-size: 1.25rem`, vertical padding, 1rem end margin) — for a layout whose brand is not in the sidebar. | Defined only; no dist page uses it. |
| `header-divider` | A full-width horizontal rule between two rows of the same header (negative side margins so it spans the padding). The dist's two-row header uses `border-bottom` on the row instead. | Defined only. |
| `header-text` | Plain text in the header row with the header's own text colour and `nav-link` vertical padding — e.g. a page title or the signed-in user's name outside a dropdown. | Defined only. |
| `header-toggler-icon` | A `span` inside `button.header-toggler` that draws the hamburger as a CSS background image; alternative to the `icon icon-lg cil-menu` glyph CoreUIDemo uses. | Defined only; the dist draws its hamburger with an inline SVG. |

**`Views/Shared/_Layout.cshtml`** — the two "defined only" helpers, if wanted, drop into the first row
```html
<div class="container-fluid border-bottom px-4">
    <button class="header-toggler" type="button" …><span class="header-toggler-icon"></span></button>
    <span class="header-text ms-3">@ViewBag.Title</span>
    …
```

Two utilities on the same elements are worth knowing as variants even though they are not `header-*` classes: `mb-4` on `header.header` is the 1.5rem gap between the header and the first card (remove it for content flush under the line), and `p-0` cancels the header's own padding so that the inner `container-fluid px-4` controls the gutter — keep `p-0` whenever the row container has `px-4`, or the gutters double.

## Colour modes

One attribute, `data-coreui-theme`, in two places:

**On `<html>` — global.** `Scripts/js/color-modes.js` (dist `js/color-modes.js`, unmodified) sets `data-coreui-theme="light"` or `"dark"` on `document.documentElement` from the value stored under `localStorage['coreui-free-bootstrap-admin-template-theme']`. The stored value can be `light`, `dark` or `auto`; `auto` is resolved against `window.matchMedia('(prefers-color-scheme: dark)')` and re-resolved when the OS preference changes, so the attribute on `<html>` is `light` or `dark` whenever the OS prefers dark or the stored value is one of those two. When the stored value is `auto` and the OS prefers light, `setTheme()` writes `data-coreui-theme="auto"` literally onto `<html>` (it still renders light because no rule matches `[data-coreui-theme=auto]`). The dropdown items with `data-coreui-theme-value` are what write the stored value. All of that, and how to remove switching altogether (delete the dropdown *and* the script tag, then write `<html ng-app="app" data-coreui-theme="light">` — the script throws without the dropdown), is in [`../COREUI_GUIDE.md` § 9](../COREUI_GUIDE.md#9-dark-mode).

**On any element — local.** `style.css` has 86 rules that start with `[data-coreui-theme=dark]`. The first is `[data-coreui-theme=dark] { color-scheme: dark; --cui-body-bg: #212631; --cui-body-color: …; --cui-secondary-bg: …; --cui-tertiary-bg: …; --cui-border-color: …; … }` — an attribute selector with no element in front of it, so it matches *any* element carrying the attribute and rescopes every colour variable for that element's subtree. The others (`[data-coreui-theme=dark] .btn-primary`, `… .text-bg-primary`, `… .footer`, `… .form-select`, …) are descendant rules, so they also apply inside a local dark subtree. The dist uses this on the mobile `btn-close` (white × on the dark sidebar, `index.html` line 69); the same trick gives a dark sidebar on an otherwise light page without `sidebar-dark`:

**`Views/Shared/_Layout.cshtml`** — dark sidebar in a light page, by attribute instead of class
```html
<div class="sidebar sidebar-fixed border-end" id="sidebar" data-coreui-theme="dark">
```

The difference from `sidebar-dark`: the class sets eight variables (background, text, secondary, tertiary, border — without their `-rgb` twins), enough for the sidebar's own parts; the attribute sets the whole dark set, so utilities used *inside* the sidebar (`bg-body-tertiary`, `text-body-secondary`, `border`, `text-bg-*`) go dark too, and `color-scheme: dark` makes native controls (a `form-select` in a `sidebar-form`) render dark. Either way the sidebar no longer follows the theme dropdown — it is dark in both modes. The reverse is asymmetric rather than unavailable: `style.css` line 7 opens `:root, [data-coreui-theme=light] { … }`, so putting `data-coreui-theme="light"` on a subtree inside a dark page does flip that subtree's variables to light. But the ~85 rules that follow are written as `[data-coreui-theme=dark] .something` (descendant selectors), and those still match through a light island because the `[data-coreui-theme=dark]` attribute is up on `<html>`, not on the island — so a light island inside a dark page gets the light variables but keeps the dark component rules (`btn-primary`, `text-bg-*`, `.footer`, `.form-select`, …), i.e. it is only partially light. A dark island inside a light page (the sidebar case above) does not have this problem, because there is no `<html>`-level `[data-coreui-theme=light] .something` descendant rule for it to collide with — the dark direction is the fully supported one.

While `color-modes.js` is in place, `<html>` carries one of these from the moment the script runs (plus the `auto`-with-light-OS case above) — its top-level `setTheme(getPreferredTheme())` executes as the `<head>` is parsed, before any of the body renders, which is why `_Layout.cshtml` loads it as a plain `<script>` tag ahead of the bundles instead of inside one:

**`Views/Shared/_Layout.cshtml`** — what the script produces (not something to type; the view has `<html ng-app="app">`)
```html
<html ng-app="app" data-coreui-theme="light">
<html ng-app="app" data-coreui-theme="dark">
```

`Views/Home/Login.cshtml` (`Layout = null`) does write the attribute itself, `data-coreui-theme="light"`, so the login page is pinned light and does not load the script — the one-liner that makes it follow the saved preference instead is in § 9.

Which utilities follow the theme and which do not, so that a page looks right in both modes without any page-specific colour:

| Follow the theme (read a `--cui-body-*` / `--cui-*-bg` / `--cui-border-color` variable) | Fixed regardless of theme (read a palette constant) |
|---|---|
| `bg-body`, `bg-body-secondary`, `bg-body-tertiary` — page, card header and page-header backgrounds | `bg-white`, `bg-light`, `bg-dark` — `--cui-white-rgb` and `--cui-dark-rgb` are the same in both modes |
| `text-body`, `text-body-secondary`, `text-body-tertiary`, `text-body-emphasis` | `text-white`, `text-dark`, `text-black` — `text-dark` on a dark page is `#212631` on `#212631` |
| `border`, `border-top`/`-end`/`-bottom`/`-start` (colour from `--cui-border-color`) | `border-white`, `border-dark`, and any `border-<theme colour>` |
| `card`, `dropdown-menu`, `list-group`, `table`, `form-control` — component backgrounds and borders are variable-driven | `bg-<theme colour>`, `text-<theme colour>` keep the same hue; `btn-*` and `text-bg-*` get a *tuned* dark rule, not a swap |

Rule of thumb from the dist's own pages: neutrals come from the `-body-*` family, colour comes from the theme colours, and `bg-white`/`text-dark` are not used anywhere in `index.html`.

## Brand and favicon

The brand is two images in the sidebar header — both always in the DOM, `style.css` shows one or the other ([01 · brand](./01-layout.md#brand-and-sidebar-header)) — and one favicon `link` in `<head>`. CoreUIDemo's files, in `Src/Image/` (`ls Src/Image`):

| File | Used as |
|---|---|
| `masaito-logo-dark-gradient.svg` | `sidebar-brand-full` — the wide logo in the dark sidebar |
| `masaito-mark-dark-gradient.svg` | `sidebar-brand-narrow` — the square mark shown in narrow / unfoldable-not-hovered mode |
| `masaito-mark-light-gradient.svg` | `<link rel="icon">` — the browser tab icon |
| `masaito-logo-light-gradient.svg` | not referenced by `_Layout.cshtml` — the wide logo for a light background, ready for a light sidebar |

**`Views/Shared/_Layout.cshtml`** — current markup (lines 8 and 26–29)
```html
<link href="~/Src/Image/masaito-mark-light-gradient.svg" rel="icon" type="image/svg+xml" />
…
<div class="sidebar-brand me-auto">
    <img class="sidebar-brand-full" style="height:32px;" src="~/Src/Image/masaito-logo-dark-gradient.svg" alt="Masaito Development Corporation" />
    <img class="sidebar-brand-narrow" style="height:32px;" src="~/Src/Image/masaito-mark-dark-gradient.svg" alt="Masaito" />
</div>
```

To rebrand: replace the SVG files in `Src/Image/` keeping the names (nothing else changes), or drop new files in and change the three `src`/`href` values. Keep one wide image and one square image — a single wide logo used for both is clipped to the 4rem narrow width. Swapping to the `-light-gradient` pair goes with dropping `sidebar-dark` (light sidebar), since the dark-gradient artwork is drawn for a dark background. The dist itself ships inline SVGs for the brand (`index.html` lines 52 and 62, `width="88" height="32"`) and a folder of PNG favicons (`assets/favicon/`, one `<link rel="icon" sizes="…">` per size); CoreUIDemo's single SVG favicon is the simpler equivalent — modern browsers accept `type="image/svg+xml"`, and nothing in the template depends on the PNG set.

## Component colour variants

Eight theme colours — `primary secondary success danger warning info light dark` — and the class families that take them. What the dist uses each for, so that CoreUIDemo's pages mean the same thing by the same colour:

- `primary` — the one action per view (`btn-primary` on a modal's Save, the active `nav-link`, links); CoreUIDemo's footer text.
- `secondary` — neutral or dismiss actions (`btn-secondary` on the modal's Cancel), neutral badges.
- `success` / `danger` / `warning` / `info` — status: saved / failed or delete / needs attention / informational. `growl.success` and `growl.error` map to the first two.
- `light` / `dark` — neutral surfaces; rarely needed because the `-body-*` utilities in [Colour modes](#colour-modes) already give theme-aware neutrals.

Each family has one class per colour; the table shows the `primary` member, and replacing `primary` with any of the other seven gives the sibling (all 80 — ten families × eight colours — exist in `style.css`).

| Family | Example | Where it applies | Notes |
|---|---|---|---|
| `btn-primary` | `<button class="btn btn-primary">` | Solid buttons | Has a `[data-coreui-theme=dark]` variant with adjusted hover/active. |
| `btn-outline-primary` | `<button class="btn btn-outline-primary">` | Bordered buttons, fill on hover | Same dark variant. |
| `btn-ghost-primary` | `<button class="btn btn-ghost-primary">` | **CoreUI-only**: transparent border and background at rest, coloured text | Same hover as `btn-outline-primary`: solid fill with white text. Dist demo: `components/buttons.html`. Use it for secondary actions in a `card-header` or a table row. |
| `bg-primary` | `<div class="bg-primary text-white">` | Background only; you pick the text colour | `badge` already sets white text, which is why the dist's sidebar badge is `badge badge-sm bg-info` with no `text-white`; on a `div` you add it yourself. |
| `text-bg-primary` | `<span class="badge text-bg-primary">` | Background **and** a contrasting text colour in one class | Preferred over `bg-*` + `text-*`; also has a tuned dark-mode rule. |
| `text-primary` | `<span class="text-primary">` | Text colour only | CoreUIDemo's footer line and the `text-success`/`text-danger` status cell in `Views/Settings/UserAccounts.cshtml`. |
| `border-primary` | `<div class="card border-primary">` | Border colour (needs a border to exist: `border`, or a `card`) | Fixed hue in both modes. |
| `alert-primary` | `<div class="alert alert-primary">` | Alerts | Dist demo: `components/alerts.html`. |
| `badge` + `text-bg-primary` | `<span class="badge text-bg-primary">` | Badges — `badge` alone has no colour | `badge-sm` for the small sidebar/menu size. |
| `list-group-item-primary` | `<li class="list-group-item list-group-item-primary">` | Tinted list rows | Dist demo: `components/list-group.html`. |
| `table-primary` | `<tr class="table-primary">` | Tinted table, row or cell (`table`, `tr`, `td`) | Dist demo: `components/tables.html`. |

Choosing the colour from data is one `ng-class` with a ternary — the class string is computed, the `badge` base class stays static:

**`Views/Settings/UserAccounts.cshtml`** — a status badge whose colour follows the row
```html
<span class="badge" ng-class="acc.IsActive ? 'text-bg-success' : 'text-bg-danger'">{{ acc.IsActive ? 'Active' : 'Inactive' }}</span>
```

Today's `UserAccounts.cshtml` uses the same boolean on the plain-text cell (`ng-class="{'text-success': acc.IsActive, 'text-danger': !acc.IsActive}"`, line 55); the `text-bg-*` version above turns it into a pill instead. Any class in this table can be swapped the same way — `btn-outline-danger` for a delete button that becomes `btn-danger` once a confirmation is pending, `table-warning` on a `tr` whose record is locked.

## Page width

The content container in `div.body` decides how wide a page's grid can get:

| Class | Width | Who uses it |
|---|---|---|
| `container-lg` | 100% below 992px, then capped at 960 / 1140 / 1320px at the lg / xl / xxl breakpoints, centred | Dist `index.html` and `blank.html` (`<div class="container-lg px-4">`) |
| `container-xl` | 100% below 1200px, then capped at 1140 / 1320px | Nobody in the dist — the middle option |
| `container-fluid` | Always 100% of `.wrapper` (minus the sidebar) | **CoreUIDemo** — `<div class="container-fluid px-4">@RenderBody()</div>`, so grids use the whole width as OneMasaito's pages do |

`px-4` on all three is the 1.5rem side padding that lines the content up with the header row, which carries the same `px-4`.

Keep `_Layout.cshtml` on `container-fluid` and let a page that reads better narrow — a settings page, a profile page — cap itself by wrapping its own content instead. A container inside a container is fine: the inner one adds `max-width` and auto margins; `px-0` stops it adding a second gutter on top of the layout's `px-4`:

**`Views/Reports/Index.cshtml`** — per-page width, layout untouched (the same view built out in [Spacing and layout utilities](#spacing-and-layout-utilities-the-template-leans-on) below)
```html
@{
    ViewBag.Title = "Reports";
}

<div ng-controller="reportsController as vm" ng-init="Init()">
    <div class="container-lg px-0">
        <div class="card mb-4">
            …
        </div>
    </div>
</div>
```

## Spacing and layout utilities the template leans on

The dist pages are built from a handful of Bootstrap utilities, and copying the same ones is what makes a new CoreUIDemo view look like a dist page rather than a plain page. One example of each, from the file named.

| Utility | What it is for | Dist example |
|---|---|---|
| `d-flex` + `gap-*` | A row (or, with `flex-column`, a stack) of items with a fixed space between them, no margins on the children | `components/chip.html`: `<div class="d-flex flex-wrap gap-2">…chips…</div>`; `authentication/login.html` (and `Views/Home/Login.cshtml` line 31): `<div class="card-body d-flex flex-column gap-4">` |
| `mb-4` on `card` | 1.5rem below every card so stacked cards and the footer keep the same rhythm as the header's `mb-4` | `index.html`: `<div class="card mb-4">` (every content card); `Views/Home/Index.cshtml` and `Views/Settings/UserAccounts.cshtml` do the same |
| `row g-3` | A grid row whose columns are 1rem apart in both directions — the gutter the dist's forms pages use | `forms/layout.html` line 897: `<div class="row g-3"><div class="col"><input class="form-control" …></div><div class="col">…</div></div>` |
| `card-header d-flex justify-content-between align-items-center` | A card title on the left and an action on the right, vertically centred, without a nested grid | `index.html` line 949 uses the centred version (`card-header position-relative d-flex justify-content-center align-items-center`) for the social cards; the space-between version is the CRUD-page header below |

**`Views/Reports/Index.cshtml`** — the four together on a list page
```html
<div class="card mb-4">
    <div class="card-header d-flex justify-content-between align-items-center">
        <span>Reports</span>
        <button class="btn btn-sm btn-primary" type="button" ng-click="OpenReportModal()">
            <i class="cil-plus me-1"></i> New
        </button>
    </div>
    <div class="card-body">
        <div class="row g-3 mb-3">
            <div class="col-md-4"><input class="form-control" type="text" placeholder="Search" ng-model="vm.Search" /></div>
            <div class="col-md-3"><select class="form-select" ng-model="vm.Status"><option value="">All</option></select></div>
        </div>
        <div class="d-flex flex-wrap gap-2">
            <span class="badge text-bg-secondary" ng-repeat="t in vm.Tags">{{t}}</span>
        </div>
    </div>
</div>
```

`mb-3` under the filter row and `me-1` after the icon are the same spacing scale (`1` = 0.25rem, `2` = 0.5rem, `3` = 1rem, `4` = 1.5rem, `5` = 3rem); nearly all dist markup uses `2`, `3` and `4`.

## Turning things off

Each of these is a deletion in `Views/Shared/_Layout.cshtml`; nothing else in the dist depends on them except where noted.

| Region | How to remove it | What else changes |
|---|---|---|
| Breadcrumb row (header's second `container-fluid`) | Already removed in CoreUIDemo. To keep it out, do not add the optional block from [01 · breadcrumb](./01-layout.md#second-row-breadcrumb). | The header's `border-bottom` sits on the first row, so the header ends at the line — nothing to tidy. |
| Footer (`<footer class="footer px-4">`) | Delete the element. | `div.body.flex-grow-1` already fills `.wrapper` (`min-vh-100`), so short pages stay full height; `[data-coreui-theme=dark] .footer` becomes unused, harmlessly. |
| Theme dropdown (the `li.nav-item.dropdown` with `data-coreui-theme-value` items) | **Cannot be deleted on its own**: `color-modes.js` calls `showActiveTheme()` on `DOMContentLoaded` and `querySelector`s the `[data-coreui-theme-value]` button, so with no buttons every page throws a `TypeError`. Delete the dropdown *together with* the `<script src="~/Scripts/js/color-modes.js">` tag, and write `data-coreui-theme="light"` (or `"dark"`) on `<html>` — the full recipe is [`../COREUI_GUIDE.md` § 9](../COREUI_GUIDE.md#9-dark-mode). | The page is pinned to one mode; `localStorage` is no longer read. The `vr` divider next to the dropdown goes with it. |
| Sidebar footer toggler (`div.sidebar-footer` with `button.sidebar-toggler`) | Delete the `div.sidebar-footer`. | The sidebar stays at full width on desktop; the header's `header-toggler` still hides and shows it (`toggle()` adds `hide`, the wrapper follows). Narrow mode is then only reachable by putting `sidebar-narrow` on the `div.sidebar` permanently, as in [Sidebar variants](#sidebar-variants). Below 768px the bar was already hidden (`d-none d-md-flex`). |

The sidebar's mobile `btn-close` and the header's `header-toggler` are the two things not to remove: below 992px they are the only way to open and close the sidebar.
