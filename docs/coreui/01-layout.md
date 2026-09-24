# 01 · Layout

The sidebar, header, body and footer shell that every dist page — and every CoreUIDemo view that keeps `_Layout.cshtml` — renders inside. *Installing* the shell is already covered: [`../COREUI_GUIDE.md` § 5](../COREUI_GUIDE.md#5-layout-anatomy--indexhtml--_layoutcshtml) shows what became Razor/Angular and [§ 7](../COREUI_GUIDE.md#7-javascript-api-rules-bootstrap-4--jquery--coreui-v5) sets the JavaScript API rules. This chapter is about *using* it: what each region does, the markup and behaviour it needs, how CoreUIDemo drives it from `mainController`, and — at the end — the complete recipe for adding a page to it. Entries follow the five-part shape in [README · How to read an entry](./README.md#how-to-read-an-entry).

- [The shell at a glance](#the-shell-at-a-glance)
- [Sidebar](#sidebar) — [brand and header](#brand-and-sidebar-header) · [navigation item](#navigation-item) · [section title](#section-title) · [navigation group](#navigation-group) · [mobile close button](#mobile-close-button) · [toggler and footer](#toggler-and-sidebar-footer)
- [Header](#header) — [toggler](#header-toggler) · [notification links](#notification-links-demo) · [theme dropdown](#theme-dropdown) · [user dropdown](#user-dropdown) · [vertical divider](#vertical-divider) · [breadcrumb](#second-row-breadcrumb)
- [Body and footer](#body-and-footer)
- [blank.html](#blankhtml)
- [Worked example — adding a "Reports" page](#worked-example--adding-a-reports-page)
- [Gotchas](#gotchas)

## The shell at a glance

Dist pages: `index.html` (the dashboard — the shell is lines 49–733 and 2240–2254 of 2,272; everything between is dashboard content) and `blank.html` (the same shell with an empty body).

```text
body
├── div.sidebar.sidebar-dark.sidebar-fixed.border-end#sidebar
│   ├── div.sidebar-header
│   │   ├── div.sidebar-brand            logo, full + narrow versions
│   │   └── button.btn-close             mobile only
│   ├── ul.sidebar-nav[data-coreui="navigation"][data-simplebar]
│   │   ├── li.nav-item > a.nav-link
│   │   ├── li.nav-title
│   │   └── li.nav-group > a.nav-link.nav-group-toggle + ul.nav-group-items
│   └── div.sidebar-footer > button.sidebar-toggler        dist only — removed from _Layout.cshtml on 2026-09-22
└── div.wrapper.d-flex.flex-column.min-vh-100
    ├── header.header.header-sticky
    │   ├── div.container-fluid          toggler · header-nav · header-nav
    │   └── div.container-fluid          breadcrumb
    ├── div.body.flex-grow-1 > div.container-lg.px-4     page content
    └── footer.footer
```

Two siblings: the sidebar, and a wrapper that holds everything else. `sidebar-fixed` takes the sidebar out of the document flow; `.wrapper` pads its inline-start by a custom property that `style.css` sets from the sidebar's own state classes, so the content shifts when the sidebar narrows or hides with no layout JavaScript. `header-sticky` pins the header to the top of the scrolling wrapper; `flex-grow-1` on `div.body` pushes the footer to the bottom of short pages. Both `coreui.Sidebar` (for every `.sidebar`) and `coreui.Navigation` (for every `[data-coreui="navigation"]`) initialise themselves on the window `load` event — nothing has to be constructed by hand.

In CoreUIDemo the whole shell is wrapped by `<div ng-controller="mainController as main" ng-init="Init()">` and sits inside `<div ng-show="main.ItemLoad">`, so it stays hidden until `/Home/GetCurrentUser` has answered and `main.CurrentUser` is populated (the region-by-region mapping is the table in [§ 5](../COREUI_GUIDE.md#5-layout-anatomy--indexhtml--_layoutcshtml)). Every `ng-*` binding in the shell therefore reads `main.*`; each page view gets its own controller (`vm`) inside `@RenderBody()`.

## Sidebar

### Brand and sidebar header

**1. What the dist page shows.** A dark strip at the top of the sidebar with the CoreUI logo. When the sidebar is expanded it shows the wide logo; when it is narrowed (footer toggler, see [below](#toggler-and-sidebar-footer)) it shows the square signet. Dist: `index.html` lines 50–70.

**2. Essential markup.**

**`index.html`**
```html
<div class="sidebar-header border-bottom">
  <div class="sidebar-brand me-auto">                        <!-- me-auto pushes the mobile close button right -->
    <svg class="sidebar-brand-full" …>…</svg>                 <!-- shown while expanded -->
    <svg class="sidebar-brand-narrow" …>…</svg>               <!-- shown while narrow / unfoldable-not-hovered -->
  </div>
  <button class="btn-close d-lg-none" type="button" …></button>   <!-- see "Mobile close button" -->
</div>
```

**3. Behaviour.** CSS only. `style.css` hides `.sidebar-brand-narrow` by default; at ≥ 992px, when the sidebar carries `sidebar-narrow`, or carries `sidebar-narrow-unfoldable` and is not hovered, the rule flips (`-full` hidden, `-narrow` shown). Both elements are always in the DOM. The other sidebar variant classes are tabulated in [08 · Customizing](./08-customizing.md).

**4. In CoreUIDemo.** The two SVGs become two `<img>` tags at a fixed height, pointing at the company's full and mark logos:

**`Views/Shared/_Layout.cshtml`**
```html
<div class="sidebar-brand me-auto">
    <img class="sidebar-brand-full" style="height:32px;" src="~/Src/Image/masaito-logo-dark-gradient.svg" alt="Masaito Development Corporation" />
    <img class="sidebar-brand-narrow" style="height:32px;" src="~/Src/Image/masaito-mark-dark-gradient.svg" alt="Masaito" />
</div>
```

**5. Gotchas.** Keep both images. One wide logo with only `sidebar-brand-full` disappears entirely in narrow mode (the rule hides it and there is nothing to show); one wide logo used for both is clipped to the 4rem narrow width.

### Navigation item

**1. What the dist page shows.** A link with an icon and a label, optionally a badge on the right (`Dashboard` has a `NEW` badge). Dist: `index.html` lines 72–81.

**2. Essential markup.**

**`index.html`**
```html
<ul class="sidebar-nav" data-coreui="navigation" data-simplebar>     <!-- navigation = CoreUI's Navigation component; simplebar = thin scrollbar -->
  <li class="nav-item">
    <a class="nav-link" href="index.html">
      <svg class="nav-icon" …>…</svg>                                  <!-- nav-icon: fixed width + colour that follows active/hover -->
      Dashboard
      <span class="badge badge-sm bg-info ms-auto">NEW</span>         <!-- optional; ms-auto pushes it to the right edge -->
    </a>
  </li>
</ul>
```

**3. Behaviour.** `data-coreui="navigation"` on the `ul`. On window `load` the Navigation component reads every `a.nav-link`, compares its `href` (as the browser resolves it) with the page URL — query string and hash stripped, exact match by default (`activeLinksExact: true`) — and adds `active` to the match plus `show` to every `nav-group` enclosing it. None of the dist pages marks an item active by hand; the highlight you see is this comparison. `data-simplebar` replaces the native scrollbar of the nav when it overflows; it needs `simplebar.min.js` and `simplebar.css`, which are in `~/bundles/scripts` and `~/Content/css`.

**4. In CoreUIDemo.** The inline SVG becomes an icon font `<i>` tag with the same `nav-icon` class (icon names: [§ 8](../COREUI_GUIDE.md#8-icons)); the `href` is the MVC route:

**`Views/Shared/_Layout.cshtml`**
```html
<li class="nav-item">
    <a class="nav-link" href="/Home/Index">
        <i class="nav-icon cil-speedometer"></i>
        Dashboard
    </a>
</li>
```

Two Angular additions the dist has no equivalent for:

*Gating by user.* `ng-show="main.CurrentUser.<Field> …"` on the `li` hides an item from users who should not see it. Before `GetCurrentUser` answers, `main.CurrentUser` is `undefined`, the expression is falsy and the item is hidden — that is fine, because the whole shell is behind `ng-show="main.ItemLoad"` at that point anyway. The layout does this on the Settings group (next entry).

*Active item.* The Navigation component's exact-match rule highlights `/Reports/Index` when the address bar says `/Reports/Index`, but not when the same action is reached as `/Reports` (default action) or `/` (the route default), and not for a detail page like `/Reports/Detail/5` whose sidebar parent you still want lit. `$location.path()` cannot be used for this — under MVC routing without html5Mode Angular's `$location` only sees the hash. Instead expose the real path from `mainController` and bind `ng-class`:

**`App/App.js`** — optional addition, one line at the top of `Init`
```js
        $scope.Init = function () {
            main.Path = window.location.pathname;   // e.g. "/Reports/Index"

            main.ItemLoad = false;
            // … unchanged
```

**`Views/Shared/_Layout.cshtml`**
```html
<a class="nav-link" href="/Reports/Index" ng-class="{active: main.Path === '/Reports/Index' || main.Path === '/Reports'}">
```

Angular adds `active` when the expression is true and never removes a class it did not add, so the two mechanisms do not fight; `main.Path` is set once per full page load, which is the only kind of navigation an MVC site does.

### Section title

**1. What the dist page shows.** Grey uppercase headings (`UI Elements`, `Extras`) between groups of items. Dist: `index.html` line 82.

**2. Essential markup.**

**`index.html`**
```html
<li class="nav-title">UI Elements</li>     <!-- a plain li; not a link -->
```

**3. Behaviour.** CSS only. In narrow mode `style.css` hides it.

**4. In CoreUIDemo.** `<li class="nav-title">Modules</li>` — identical markup; gate it with `ng-show` if every item under it is gated, otherwise a heading with nothing beneath it is left on screen.

### Navigation group

**1. What the dist page shows.** A collapsible parent (`Components`, `Forms`, `Icons`, `Authentication`) whose children are bullet items; clicking the parent slides the children open and closes any other open group. Dist: `index.html` lines 92–255.

**2. Essential markup.**

**`index.html`**
```html
<li class="nav-group">                                             <!-- add "show" to render it open on load -->
  <a class="nav-link nav-group-toggle" href="#">                   <!-- nav-group-toggle: the chevron (::after) and the click target -->
    <svg class="nav-icon" …>…</svg>
    Components
  </a>
  <ul class="nav-group-items compact">                             <!-- compact: shorter rows for the children -->
    <li class="nav-item">
      <a class="nav-link" href="components/accordion.html">
        <span class="nav-icon"><span class="nav-icon-bullet"></span></span>   <!-- bullet instead of an icon -->
        Accordion
      </a>
    </li>
  </ul>
</li>
```

**3. Behaviour.** `data-coreui="navigation"` on the enclosing `ul.sidebar-nav` — the same component as the previous entry. It handles the click on `a.nav-group-toggle`: toggles `show` on the parent `li.nav-group` (the children's `height` is animated), and with the default `groupsAutoCollapse: true` closes sibling groups. On load it also opens the group that contains the active link. Nothing else is needed — in particular **no** `data-coreui-toggle="collapse"` on the toggle (see [Gotchas](#gotchas)). The dist nests two levels once (`Authentication › Forgot password`, `index.html` ~494–519): the Navigation component opens every enclosing group of the active link, so it works, but `style.css` indents every depth by the same amount (only one `.sidebar-nav .nav-group-items .nav-link` indent rule), so the second level is not visually distinct — stay at one level.

**4. In CoreUIDemo.** The Settings group, visible to administrators only, with an icon instead of the bullet on its child:

**`Views/Shared/_Layout.cshtml`**
```html
<li class="nav-group" ng-show="main.CurrentUser.Role === 'admin'">
    <a class="nav-link nav-group-toggle" href="#">
        <i class="nav-icon cil-settings"></i>
        Settings
    </a>
    <ul class="nav-group-items compact">
        <li class="nav-item">
            <a class="nav-link" href="/Settings/UserAccounts">
                <i class="nav-icon cil-people"></i>
                User Account
            </a>
        </li>
    </ul>
</li>
```

**5. Gotchas.** The toggle needs *an* `href` (any value) so it looks and focuses like a link; the component calls `preventDefault` on click, so it never navigates and the target does not have to be `#`. A group whose children are all hidden by `ng-show` still shows its toggle and opens onto nothing — gate the group, not each child.

### Mobile close button

**1. What the dist page shows.** Below 992px the sidebar is off-canvas; the header toggler slides it in over a backdrop, and an × in the sidebar header (or a tap on the backdrop) slides it out. Dist: `index.html` line 69.

**2. Essential markup.**

**`index.html`**
```html
<button class="btn-close d-lg-none" type="button" data-coreui-theme="dark" aria-label="Close"
        onclick="coreui.Sidebar.getInstance(document.querySelector('#sidebar')).toggle()"></button>
<!-- d-lg-none: only exists on mobile; data-coreui-theme="dark": scopes the dark colour mode to the button so the × is white on the dark sidebar -->
```

**3. Behaviour.** `coreui.Sidebar` — `toggle()` calls `show()` or `hide()` depending on whether the sidebar is currently inside the viewport. The component decides "mobile" by reading a custom property that `style.css` sets on `.sidebar` under 992px: on mobile `show()` adds `show` plus a `sidebar-backdrop` and a click-outside listener; on desktop `hide()` adds `hide`, which `style.css` turns into a negative margin, and `.wrapper` follows. There is one Sidebar instance per `.sidebar`, created on `load`.

**4. In CoreUIDemo.** Same markup with one change: `getOrCreateInstance` instead of `getInstance`, here and on the header toggler, so the call cannot return `null` when something triggers it before the data-API's `load` handler has run (the reason is the last paragraph of [§ 5](../COREUI_GUIDE.md#5-layout-anatomy--indexhtml--_layoutcshtml)).

### Toggler and sidebar footer

**1. What the dist page shows.** A thin bar at the bottom of the sidebar with a chevron button that collapses the sidebar to icon width; hovering the collapsed sidebar expands it temporarily. Dist: `index.html` lines 568–570.

**2. Essential markup.**

**`index.html`**
```html
<div class="sidebar-footer border-top d-none d-md-flex">           <!-- hidden below 768px (d-none d-md-flex); the close button (d-lg-none) is shown below 992px, so both exist together between 768–992px -->
  <button class="sidebar-toggler" type="button" data-coreui-toggle="unfoldable"></button>
</div>
```

**3. Behaviour.** `coreui.Sidebar` again: the instance listens for clicks on any `[data-coreui-toggle]` inside the sidebar. `unfoldable` toggles the `sidebar-narrow-unfoldable` class (4rem wide, expands on hover); `narrow` toggles `sidebar-narrow` (4rem wide, stays narrow). Both are ignored on mobile. Which one to start with, and the other width/position variants, is the table in [08 · Customizing](./08-customizing.md).

**4. In CoreUIDemo. Removed on 2026-09-22.** `_Layout.cshtml` has no `div.sidebar-footer` and no `button.sidebar-toggler`: the `ul.sidebar-nav` is the last child of `div.sidebar`. The sidebar is therefore always full width on desktop, and the header's `header-toggler` (hide / show) is the only sidebar control left — nothing else changes, because the toggler was pure markup plus `coreui.Sidebar`'s own delegated listener. To get narrow mode back, either paste the block above back in as the last child of the sidebar, or put `sidebar-narrow` / `sidebar-narrow-unfoldable` on `div.sidebar` permanently ([08 · Sidebar variants](./08-customizing.md#sidebar-variants)).

## Header

### Header toggler

**1. What the dist page shows.** The hamburger at the left of the header. On desktop it hides/shows the sidebar; on mobile it slides it in. Dist: `index.html` lines 575–579.

**2. Essential markup.**

**`index.html`**
```html
<header class="header header-sticky p-0 mb-4">
  <div class="container-fluid border-bottom px-4">                  <!-- the header's first row; px-4 lines it up with the body -->
    <button class="header-toggler" type="button" style="margin-inline-start: -14px"
            onclick="coreui.Sidebar.getInstance(document.querySelector('#sidebar')).toggle()">
      <svg class="icon icon-lg" …>…</svg>
    </button>
```

**3. Behaviour.** `coreui.Sidebar` — the same `toggle()` as the mobile close button. The dist also has an inline script at the bottom of each page that adds `shadow-sm` to `header.header` once the page is scrolled; CoreUIDemo does not carry it over (all scripts are in `<head>`), and the header still sticks without it.

**4. In CoreUIDemo.** `<i class="icon icon-lg cil-menu"></i>` for the SVG and the same `getOrCreateInstance` call as the mobile close button — otherwise identical.

### Notification links (demo)

**1. What the dist page shows.** Three links that are only an icon (bell, list, envelope) pushed to the right — placeholders for notifications, tasks and messages. In v5.5.0 they are plain `href="#"` anchors with no dropdown behind them: **demo — replace or drop**. Dist: `index.html` lines 580–602.

**2. Essential markup.**

**`index.html`**
```html
<ul class="header-nav ms-auto">                                    <!-- ms-auto: this list and everything after it go right -->
  <li class="nav-item">
    <a class="nav-link" href="#">
      <svg class="icon icon-lg" …>…</svg>                            <!-- icon icon-lg: header icon size; nav-icon is for the sidebar -->
    </a>
  </li>
</ul>
```

**3. Behaviour.** CSS only. `header-nav .nav-link` gives the hover/active colours; `icon icon-lg` sizes the glyph.

**4. In CoreUIDemo.** Dropped. OneMasaito has a search box where the dist has these links, so the layout puts its `input-group` right after the toggler and moves `ms-auto` to the remaining `header-nav`:

**`Views/Shared/_Layout.cshtml`**
```html
<div class="input-group ms-3" style="max-width:500px;">
    <input type="text" class="form-control" placeholder="Search for..." aria-label="Search" ng-model="main.SearchBox">
    <span class="input-group-text"><i class="cil-search"></i></span>
</div>

<ul class="header-nav ms-auto">
    …theme dropdown, divider, user dropdown…
</ul>
```

`main.SearchBox` is declared in `mainController` (`App/App.js`) and is read by page controllers, as OneMasaito does; nothing in the shell acts on it. If you do want an icon link in the header, `<a class="nav-link" href="…"><i class="icon icon-lg cil-bell"></i></a>` inside a `li.nav-item` is the whole pattern.

### Theme dropdown

**1. What the dist page shows.** A sun/moon button opening Light / Dark / Auto. Dist: `index.html` lines 606–640.

**2–3. Markup and behaviour.** `data-coreui-toggle="dropdown"` on the button (CoreUI's Dropdown component, wired by the data-API) plus `data-coreui-theme-value="light|dark|auto"` on the items, read by `js/color-modes.js`. Both the trimmed markup and why the dropdown must stay even if you do not want theme switching are in [§ 9](../COREUI_GUIDE.md#9-dark-mode) — not repeated here.

**4. In CoreUIDemo.** Kept, with text labels instead of the SVGs ([§ 9](../COREUI_GUIDE.md#9-dark-mode)).

### User dropdown

**1. What the dist page shows.** A round user photo at the far right; clicking it opens a menu with an `Account` header, badge-annotated items, a `Settings` header, a divider and `Logout`. Dist: `index.html` lines 644–720.

**2. Essential markup.**

**`index.html`**
```html
<li class="nav-item dropdown">
  <a class="nav-link py-0 pe-0" data-coreui-toggle="dropdown" href="#" role="button" aria-haspopup="true" aria-expanded="false">
    <div class="avatar avatar-md"><img class="avatar-img" src="assets/img/avatars/8.jpg" alt="user@email.com"></div>
  </a>
  <div class="dropdown-menu dropdown-menu-end pt-0">                 <!-- dropdown-menu-end: right-aligned under the trigger -->
    <div class="dropdown-header bg-body-tertiary text-body-secondary fw-semibold rounded-top mb-2">Account</div>
    <a class="dropdown-item" href="#"><svg class="icon me-2" …>…</svg> Updates <span class="badge badge-sm bg-info ms-2">8</span></a>
    <div class="dropdown-divider"></div>
    <a class="dropdown-item" href="/authentication/login.html">…Logout</a>
  </div>
</li>
```

**3. Behaviour.** `data-coreui-toggle="dropdown"` — CoreUI's Dropdown component via the data-API; no call needed.

**4. In CoreUIDemo.** The photo becomes the user's name (`{{main.CurrentUser.FirstName}} {{main.CurrentUser.LastName}}`, hidden below lg) plus a `cil-user` glyph; the menu has two items that open the layout's modals. The `ng-click` on the first one resets the modal's fields before it opens:

**`Views/Shared/_Layout.cshtml`**
```html
<li class="nav-item dropdown">
    <a class="nav-link py-0 pe-0 d-flex align-items-center" data-coreui-toggle="dropdown" href="#" role="button"
       aria-haspopup="true" aria-expanded="false">
        <span class="me-2 d-none d-lg-inline text-body-secondary small">{{main.CurrentUser.FirstName}} {{main.CurrentUser.LastName}}</span>
        <i class="icon icon-lg cil-user"></i>
    </a>
    <div class="dropdown-menu dropdown-menu-end pt-0">
        <div class="dropdown-header bg-body-tertiary text-body-secondary fw-semibold rounded-top mb-2">Account</div>
        <a class="dropdown-item" href="#" data-coreui-toggle="modal" data-coreui-target="#PasswordModal" ng-click="OpenPasswordModal()">
            <i class="cil-lock-locked me-2"></i>
            Change Password
        </a>
        <div class="dropdown-divider"></div>
        <a class="dropdown-item" href="#" data-coreui-toggle="modal" data-coreui-target="#logoutModal">
            <i class="cil-account-logout me-2"></i>
            Logout
        </a>
    </div>
</li>
```

**5. Gotchas.** An item that opens a modal declaratively (`data-coreui-toggle="modal" data-coreui-target="#id"`) and also has an `ng-click` runs both: the modal opens through the data-API and the Angular handler runs independently. Use `ng-click` alone with `ShowModal('id')` when the handler has to decide *whether* to open it.

### Vertical divider

**1. What the dist page shows.** A thin vertical rule between the icon links and the theme dropdown, and another between the theme dropdown and the avatar. Dist: `index.html` lines 604–606.

**2. Essential markup.**

**`index.html`**
```html
<li class="nav-item py-1">
  <div class="vr h-100 mx-2 text-body text-opacity-75"></div>       <!-- vr = Bootstrap's vertical rule; py-1 keeps it short of the row edges -->
</li>
```

**3. Behaviour.** CSS only. **4. In CoreUIDemo.** Kept, once, between the theme and user dropdowns.

### Second row: breadcrumb

**1. What the dist page shows.** A second header row, `Home / Dashboard`, under the border. Dist: `index.html` lines 723–731.

**2. Essential markup.**

**`index.html`**
```html
<div class="container-fluid px-4">                                  <!-- second row of header.header; no border-bottom -->
  <nav aria-label="breadcrumb">
    <ol class="breadcrumb my-0">                                     <!-- my-0: the header's padding is enough -->
      <li class="breadcrumb-item"><a href="#">Home</a></li>
      <li class="breadcrumb-item active"><span>Dashboard</span></li>
    </ol>
  </nav>
</div>
```

**3. Behaviour.** CSS only (the separator is `breadcrumb-item::before`).

**4. In CoreUIDemo.** Dropped from `_Layout.cshtml`. To have it, put the row back after the first `container-fluid` and let each view name itself through `ViewBag.Title` — every view already sets it (the worked example below does too), and `<title>` is static so nothing else reads it:

**`Views/Shared/_Layout.cshtml`** — optional addition after the first `container-fluid` inside `header`
```html
<div class="container-fluid px-4">
    <nav aria-label="breadcrumb">
        <ol class="breadcrumb my-0">
            <li class="breadcrumb-item"><a href="/Home/Index">Home</a></li>
            <li class="breadcrumb-item active"><span>@ViewBag.Title</span></li>
        </ol>
    </nav>
</div>
```

## Body and footer

**1. What the dist page shows.** The dashboard's cards and tables, and a one-line footer with links left and "Powered by" right. Dist: `index.html` lines 732–733 and 2242–2254; `blank.html` lines 730–743.

**2. Essential markup.**

**`blank.html`**
```html
<div class="body flex-grow-1">                                      <!-- flex-grow-1: fills the wrapper so the footer sits at the bottom -->
  <div class="container-lg px-4"></div>                              <!-- page content goes here; container-lg caps the width at the lg breakpoint -->
</div>
<footer class="footer px-4">                                        <!-- footer: flex row, space-between, tertiary background, top border -->
  <div>
    <a href="https://coreui.io">CoreUI</a>
    <a href="https://coreui.io/product/free-bootstrap-admin-template/">Bootstrap Admin Template</a>
    &copy; 2026 creativeLabs.
  </div>
  <div class="ms-auto">Powered by&nbsp;<a href="https://coreui.io/bootstrap/docs/">CoreUI UI Components</a></div>
</footer>
```

**3. Behaviour.** CSS only.

**4. In CoreUIDemo.** The content container is `container-fluid px-4`, not `container-lg`, so grids use the full width of wide screens as OneMasaito's pages do; it holds `@RenderBody()`. The footer keeps `footer px-4` and one `div` of text:

**`Views/Shared/_Layout.cshtml`**
```html
<div class="body flex-grow-1">
    <div class="container-fluid px-4">
        @RenderBody()
    </div>
</div>

<footer class="footer px-4">
    <div><span class="text-primary"><b>&copy;2026 Management Information System.</b></span></div>
</footer>
```

The two modals the shell owns (`#logoutModal`, `#PasswordModal`) come after `div.wrapper`, still inside the `mainController` div, so `main.ChangePassword` and `Logout()` resolve.

**5. Gotchas.** Views set `ViewBag.Title` but must not set `Layout` — `Views/_ViewStart.cshtml` already points every view at `_Layout.cshtml`; only `Login.cshtml` overrides it with `Layout = null` (chapter 02).

## blank.html

`blank.html` is `index.html` with the dashboard removed: the same sidebar and header (its lines 48–729), then `<div class="container-lg px-4"></div>` and the footer. It is the dist's own answer to "what do I copy to start a page": open it next to a component demo, paste the component into the empty container, and everything already lines up with the shell.

In CoreUIDemo the shell is `_Layout.cshtml`, so the equivalent of "start from `blank.html`" is a view that contains only its own content. The worked example below is exactly that.

## Worked example — adding a "Reports" page

Goal: `/Reports/Index` renders inside the shell, has a sidebar item, and lists reports fetched from the server through the project's AngularJS pattern. Six files, in dependency order. Create the new ones from Visual Studio (**Add → New Item** / **Add → Controller**) so they are added to `CoreUIDemo.csproj`; a file created on disk any other way must be included by hand (Solution Explorer → **Show All Files** → **Include In Project**), or — for a `.cs` file — it is not compiled and not deployed; `.cshtml` and `.js` files are served from disk regardless of csproj membership under local IIS Express/F5 (see [`../BUILD_GUIDE.md` § 1](../BUILD_GUIDE.md#-1--repository-state-and-cleanup)).

**`Controllers/ReportsController.cs`**
```csharp
using System.Web.Mvc;

namespace CoreUIDemo.Controllers
{
    public class ReportsController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public JsonResult GetReports()
        {
            return Json(new
            {
                reportList = new[] { new { Id = 1, Name = "Monthly" } }
            });
        }
    }
}
```

`GetReports` returns `{ reportList }` — the property name is what the controller script reads. OneMasaito's convention adds the `UniversalHelpers.CurrentUser` null-check-and-redirect to every GET view action, exactly as `HomeController.Index()` does; add it once the page is wired up.

**`Views/Reports/Index.cshtml`**
```html
@{
    ViewBag.Title = "Reports";
}

<div ng-controller="reportsController as vm" ng-init="Init()">
    <div class="card mb-4">
        <div class="card-header">Reports</div>
        <div class="card-body">
            <ul class="list-group">
                <li class="list-group-item" ng-repeat="r in vm.ReportList">{{r.Name}}</li>
            </ul>
        </div>
    </div>
</div>
```

No `Layout`, no `<html>`, no scripts: `_ViewStart.cshtml` supplies the layout and the bundles are already in `<head>`. The `ng-controller` div nests inside the layout's `mainController` div, so `main.*` is still reachable from here if a page ever needs it.

**`App/Controller/Reports.js`**
```js
angular.module("reports", ["app"])

    .controller("reportsController", function ($scope, $location, $http, growl) {
        var vm = this;

        vm.ReportList = [];

        $scope.Init = function () {
            $http({
                method: "POST",
                url: "/Reports/GetReports"
            }).then(function (data) {
                vm.ReportList = data.data.reportList;
            });
        };
    });
```

**`App/App.js`** — add `"reports"` to the root module's dependencies
```js
var app = angular.module('app', ["angular-growl", "login", "useraccount", "reports"])
```

The page module depends on `app` and `app` depends on the page module; AngularJS resolves the cycle as long as both files are loaded before the app bootstraps, which the bundle guarantees.

**`App_Start/BundleConfig.cs`** — add the file to the Angular bundle
```csharp
            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                "~/App/App.js",
                "~/App/Controller/Login.js",
                "~/App/Controller/UserAccounts.js",
                "~/App/Controller/Reports.js"
                ));
```

**`Views/Shared/_Layout.cshtml`** — the sidebar item, after the Dashboard item
```html
<li class="nav-item">
    <a class="nav-link" href="/Reports/Index">
        <i class="nav-icon cil-chart"></i>
        Reports
    </a>
</li>
```

`cil-chart` is one of the 562 icons in `free.min.css`. Add `ng-class="{active: main.Path === '/Reports/Index' || main.Path === '/Reports'}"` if you adopted the `main.Path` addition under [Navigation item](#navigation-item).

**Verify:**

- [ ] **Build → Rebuild Solution** succeeds (a `ReportsController.cs` missing from the csproj is not compiled, so `/Reports/Index` 404s at run time with `The resource cannot be found` rather than failing the build — check Solution Explorer).
- [ ] Log in, then open `/Reports/Index`: the page renders inside the shell (sidebar, header, footer present), with a `Reports` card.
- [ ] The sidebar shows the `Reports` item under Dashboard, highlighted.
- [ ] DevTools → Network: `POST /Reports/GetReports` → `200`, response `{"reportList":[{"Id":1,"Name":"Monthly"}]}`; the card lists `Monthly`.
- [ ] DevTools → Console: no errors. `[$injector:modulerr] … Module 'reports' is not available` means `Reports.js` is not in the `~/bundles/angular` entry, or its module name differs from what `App.js` lists as a dependency; `Unknown provider` instead points to a mistyped injectable inside `reportsController`, not the bundle.

## Gotchas

- **A bundle's virtual path must not be a real folder.** `~/bundles/angular` is fine because no `bundles/` directory exists; never name a bundle after a folder that does (`~/Content/css` vs a physical `Content/css/`), or IIS serves the folder and the bundle silently never renders — [§ 4](../COREUI_GUIDE.md#4-bundles), rule 1.
- **`data-coreui="navigation"` wires the group toggles.** Do not add `data-coreui-toggle="collapse"` (or a `data-coreui-target`) to `a.nav-group-toggle`: `coreui.Collapse` would then also act on the click, look for a target, and either throw or fight the Navigation component's `show` class. The sidebar has no `collapse` markup anywhere.
- **`ng-show` on a `nav-group` hides the whole group; on a `nav-item` only that item.** When a group's children are all gated individually, the parent toggle stays visible with nothing under it; when the group is gated, its children vanish with it. Gate at the level that matches the rule (`Role === 'admin'` for the Settings group).
- **The shell is inside `ng-show="main.ItemLoad"`.** Anything you add to `_Layout.cshtml` is invisible until `/Home/GetCurrentUser` answers. That includes a page's own content, so a slow or failing `GetCurrentUser` looks like a blank page — the Network tab tells the two apart.
