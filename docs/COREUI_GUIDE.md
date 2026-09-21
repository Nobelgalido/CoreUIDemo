# CoreUIDemo — CoreUI v5.5.0 Implementation Guide

How **coreui-free-bootstrap-admin-template-v5.5.0-dist** is used in CoreUIDemo: which files are taken from the dist, where they live, how they are bundled, how the template's HTML shell becomes `_Layout.cshtml`, and the handful of rules that differ from the Bootstrap 4 / jQuery world OneMasaito's markup comes from.

This is the *why* and the *rules*. The *steps* — copy commands, full file listings, verify checks — are in `BUILD_GUIDE.md` § 7–§ 9 and are linked rather than repeated.

---

## 1. What the dist contains and what we use

The dist is a set of static HTML demo pages plus the compiled assets they share. Only the assets matter; the pages are reference material.

| Dist path | What it is | Used? |
|---|---|---|
| `css/style.css` (+ `.map`, `.min` variants) | The whole theme: Bootstrap 5.3 + CoreUI components (sidebar, header, etc.) + utilities, one file | **Yes** → `Content/build/css/style.css` |
| `css/examples.css` | Styles for the demo pages' code samples | No — demo only |
| `css/vendors/simplebar.css` | Tiny override for SimpleBar inside the sidebar | No — `style.css` already covers the sidebar scrollbar |
| `js/color-modes.js` | Light / dark / auto theme switcher, persisted in `localStorage` | **Yes** → `Scripts/js/color-modes.js` (loaded in `<head>`, not bundled) |
| `js/config.js` | Sets demo-only globals (`window.coreui = { ... }` config for the dist pages) | No |
| `js/main.js`, `js/charts.js`, `js/widgets.js`, `js/popovers.js`, `js/toasts.js`, `js/tooltips.js` | Demo-page scripts (fill the dashboard charts, wire example popovers…) | No |
| `vendors/@coreui/coreui/js/coreui.bundle.min.js` | CoreUI's JavaScript: Bootstrap 5 components under the `coreui` namespace, **Popper included** | **Yes** → `Content/vendor/@coreui/coreui/js/` |
| `vendors/@coreui/icons/css/free.min.css` + `fonts/CoreUI-Icons-Free.*` | CoreUI Icons Free as an icon font (`cil-*` classes) | **Yes** → `Content/vendor/@coreui/icons/{css,fonts}/` |
| `vendors/@coreui/icons/css/brand.min.css`, `flag.min.css` (+ fonts), `svg/` | Brand logos, flags, SVG sprite editions | No |
| `vendors/simplebar/{css,js}` | Custom scrollbar the sidebar uses (`data-simplebar` on `ul.sidebar-nav`) | **Yes** → `Content/vendor/simplebar/` |
| `vendors/chart.js`, `vendors/@coreui/chartjs`, `vendors/@coreui/utils` | Charts for the demo dashboard | No |
| `assets/brand/coreui.svg` | The CoreUI logo in the sidebar and login card | No — replaced by the Masaito SVGs from `..\MasaitoLogo\` (see below) |
| `assets/favicon/favicon-32x32.png` | Favicon | No — replaced by `Src/Image/masaito-mark-light-gradient.svg` (`rel="icon" type="image/svg+xml"`) |
| `assets/img/*`, `assets/icons/*` | Demo avatars, backgrounds, PWA icons | No |
| `index.html` | The dashboard — **the reference for `_Layout.cshtml`** (§ 5) | Read, not copied |
| `authentication/login.html` | **The reference for `Login.cshtml`** (§ 6) | Read, not copied |
| `authentication/*.html`, `components/`, `forms/`, `icons/`, `error-pages/`, `widgets.html`, `charts.html` | Markup samples for every component; `icons/coreui-icons-free.html` is the icon catalogue | Reference |

Copy commands: `BUILD_GUIDE.md` → [§ 7.1](BUILD_GUIDE.md#-7--vendoring-coreui-and-bundles).

## 2. Why hand-vendored, not NuGet or npm

OneMasaito hand-copies its theme: the compiled SB Admin 2 bundle lives in `Content/build/{css,js}/` and every plugin in `Content/vendor/<name>/`, all referenced by `BundleConfig` and all committed to the repo. CoreUIDemo does the same with CoreUI for the same reasons:

- The dist is **already compiled**. There is nothing to build, so npm adds a toolchain for no output.
- The `CoreUI` packages on NuGet ship the *library* (Bootstrap-with-CoreUI-components CSS/JS), not the *admin template* (`style.css` with the sidebar/header layout). The repo previously had those library files (`Content/coreui*.css`, `Scripts/coreui*.js`) copied in; they are removed in `BUILD_GUIDE.md` § 1 because the template's `style.css` supersedes them.
- OneMasaito also lists `bootstrap 5.3.3` in `packages.config` and never references it. CoreUI's `style.css` and `coreui.bundle.min.js` *contain* Bootstrap 5, so the package is removed rather than carried as dead weight.

## 3. Folder layout (mirrors OneMasaito)

```
Content/
  Site.css                                   ← .loader spinner (OneMasaito's sb-admin-2.css extra) + growl BS4 shim (§ 10)
  build/css/style.css                        ≙ OneMasaito Content/build/css/sb-admin-2.css
  vendor/@coreui/coreui/js/coreui.bundle.min.js   ≙ Content/vendor/bootstrap/js/bootstrap.bundle.min.js
  vendor/@coreui/icons/css/free.min.css      ≙ Content/vendor/fontawesome-free/css/all.min.css
  vendor/@coreui/icons/fonts/CoreUI-Icons-Free.{eot,svg,ttf,woff}
  vendor/simplebar/css/simplebar.css
  vendor/simplebar/js/simplebar.min.js
  vendor/growl/angular-growl.min.css         (same file as OneMasaito's)
Scripts/
  jquery-3.7.0.js / .min.js                  (NuGet jQuery — referenced as jquery-{version}.js in the bundle)
  angular.min.js                             (NuGet angularjs 1.8.2)
  angular-growl.min.js                       (copied from OneMasaito / angular-growl-v2 0.7.3)
  js/color-modes.js                          (dist js/color-modes.js)
Src/Image/
  masaito-logo-{light,dark}-gradient.svg     ≙ OneMasaito Src/Image/OMLogo*.png (full wordmark; light = dark ink for the login card, dark = white ink for the sidebar)
  masaito-mark-{light,dark}-gradient.svg     ≙ OneMasaito Src/Image/MDC.ico       (square mark; light = favicon, dark = collapsed sidebar brand)
```

The `fonts/` folder **must** stay beside `css/free.min.css`: the stylesheet references `../fonts/CoreUI-Icons-Free.woff`, and ASP.NET bundling rewrites that relative URL against the bundle's virtual path only when the original relative relationship holds.

## 4. Bundles

Three bundles, named exactly as OneMasaito's, registered in `App_Start/BundleConfig.cs` (full file: `BUILD_GUIDE.md` → [§ 7.4](BUILD_GUIDE.md#-7--vendoring-coreui-and-bundles)):

| Bundle | Contents, in order | Why this order |
|---|---|---|
| `~/Content/css` | `style.css` → `free.min.css` → `simplebar.css` → `angular-growl.min.css` → `Site.css` | Theme first; `Site.css` last so its rules win |
| `~/bundles/scripts` | `jquery-{version}.js` → `coreui.bundle.min.js` → `simplebar.min.js` → `angular.min.js` → `angular-growl.min.js` | jQuery is used only by OneMasaito's keypress filters; **CoreUI before Angular** so the `coreui` global exists when controllers run; `angular-growl` registers a module on `angular`, so Angular first |
| `~/bundles/angular` | `GrowlConfig.js` → `App.js` → `Login.js` → `UserAccounts.js` | `GrowlConfig.js` declares the `growlConfig` module (global growl TTLs) that both `app` and `login` list; `App.js` declares the root module the others depend on / are depended on by. Order inside this bundle does not matter to Angular (modules resolve at bootstrap), but keep it readable: shared config first |

Three rules that are easy to get wrong:

1. **A bundle's virtual path must not be a real folder.** `~/Content/css` is a bundle; if a physical `Content/css/` directory exists, IIS's static-file handler wins and the page is served unstyled with no error. `BUILD_GUIDE.md` § 1 deletes that folder for this reason.
2. **`color-modes.js` is not bundled.** It must run in `<head>` *before first paint*, otherwise a user who chose the dark theme sees a white flash on every navigation. `_Layout.cshtml` loads it with a plain `<script src="~/Scripts/js/color-modes.js">` between the styles and the script bundles.
3. **`~/bundles/scripts` must have `Transforms.Clear()` called on it.** `coreui.bundle.min.js` is output by a modern build toolchain that the ~2013-era `Microsoft.Ajax.Utilities` JS minifier (`System.Web.Optimization`'s default transform) cannot parse — it throws an unhandled `NullReferenceException` the first time anything requests the bundle URL, rather than falling back the way the CSS minifier does. This happens **regardless of `BundleTable.EnableOptimizations`**, which only controls whether `@Scripts.Render(...)` links to the bundle URL or to each file individually — not whether the transform pipeline runs when that URL is actually requested. Since every file already in `~/bundles/scripts` is pre-minified, skipping the transform loses nothing. `BUILD_GUIDE.md` § 7.4 shows the fix; Appendix A has the crash signature if you ever hit it.

Both bundles and `color-modes.js` render in `<head>`, as OneMasaito renders all of its scripts in `<head>`.

## 5. Layout anatomy — `index.html` → `_Layout.cshtml`

The dist's `index.html` is 2,270 lines; the *shell* is about 60 of them. This is the skeleton, with everything that becomes Razor/Angular annotated. Full file: `BUILD_GUIDE.md` → [§ 8.2](BUILD_GUIDE.md#82-viewsshared_layoutcshtml).

```html
<div class="sidebar sidebar-dark sidebar-fixed border-end" id="sidebar">
  <div class="sidebar-header border-bottom">
    <div class="sidebar-brand me-auto">  <!-- SVG logos → <img src="~/Src/Image/masaito-logo-dark-gradient.svg"> (.sidebar-brand-full, shown when expanded)
                                              and <img src="~/Src/Image/masaito-mark-dark-gradient.svg"> (.sidebar-brand-narrow, shown when collapsed) -->
    </div>
    <button class="btn-close d-lg-none" …>  <!-- mobile close; onclick uses coreui.Sidebar -->
  </div>
  <ul class="sidebar-nav" data-coreui="navigation" data-simplebar>
    <li class="nav-item"><a class="nav-link" href="…"><svg class="nav-icon">…</svg> Dashboard</a></li>
    <li class="nav-title">UI Elements</li>          <!-- → "Modules" -->
    <li class="nav-group">                          <!-- → Settings group, ng-show="main.CurrentUser.Role === 'admin'" -->
      <a class="nav-link nav-group-toggle" href="#">…</a>
      <ul class="nav-group-items compact">
        <li class="nav-item"><a class="nav-link" href="…">…</a></li>   <!-- → User Account -->
      </ul>
    </li>
  </ul>
  <div class="sidebar-footer border-top d-none d-md-flex">
    <button class="sidebar-toggler" type="button" data-coreui-toggle="unfoldable"></button>
  </div>
</div>

<div class="wrapper d-flex flex-column min-vh-100">
  <header class="header header-sticky p-0 mb-4">
    <div class="container-fluid border-bottom px-4">
      <button class="header-toggler" …>              <!-- kept; onclick → coreui.Sidebar toggle -->
      <ul class="header-nav ms-auto">…</ul>           <!-- three icon links → replaced by the search box (OneMasaito) -->
      <ul class="header-nav">
        <li class="nav-item dropdown">…theme…</li>    <!-- KEPT (see §9) -->
        <li class="nav-item py-1"><div class="vr …"></div></li>
        <li class="nav-item dropdown">…avatar…</li>   <!-- → user dropdown: name + Change Password + Logout -->
      </ul>
    </div>
    <div class="container-fluid px-4">…breadcrumb…</div>   <!-- dropped -->
  </header>
  <div class="body flex-grow-1">
    <div class="container-lg px-4">…dashboard…</div>  <!-- → container-fluid px-4 → @RenderBody() -->
  </div>
  <footer class="footer px-4">…</footer>               <!-- text replaced -->
</div>
```

| Dist region | In `_Layout.cshtml` |
|---|---|
| Everything above `div.sidebar` | OneMasaito's wrapper: `<div ng-controller="mainController as main" ng-init="Init()">`, the `.loader` div (`ng-hide="main.ItemLoad"`), a `<div ng-show="main.ItemLoad">` around the whole shell, `<div growl class="fading">` |
| Sidebar brand SVGs | Two `<img>` tags: `.sidebar-brand-full` → `~/Src/Image/masaito-logo-dark-gradient.svg`, `.sidebar-brand-narrow` → `~/Src/Image/masaito-mark-dark-gradient.svg` (the `-dark` variants are white ink for `sidebar-dark`) |
| Nav items | `Dashboard` (`/Home/Index`) and a `Settings` group with `User Account` (`/Settings/UserAccounts`), the group gated by `ng-show` |
| Header icon links | OneMasaito's search `input-group`, `ng-model="main.SearchBox"` |
| Theme dropdown | Kept, with text labels instead of SVG icons (§ 9) |
| Avatar dropdown | `{{main.CurrentUser.FirstName}} {{main.CurrentUser.LastName}}` + `cil-user`; items open `#PasswordModal` and `#logoutModal` |
| Breadcrumb row | Dropped |
| Body container | `container-fluid px-4` holding `@RenderBody()` |
| Footer | One line of text |
| After `div.wrapper` | OneMasaito's two modals (`#logoutModal`, `#PasswordModal`) in Bootstrap 5 markup |
| Script tags at the bottom | None — all scripts are in `<head>` via the bundles (§ 4) |

The dist toggles the sidebar with `coreui.Sidebar.getInstance(...)`. CoreUIDemo uses `getOrCreateInstance(...)` in both places so the call cannot return `null` if the data-API has not initialised the sidebar yet.

## 6. Login page — `authentication/login.html` → `Login.cshtml`

Full file: `BUILD_GUIDE.md` → [§ 8.3](BUILD_GUIDE.md#83-viewshomelogincshtml).

Kept from the dist: the outer `bg-body-tertiary min-vh-100 d-flex flex-row align-items-center` body, `div.container` at `max-width: 32rem`, the `d-flex flex-column gap-4` stack, the logo above the card, `card p-4` → `card-body d-flex flex-column gap-4`, the `h2.h5` title, the `form.row.gap-3` field layout, the `btn btn-primary w-100`.

Removed because OneMasaito's login has none of it: the email-type input, the show-password eye button and its tooltip, "I forgot password", "Remember me", the "or" divider, "Login with Google / Apple", "Need an account? Sign up".

Changed: the form carries `ng-submit="TryLogin()"` and the button is `type="submit"` (the form has no `action`, so `ngSubmit` prevents the default navigation and both the button and Enter call the controller — no jQuery `keypress` handler needed); inputs carry `ng-model="vm.Username"` / `vm.Password`; a `<div growl class="fading">` sits at the top of `<body>`; the version line at the bottom is OneMasaito's.

Why `Layout = null` and `ng-app="login"`: OneMasaito's login page is a self-contained HTML document with its own Angular module; it does not share the layout (there is no sidebar to show before you log in) and does not load `mainController` (which would immediately call `/Home/GetCurrentUser`). The same structure is kept.

Why `data-coreui-theme="light"` on `<html>`: this page does not load `color-modes.js`, so it does not follow the saved theme — see § 9 for how to change that.

## 7. JavaScript API rules (Bootstrap 4 + jQuery → CoreUI v5)

`coreui.bundle.min.js` is Bootstrap 5's JavaScript, forked and **renamed**: every component lives on the `coreui` global, every data attribute is prefixed `data-coreui-`, and there is **no jQuery plugin API**. OneMasaito's controllers do `$("#AccountModal").modal("show")`; that line throws `TypeError: $(...).modal is not a function` here.

| OneMasaito (BS4 + jQuery) | CoreUIDemo (CoreUI v5) |
|---|---|
| `$('#AccountModal').modal('show')` | `coreui.Modal.getOrCreateInstance(document.getElementById('AccountModal')).show()` — wrapped as the global `ShowModal('AccountModal')` in `App.js` |
| `$('#AccountModal').modal('hide')` | `…hide()` — `HideModal('AccountModal')` |
| `data-toggle="modal" data-target="#x"` | `data-coreui-toggle="modal" data-coreui-target="#x"` |
| `data-dismiss="modal"` | `data-coreui-dismiss="modal"` |
| `data-toggle="dropdown"` | `data-coreui-toggle="dropdown"` |
| `data-toggle="collapse" data-target="#x"` (sidebar groups) | Not needed — `ul.sidebar-nav[data-coreui="navigation"]` + `li.nav-group > a.nav-group-toggle` are wired by CoreUI's Navigation component |
| `$('#sidebarToggle').click(...)` + `body.sidebar-toggled` | `coreui.Sidebar.getOrCreateInstance(el).toggle()` on the header button; `button.sidebar-toggler[data-coreui-toggle="unfoldable"]` in the sidebar footer |
| `bootstrap.Tooltip` / `new bootstrap.Modal(...)` | `coreui.Tooltip` / `new coreui.Modal(...)` — `bootstrap.*` is `undefined` |
| Popper loaded separately | Included in `coreui.bundle.min.js` |

jQuery is still loaded (first in `~/bundles/scripts`) only because OneMasaito's `UserAccounts.js` uses `$("#firstName").keypress(...)` / `$('#lastName').keypress(...)` for the letters-only filters. (OneMasaito's `Login.js` also used `$(document).on('keypress', ...)` for Enter; CoreUIDemo replaces that with `ng-submit` on the form.) CoreUI itself never touches it.

Where the two modal helpers live and why they are global: `BUILD_GUIDE.md` → [§ 9.1](BUILD_GUIDE.md#91-appappjs).

## 8. Icons

CoreUI Icons Free, as a font. `free.min.css` defines one class per icon, `cil-<name>`; usage is `<i class="cil-pencil"></i>`. Inside the sidebar add `nav-icon` (`<i class="nav-icon cil-speedometer"></i>`) so it aligns and colours like the dist's SVG icons; in the header add `icon icon-lg`.

Icons used by CoreUIDemo (every one verified present in `free.min.css`):

| Where | Icon |
|---|---|
| Sidebar: Dashboard / Settings / User Account | `cil-speedometer` / `cil-settings` / `cil-people` |
| Header: toggler / search / user | `cil-menu` / `cil-search` / `cil-user` |
| User dropdown: Change Password / Logout | `cil-lock-locked` / `cil-account-logout` |
| Grid: New / Edit / Reset Password / Deactivate / Activate | `cil-plus` / `cil-pencil` / `cil-lock-locked` / `cil-ban` / `cil-check-circle` |

To find others, open `..\coreui-free-bootstrap-admin-template-v5.5.0-dist\icons\coreui-icons-free.html` in a browser — it lists every free icon with its class name. Font Awesome (`fas fa-*`, OneMasaito's icon set) is not vendored; if you prefer it, add `Content/vendor/fontawesome-free/` from OneMasaito to `~/Content/css` and swap the class names.

The grid's action buttons are CoreUI ghost buttons — `btn-ghost-secondary` for New / Edit / Reset Password, `btn-ghost-danger` for Deactivate, `btn-ghost-success` for Activate — with a bare `<i class="cil-*">` inside: no fill or border at rest, a light tint on hover. OneMasaito's solid `btn-success` / `btn-info` / `btn-warning` / `btn-danger` and its `icon-text-white-50` half-white icon helper are gone (a half-white icon is invisible on a transparent button). The same header cell carries the *Active | Inactive | All* `btn-group btn-group-sm` of `btn-outline-secondary` segments bound to `vm.StatusFilter`; the selected segment gets `active`.

## 9. Dark mode

`Scripts/js/color-modes.js` (dist `js/color-modes.js`, unmodified):

- Reads `localStorage['coreui-free-bootstrap-admin-template-theme']` (`light` / `dark` / `auto`), defaulting to `auto` (= the OS preference).
- Sets `data-coreui-theme="light|dark"` on `<html>`. `style.css` keys every colour off that attribute, so the whole page — including the `.sidebar-dark` sidebar and Bootstrap's `bg-body-*` / `text-body-*` classes — switches.
- On `DOMContentLoaded`, calls `showActiveTheme()`, which **requires** at least one element with `data-coreui-theme-value` (it does `btnToActive.querySelector(...)` on the match). No buttons → `TypeError` in the console on every page. This is why `_Layout.cshtml` keeps the theme dropdown. The dist's version has SVG icons inside the buttons and a `.theme-icon-active` SVG in the toggle; CoreUIDemo uses text labels — the script tolerates a missing SVG, just not a missing button.

The dropdown markup used in `_Layout.cshtml`:

```html
<li class="nav-item dropdown">
    <button class="btn btn-link nav-link py-2 px-2 d-flex align-items-center" type="button"
            aria-expanded="false" data-coreui-toggle="dropdown">
        <span class="theme-icon-active">Theme</span>
    </button>
    <ul class="dropdown-menu dropdown-menu-end" style="--cui-dropdown-min-width: 8rem">
        <li><button class="dropdown-item" type="button" data-coreui-theme-value="light">Light</button></li>
        <li><button class="dropdown-item" type="button" data-coreui-theme-value="dark">Dark</button></li>
        <li><button class="dropdown-item active" type="button" data-coreui-theme-value="auto">Auto</button></li>
    </ul>
</li>
```

Options:

- **No theme switching at all**: delete the dropdown *and* the `<script src="~/Scripts/js/color-modes.js">` tag, and write `<html ng-app="app" data-coreui-theme="light">`.
- **Login page follows the saved theme** (it is pinned to light by default): remove `data-coreui-theme="light"` from its `<html>` and add this one-liner as the first thing in its `<head>` — it applies the stored preference without needing the dropdown:

  ```html
  <script>
      (function () {
          var t = localStorage.getItem('coreui-free-bootstrap-admin-template-theme');
          if (t === 'dark' || (t !== 'light' && window.matchMedia('(prefers-color-scheme: dark)').matches)) {
              document.documentElement.setAttribute('data-coreui-theme', 'dark');
          }
      })();
  </script>
  ```

## 10. Bootstrap 4 → Bootstrap 5 class renames

OneMasaito's markup is Bootstrap 4. Every place its HTML was ported into CoreUIDemo's views used these substitutions; use the same table when porting anything else from OneMasaito.

| Bootstrap 4 (OneMasaito) | Bootstrap 5 (CoreUI) |
|---|---|
| `form-group` | `mb-3` |
| `<select class="form-control">` | `<select class="form-select">` |
| `custom-control custom-checkbox` / `custom-radio` | `form-check` |
| `custom-control-input` / `custom-control-label` | `form-check-input` / `form-check-label` |
| `input-group-append` / `input-group-prepend` wrappers | Removed — put `input-group-text` / buttons directly inside `input-group` |
| `<button class="close">&times;</button>` | `<button class="btn-close" aria-label="Close"></button>` (no inner `&times;`) |
| `ml-*` / `mr-*` | `ms-*` / `me-*` |
| `pl-*` / `pr-*` | `ps-*` / `pe-*` |
| `float-left` / `float-right` | `float-start` / `float-end` |
| `text-left` / `text-right` | `text-start` / `text-end` |
| `dropdown-menu-right` | `dropdown-menu-end` |
| `badge-primary` etc. | `bg-primary` (with `badge`) |
| `font-weight-bold` | `fw-bold` |
| `font-italic` | `fst-italic` |
| `sr-only` | `visually-hidden` |
| `no-gutters` | `g-0` |
| `text-gray-800` / `text-gray-600` (SB Admin 2) | `text-body` / `text-body-secondary` |
| `bg-white` (when it should follow the theme) | `bg-body` |
| `btn-block` | `w-100` (or wrap in `d-grid`) |
| `jumbotron`, `media`, `form-row`, `form-inline` | Removed in BS5 — use utilities (`p-5 bg-body-tertiary rounded`, `d-flex`, `row g-3`, `d-flex align-items-center gap-2`) |

**Third-party markup you cannot rename — angular-growl.** Its template (inside `angular-growl.min.js`) still emits Bootstrap 4's `<button class="close">` and adds an `icon` class to every notification; you cannot apply the table above to it. Two CoreUI collisions follow: `.close` no longer exists, and CoreUI's `style.css` defines `.icon` (for CoreUI Icons) as a 1rem inline-block, which shrinks each growl to a ~47 px box. `Content/Site.css` carries a shim scoped to `.growl-container > .growl-item` that restores `display: block`, the alert text colour (`var(--cui-alert-color)`, since `.icon { color: inherit }` otherwise wins) and a Bootstrap-4-style `.close`. Keep that shim if you ever restyle `Site.css`; if you drop angular-growl for CoreUI Toasts, delete it.
| `data-*` behaviour attributes | `data-coreui-*` (§ 7) |
