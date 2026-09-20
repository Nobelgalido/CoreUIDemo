# 03 · Components

One entry per page in the dist's `components/` folder, in the order the dist's sidebar lists them. Each entry follows the five-part shape in [README · How to read an entry](./README.md#how-to-read-an-entry): what the page shows, the smallest working markup, what drives it (a `data-coreui-*` attribute, a `coreui.<Component>` call, or CSS only), how CoreUIDemo writes the same thing as a Razor view plus an `App/Controller/<Page>.js` controller, and gotchas when there are real ones. The tables entry ends with the chapter's worked example, the **CRUD grid page**, which is a trimmed version of the project's real `Views/Settings/UserAccounts.cshtml`.

Two things this chapter does not repeat. The eight theme colours and the class family each component takes (`btn-*`, `text-bg-*`, `alert-*`, `table-*`, `list-group-item-*`) are tabulated once in [08 · Component colour variants](./08-customizing.md#component-colour-variants); the jQuery-plugin-to-`coreui.*` translation table is [`../COREUI_GUIDE.md` § 7](../COREUI_GUIDE.md#7-javascript-api-rules-bootstrap-4--jquery--coreui-v5). Everything below uses `data-coreui-*` attributes and the `coreui` global only; `data-bs-*` and `bootstrap.*` do not exist in this bundle.

- [Accordion](#accordion) · [Alerts](#alerts) · [Badge](#badge) · [Breadcrumb](#breadcrumb) · [Button group](#button-group) · [Buttons](#buttons)
- [Cards](#cards) · [Carousel](#carousel) · [Chip](#chip) · [Collapse](#collapse) · [Dropdowns](#dropdowns) · [List group](#list-group)
- [Modals](#modals) · [Navs and tabs](#navs-and-tabs) · [Pagination](#pagination) · [Placeholders](#placeholders) · [Popovers](#popovers) · [Progress](#progress)
- [Spinners](#spinners) · [Tables](#tables) — ends with [Worked example — CRUD grid page](#worked-example--crud-grid-page) · [Toasts](#toasts) · [Tooltips](#tooltips)

Which pages need JavaScript: accordion, alerts (dismiss), carousel, chip, collapse, dropdowns, modals, navs and tabs, popovers, toasts and tooltips are backed by a component in `coreui.bundle.min.js`; badge, breadcrumb, button group (CSS `btn-check`; the `button` toggler exists in the bundle but the dist page does not use it), buttons, cards, list group, pagination, placeholders, progress, spinners and tables are CSS only. Every AngularJS snippet is written for the page pattern in [README · The stack every example fits](./README.md#the-stack-every-example-fits): module `angular.module("<page>", ["app"])`, controller `function ($scope, $location, $http, growl)`, view root `<div ng-controller="<x>Controller as vm" ng-init="Init()">`.

## Accordion

**1. What the dist page shows.** Three stacked panels where opening one closes the others, then the same set without outer borders (`accordion-flush`) and a set that lets several stay open. Dist: `components/accordion.html`.

**2. Essential markup.**

**`components/accordion.html`**
```html
<div class="accordion" id="accordionExample">                        <!-- the parent every item points back to -->
  <div class="accordion-item">
    <h2 class="accordion-header">
      <button class="accordion-button collapsed" type="button"         <!-- collapsed = closed at load; drop it on the open one -->
              data-coreui-toggle="collapse" data-coreui-target="#collapseOne">Accordion Item #1</button>
    </h2>
    <div class="accordion-collapse collapse" id="collapseOne" data-coreui-parent="#accordionExample">
      <div class="accordion-body">…</div>                              <!-- add "show" next to "collapse" for the open one -->
    </div>
  </div>
  <!-- more accordion-item -->
</div>
```

**3. Behaviour.** `coreui.Collapse`, entirely through the data API: `data-coreui-toggle="collapse"` on the button, `data-coreui-target` naming the panel, and `data-coreui-parent` on each panel so that opening one closes its siblings. Leave `data-coreui-parent` off and the panels open independently. `accordion-flush` on the outer `div` removes the outer border and radius so the accordion sits flush in a `card-body`.

**4. In CoreUIDemo.** Generate the items from data. Interpolation inside `id` and `data-coreui-target` works because AngularJS has compiled the view long before the user clicks; the collapse component reads the target selector at click time.

**`Views/Reports/Index.cshtml`** — an FAQ-style list from `vm.Sections`
```html
<div class="accordion accordion-flush" id="sections">
    <div class="accordion-item" ng-repeat="s in vm.Sections">
        <h2 class="accordion-header">
            <button class="accordion-button collapsed" type="button"
                    data-coreui-toggle="collapse" data-coreui-target="#section{{$index}}">{{s.Title}}</button>
        </h2>
        <div class="accordion-collapse collapse" id="section{{$index}}" data-coreui-parent="#sections">
            <div class="accordion-body">{{s.Body}}</div>
        </div>
    </div>
</div>
```

**5. Gotchas.** `$index` restarts at 0 for every `ng-repeat`, so two accordions on one view need different id prefixes (`section{{$index}}` vs `note{{$index}}`) or the second one's buttons open the first one's panels. If the list is re-fetched while a panel is open, the re-rendered panel comes back `collapsed` — that is the normal state for freshly compiled markup, not a bug.

## Alerts

**1. What the dist page shows.** One block per theme colour, alerts with links and headings, and a dismissible alert with a close button. Dist: `components/alerts.html`.

**2. Essential markup.**

**`components/alerts.html`**
```html
<div class="alert alert-primary" role="alert">This is a primary alert—check it out!</div>

<div class="alert alert-warning alert-dismissible fade show" role="alert">     <!-- fade show: animates out on dismiss -->
  <div class="fw-semibold">Holy guacamole!</div>
  You should check in on some of those fields below.
  <button class="btn-close" type="button" data-coreui-dismiss="alert" aria-label="Close"></button>
</div>
```

**3. Behaviour.** CSS only for a plain alert. The dismiss button is `coreui.Alert` through `data-coreui-dismiss="alert"`: it removes the element from the DOM after the `fade` transition. `alert-heading` styles a heading inside, `alert-link` colours a link to match.

**4. In CoreUIDemo.** OneMasaito uses growl for transient outcomes (`growl.success` after a save, `growl.error` for validation) and a page-level alert only for a note that must stay until the condition clears. Bind it to a controller flag rather than dismissing it:

**`Views/Reports/Index.cshtml`**
```html
<div class="alert alert-warning" role="alert" ng-show="vm.Warning">{{vm.Warning}}</div>
```

**`App/Controller/Reports.js`** — inside `$scope.Init`'s `.then`
```js
vm.Warning = data.data.reportList.length === 0 ? "No reports have been generated this month." : "";
```

**5. Gotchas.** Do not combine `alert-dismissible` with `ng-show`: the close button removes the element from the DOM, so a later `vm.Warning = "…"` has nothing left to show. Use one or the other.

## Badge

**1. What the dist page shows.** A `New` badge scaled inside each heading level, one badge per colour, then the same set as pills. Dist: `components/badge.html`.

**2. Essential markup.**

**`components/badge.html`**
```html
<h4>Example heading <span class="badge bg-secondary">New</span></h4>   <!-- badge scales with the parent's font-size -->
<span class="badge me-1 rounded-pill bg-primary">Primary</span>        <!-- rounded-pill: fully rounded ends -->
```

**3. Behaviour.** CSS only. The dist page pairs `badge` with `bg-*`; `text-bg-*` (background plus a contrasting text colour, with a dark-mode rule) is in `style.css` too and is the pairing [08 · Component colour variants](./08-customizing.md#component-colour-variants) recommends. `badge-sm` is the small size the dist sidebar uses. A counter pinned to a button's corner is not on the dist page but uses only utilities from `style.css` — the dist's `authentication/login.html` uses the same `translate-middle` trick:

```html
<button class="btn btn-primary position-relative" type="button">
  Inbox <span class="position-absolute top-0 start-100 translate-middle badge rounded-pill text-bg-danger">{{vm.Unread}}</span>
</button>
```

**4. In CoreUIDemo.** The status pill from 08, colour chosen by the row:

**`Views/Settings/UserAccounts.cshtml`**
```html
<span class="badge" ng-class="acc.IsActive ? 'text-bg-success' : 'text-bg-danger'">{{ acc.IsActive ? 'Active' : 'Inactive' }}</span>
```

`badge` stays static in `class`; only the colour class is computed, so the element never loses its base styling while the expression evaluates.

## Breadcrumb

**1. What the dist page shows.** Trails of one to three items, the last one `active` with no link, plus an anchor-only variant with no `ol`. Dist: `components/breadcrumb.html`.

**2. Essential markup.**

**`components/breadcrumb.html`**
```html
<nav aria-label="breadcrumb">
  <ol class="breadcrumb">
    <li class="breadcrumb-item"><a href="#">Home</a></li>
    <li class="breadcrumb-item"><a href="#">Library</a></li>
    <li class="breadcrumb-item active" aria-current="page">Data</li>      <!-- active: current page, plain text -->
  </ol>
</nav>
```

**3. Behaviour.** CSS only; the `/` separator is `breadcrumb-item::before`.

**4. In CoreUIDemo.** The breadcrumb belongs in the header's second row, which `_Layout.cshtml` dropped; [01 · Second row: breadcrumb](./01-layout.md#second-row-breadcrumb) shows the row to put back, with `@ViewBag.Title` as the active item so every view names itself. A page that needs a deeper trail than *Home / Page* renders its own `nav` at the top of the view with the middle items as real links (`/Settings/UserAccounts`), not `#`.

## Button group

**1. What the dist page shows.** Buttons fused into one bar, mixed styles, sizes, a checkbox group and a radio group that look like buttons, a toolbar of several groups, nested dropdowns and a vertical group. Dist: `components/button-group.html`.

**2. Essential markup.**

**`components/button-group.html`**
```html
<div class="btn-group" role="group">                                     <!-- btn-group-sm / btn-group-lg size the whole bar -->
  <button class="btn btn-primary" type="button">Left</button>
  <button class="btn btn-primary" type="button">Middle</button>
  <button class="btn btn-primary" type="button">Right</button>
</div>

<div class="btn-group" role="group">                                     <!-- radio toggle: input hidden, label styled as a button -->
  <input class="btn-check" id="btnradio1" type="radio" name="btnradio" autocomplete="off" checked>
  <label class="btn btn-outline-primary" for="btnradio1">Radio 1</label>
  <input class="btn-check" id="btnradio2" type="radio" name="btnradio" autocomplete="off">
  <label class="btn btn-outline-primary" for="btnradio2">Radio 2</label>
</div>

<div class="btn-toolbar" role="toolbar">…several btn-group…</div>        <!-- btn-group-vertical stacks the buttons -->
```

**3. Behaviour.** CSS only. `btn-check` hides the real `input`; the sibling `label.btn` takes the checked look from `:checked + .btn`. (The `data-coreui-toggle="button"` pressed-state toggler exists in the bundle but the dist page does not use it; a native checkbox or radio with `btn-check` is simpler and binds to `ng-model` directly.)

**4. In CoreUIDemo.** A filter that is really a radio group — `ng-model` on the hidden inputs, the same as on any radio:

**`Views/Settings/UserAccounts.cshtml`**
```html
<div class="btn-group mb-3" role="group">
    <input class="btn-check" id="statusAll" type="radio" ng-model="vm.StatusFilter" ng-value="undefined">
    <label class="btn btn-outline-secondary" for="statusAll">All</label>
    <input class="btn-check" id="statusActive" type="radio" ng-model="vm.StatusFilter" ng-value="true">
    <label class="btn btn-outline-secondary" for="statusActive">Active</label>
    <input class="btn-check" id="statusInactive" type="radio" ng-model="vm.StatusFilter" ng-value="false">
    <label class="btn btn-outline-secondary" for="statusInactive">Inactive</label>
</div>
<!-- then: ng-repeat="acc in vm.AccountList | filter: {IsActive: vm.StatusFilter}" -->
```

`ng-value` (not `value`) keeps the model typed and lets `undefined` stand for "All": AngularJS's object-expression `filter` compares each criterion by lowercased string `indexOf` (so a string `"true"` would match too), but it skips a criterion whose value is `undefined`, which is what makes the "All" radio show everything.

## Buttons

**1. What the dist page shows.** Every colour in normal, active and disabled state; outline and ghost variants; sizes; block buttons; and the `btn-close` used by alerts, modals and toasts. Dist: `components/buttons.html`.

**2. Essential markup.**

**`components/buttons.html`**
```html
<button class="btn btn-primary" type="button">Primary</button>
<button class="btn btn-outline-primary" type="button">Outline</button>   <!-- border only, fills on hover -->
<button class="btn btn-ghost-primary" type="button">Ghost</button>       <!-- CoreUI-only: no border or fill at rest -->
<button class="btn btn-primary btn-sm" type="button">Small</button>      <!-- btn-lg for the large size -->
<button class="btn btn-primary" type="button" disabled>Disabled</button>
```

**3. Behaviour.** CSS only. `disabled` on a `button` blocks clicks natively; on an `a.btn` add `class="disabled"` plus `aria-disabled="true"` because anchors have no disabled attribute. Always set `type="button"` so the browser never treats it as a submit button.

**4. In CoreUIDemo.** Two idioms from `UserAccounts.cshtml`. The glyph-only row button — one `cil-*` glyph, colour says what it does, `title` gives the tooltip — and a save button that locks while the request is in flight:

**`Views/Settings/UserAccounts.cshtml`**
```html
<button class="btn btn-info" ng-click="EditAccount(acc)" title="Edit"><i class="cil-pencil"></i></button>
<button class="btn btn-primary" type="button" ng-click="Save()" ng-disabled="vm.Saving">Save</button>
```

**`App/Controller/UserAccounts.js`** — inside `$scope.Save`, around the `$http` call
```js
vm.Saving = true;
$http({ method: "POST", url: "/Settings/SaveNewAccount", data: { account: vm.Modal, role: vm.Modal.Role } })
    .then(function (data) {
        vm.Saving = false;
        PopUpMessage(data.data);
    });
```

The real file wraps each `<i>` in a `<span>` carrying a one-rule helper from `Content/Site.css` (ported from OneMasaito) that dims the glyph to half-white; with CoreUI's `btn-*` colours that is optional. `ng-disabled` sets the real `disabled` attribute, so the button also gets the muted look for free.

## Cards

**1. What the dist page shows.** The 18-rem card with image, title, text and button; header/footer cards; cards with tabs or pills in the header; coloured cards; card groups; and grids of cards. Dist: `components/cards.html`.

**2. Essential markup.**

**`components/cards.html`**
```html
<div class="card">
  <div class="card-header">Featured</div>
  <div class="card-body">
    <h5 class="card-title">Special title treatment</h5>                <!-- card-subtitle for a muted second line -->
    <p class="card-text">With supporting text below.</p>
    <a class="btn btn-primary" href="#">Go somewhere</a>
  </div>
  <div class="card-footer text-body-secondary">2 days ago</div>
</div>

<div class="card text-white bg-primary mb-3">…</div>                  <!-- dist pairing; text-bg-primary does both in one class -->
<div class="card-group">…cards of equal height, joined…</div>
<div class="row row-cols-1 row-cols-md-3 g-4"><div class="col"><div class="card h-100">…</div></div></div>
```

**3. Behaviour.** CSS only. `card-header-tabs` / `card-header-pills` pull a `nav` up into the header's padding (the tabs themselves are in [Navs and tabs](#navs-and-tabs)). `card-group` joins cards edge to edge; the `row-cols-*` grid keeps them separate and wraps by breakpoint, with `g-4` for the gutters and `h-100` so cards in one row match heights.

**4. In CoreUIDemo.** Every page is a card (`card mb-4` → `card-body`, as `UserAccounts.cshtml` and the Reports example in [01](./01-layout.md#worked-example--adding-a-reports-page) do). A card per record, from data:

**`Views/Reports/Index.cshtml`**
```html
<div class="row row-cols-1 row-cols-md-3 g-4">
    <div class="col" ng-repeat="r in vm.ReportList">
        <div class="card h-100">
            <div class="card-header">{{r.Name}}</div>
            <div class="card-body">
                <p class="card-text">{{r.Description}}</p>
                <button class="btn btn-primary" type="button" ng-click="Open(r)">Open</button>
            </div>
            <div class="card-footer text-body-secondary">{{r.UpdatedOn}}</div>
        </div>
    </div>
</div>
```

`ng-repeat` goes on the `col`, not the `card`, so the grid's gutters and column widths apply to each repeated cell.

## Carousel

**1. What the dist page shows.** A slides-only carousel, one with previous/next controls, one with indicator dots, captions, crossfade and per-slide intervals. Dist: `components/carousel.html`.

**2. Essential markup.**

**`components/carousel.html`**
```html
<div class="carousel slide" id="carouselExampleControls" data-coreui-ride="carousel">   <!-- ride: start cycling on load -->
  <div class="carousel-inner">
    <div class="carousel-item active"><img class="d-block w-100" src="…" alt=""></div>  <!-- exactly one active -->
    <div class="carousel-item"><img class="d-block w-100" src="…" alt=""></div>
  </div>
  <button class="carousel-control-prev" type="button" data-coreui-target="#carouselExampleControls" data-coreui-slide="prev">
    <span class="carousel-control-prev-icon" aria-hidden="true"></span>
  </button>
  <button class="carousel-control-next" type="button" data-coreui-target="#carouselExampleControls" data-coreui-slide="next">
    <span class="carousel-control-next-icon" aria-hidden="true"></span>
  </button>
</div>
```

**3. Behaviour.** `coreui.Carousel`. `data-coreui-ride="carousel"` initialises it on window `load` and starts cycling; `data-coreui-interval` (ms, on the carousel or a slide) changes the pace; the controls use `data-coreui-slide="prev|next"` and the indicators `data-coreui-slide-to="0"`, both pointing at the carousel by `data-coreui-target`. `carousel-fade` swaps the slide animation for a crossfade.

**4. In CoreUIDemo.** Slides from data: the first item must carry `active` or nothing shows, so derive it from `$first`.

**`Views/Home/Index.cshtml`**
```html
<div class="carousel slide" id="announcements" data-coreui-ride="carousel" data-coreui-interval="8000">
    <div class="carousel-inner">
        <div class="carousel-item" ng-class="{active: $first}" ng-repeat="a in vm.Announcements">
            <div class="p-5 text-center">{{a.Text}}</div>
        </div>
    </div>
</div>
```

**5. Gotchas.** The component initialises on `load`, usually before `$http` has answered; that is fine, because it looks its items up each time it slides, so slides that `ng-repeat` adds later are cycled. What it cannot survive is a list with no `active` item at all — `ng-class="{active: $first}"`, never a static `active` on the template item, which would mark every slide active.

## Chip

**1. What the dist page shows.** Pill-shaped labels: plain, coloured (`chip-primary` …), outline, sizes, with an icon or avatar, clickable, and — with `data-coreui-chip` — selectable and removable ones. A CoreUI-only component; Bootstrap has no chip. Dist: `components/chip.html`.

**2. Essential markup.**

**`components/chip.html`**
```html
<span class="chip">Basic chip</span>
<span class="chip chip-outline chip-info">Filter: Priority</span>                       <!-- chip-sm / chip-lg for size -->
<span class="chip" data-coreui-chip data-coreui-selectable="true">Selectable</span>       <!-- data-coreui-chip: JS-backed -->
<span class="chip" data-coreui-chip data-coreui-selectable="true" data-coreui-selected="true">Selected</span>
<span class="chip" data-coreui-chip data-coreui-removable="true">Removable</span>         <!-- renders its own chip-remove button -->
```

**3. Behaviour.** CSS only for a static chip. Any element with `data-coreui-chip` is turned into a `coreui.Chip` on `DOMContentLoaded`: `data-coreui-selectable="true"` toggles an `active` state on click (`data-coreui-selected="true"` starts it selected), `data-coreui-removable="true"` adds the remove button and removes the chip on click. The instance fires `selected.coreui.chip`, `deselected.coreui.chip` and `removed.coreui.chip` on the element. Chips typed into an input are the chip-input control on the forms pages — see [04 · Forms](./04-forms.md).

**4. In CoreUIDemo.** Keep the state in the controller and skip the JS instance — chips arriving from `$http` after `DOMContentLoaded` would never be auto-initialised anyway:

**`Views/Reports/Index.cshtml`** — tag filters
```html
<div class="d-flex flex-wrap gap-2 mb-3">
    <span class="chip chip-outline chip-primary chip-clickable" tabindex="0" ng-repeat="t in vm.Tags"
          ng-class="{active: t === vm.Tag}" ng-click="vm.Tag = (vm.Tag === t ? null : t)">{{t}}</span>
</div>
```

`active` is the class the selectable chip sets, so the look matches the dist's selected state without an instance.

## Collapse

**1. What the dist page shows.** A link and a button that both show/hide one panel, then two buttons that each control their own panel and a third that toggles both. Dist: `components/collapse.html`.

**2. Essential markup.**

**`components/collapse.html`**
```html
<button class="btn btn-primary" type="button" data-coreui-toggle="collapse" data-coreui-target="#collapseExample">Toggle</button>
<div class="collapse" id="collapseExample">            <!-- collapse show = open at load -->
  <div class="card card-body">…</div>
</div>
```

**3. Behaviour.** `coreui.Collapse` through `data-coreui-toggle="collapse"` + `data-coreui-target` (an `a` can use `href="#id"` instead of the target attribute). Height-animated; `collapse-horizontal` animates width. Several targets: a comma-separated selector or a shared class in `data-coreui-target`.

**4. In CoreUIDemo.** Two ways, chosen by where the state lives. When only the user toggles it and nothing else needs to know — an "advanced filters" panel — keep the data API and let the DOM own the state. When the controller decides (open the panel because a search matched, close it after a save), use `ng-show` and skip the collapse entirely:

**`Views/Settings/UserAccounts.cshtml`**
```html
<!-- user-driven: DOM owns the state -->
<button class="btn btn-ghost-secondary btn-sm" type="button" data-coreui-toggle="collapse" data-coreui-target="#filters">Filters</button>
<div class="collapse" id="filters">…</div>

<!-- controller-driven: vm owns the state -->
<button class="btn btn-ghost-secondary btn-sm" type="button" ng-click="vm.ShowFilters = !vm.ShowFilters">Filters</button>
<div ng-show="vm.ShowFilters">…</div>
```

**5. Gotchas.** Never put both on one element: `ng-show` toggles `display` while `coreui.Collapse` toggles `collapse`/`show` and an animated height, and each undoes the other. The `ng-show` version has no slide animation — accept that, rather than calling `coreui.Collapse.getOrCreateInstance(el).show()` from the controller to get one.

## Dropdowns

**1. What the dist page shows.** A button that opens a menu; split buttons; sizes; dark menus; alignment (`dropdown-menu-end`, responsive variants); dropup/dropend; menus with headers, dividers and text. Dist: `components/dropdowns.html`.

**2. Essential markup.**

**`components/dropdowns.html`**
```html
<div class="dropdown">
  <button class="btn btn-secondary dropdown-toggle" type="button" data-coreui-toggle="dropdown" aria-expanded="false">Dropdown</button>
  <ul class="dropdown-menu dropdown-menu-end">                          <!-- -end aligns the menu to the button's right edge -->
    <li><h6 class="dropdown-header">Section</h6></li>
    <li><a class="dropdown-item" href="#">Action</a></li>
    <li><hr class="dropdown-divider"></li>
    <li><a class="dropdown-item" href="#">Something else</a></li>
  </ul>
</div>
```

**3. Behaviour.** `coreui.Dropdown` through `data-coreui-toggle="dropdown"` on the toggle; the bundle resolves the menu as a sibling (or inside the same `.dropdown`) — it tries the toggle's next sibling first, then its previous sibling, then a `.dropdown-menu` found anywhere inside the parent `.dropdown`. Popper (bundled) positions it, so `dropdown-menu-end` only sets the preferred alignment. `dropdown-toggle` draws the caret; leave it off for a glyph-only toggle. The header's theme and user menus in [01 · Header](./01-layout.md#header) are this component.

**4. In CoreUIDemo.** A row-actions menu, items from data:

**`Views/Settings/UserAccounts.cshtml`**
```html
<div class="dropdown">
    <button class="btn btn-ghost-secondary btn-sm" type="button" data-coreui-toggle="dropdown" aria-expanded="false"><i class="cil-options"></i></button>
    <ul class="dropdown-menu dropdown-menu-end">
        <li><h6 class="dropdown-header">{{acc.Username}}</h6></li>
        <li><a class="dropdown-item" href ng-click="EditAccount(acc)">Edit</a></li>
        <li><a class="dropdown-item" href ng-click="UpdatePassword(acc)">Reset password</a></li>
        <li><hr class="dropdown-divider"></li>
        <li><a class="dropdown-item" href ng-click="UpdateStatus(acc)">{{acc.IsActive ? 'Deactivate' : 'Activate'}}</a></li>
    </ul>
</div>
```

**5. Gotchas.** Write `href` with no value on an `a.dropdown-item` that only runs `ng-click`: AngularJS's `a` directive then prevents the navigation, while `href="#"` would push `#` into `$location`. Inside `table-responsive` the menu is clipped by the wrapper's `overflow`, and `dropup` does not fix it: `.table-responsive { overflow-x: auto }` makes `overflow-y` `auto` too, so a dropup menu is clipped at the top edge exactly as a dropdown is at the bottom — neither direction escapes the wrapper. Use the row-button pattern from [Buttons](#buttons) instead.

## List group

**1. What the dist page shows.** Plain items, an active item, disabled items, links and buttons as items, flush and horizontal lists, coloured items, items with badges, and custom content. Dist: `components/list-group.html`.

**2. Essential markup.**

**`components/list-group.html`**
```html
<ul class="list-group">                                                <!-- list-group-flush: no outer border/radius -->
  <li class="list-group-item">Cras justo odio</li>
  <li class="list-group-item d-flex justify-content-between align-items-center">
    Morbi leo risus <span class="badge bg-primary rounded-pill">14</span>
  </li>
</ul>
<div class="list-group">
  <a class="list-group-item list-group-item-action active" href="#">The current link item</a>   <!-- -action: hover state -->
  <a class="list-group-item list-group-item-action" href="#">A second link item</a>
</div>
```

**3. Behaviour.** CSS only. `list-group-item-action` adds the hover/focus highlight to `a` or `button` items; `active` marks the current one; `list-group-item-<colour>` tints a row; `list-group-horizontal` lays the items in a row.

**4. In CoreUIDemo.** A selectable list bound to `vm.Selected` — the Reports example of [01](./01-layout.md#worked-example--adding-a-reports-page) with selection added:

**`Views/Reports/Index.cshtml`**
```html
<div class="list-group">
    <button class="list-group-item list-group-item-action d-flex justify-content-between align-items-center" type="button"
            ng-repeat="r in vm.ReportList" ng-class="{active: r === vm.Selected}" ng-click="vm.Selected = r">
        {{r.Name}}
        <span class="badge text-bg-secondary rounded-pill">{{r.Rows}}</span>
    </button>
</div>
```

Comparing by reference (`r === vm.Selected`) is correct here because `vm.Selected` is set from the same array; after `Init()` re-fetches, reset `vm.Selected = null` or compare by `r.Id === vm.Selected.Id`.

## Modals

**1. What the dist page shows.** A static rendering of the modal anatomy, a live modal opened by a button, a static-backdrop modal, scrollable, vertically centred, sizes (`modal-sm` to `modal-xl`, fullscreen), and one whose content is set from the button that opened it. Dist: `components/modals.html`.

**2. Essential markup.**

**`components/modals.html`**
```html
<button class="btn btn-primary" type="button" data-coreui-toggle="modal" data-coreui-target="#exampleModalLive">Launch</button>

<div class="modal fade" id="exampleModalLive" tabindex="-1" aria-hidden="true">     <!-- tabindex=-1: Esc and focus trap work -->
  <div class="modal-dialog">                          <!-- add modal-lg, modal-dialog-centered, modal-dialog-scrollable here -->
    <div class="modal-content">
      <div class="modal-header">
        <h5 class="modal-title">Modal title</h5>
        <button class="btn-close" type="button" data-coreui-dismiss="modal" aria-label="Close"></button>
      </div>
      <div class="modal-body"><p>Modal body text goes here.</p></div>
      <div class="modal-footer">
        <button class="btn btn-secondary" type="button" data-coreui-dismiss="modal">Close</button>
        <button class="btn btn-primary" type="button">Save changes</button>
      </div>
    </div>
  </div>
</div>
```

**3. Behaviour.** `coreui.Modal`. Opened from markup with `data-coreui-toggle="modal"` + `data-coreui-target="#id"`, or from script with `coreui.Modal.getOrCreateInstance(el).show()`; closed by any `data-coreui-dismiss="modal"` inside it, Esc, or a backdrop click. `data-coreui-backdrop="static"` on the modal keeps it open on backdrop click and `data-coreui-keyboard="false"` disables Esc — the pair the dist's "static backdrop" example uses for inputs that must not be lost.

**4. In CoreUIDemo.** Modals open from the controller, never from markup, because the controller has to prepare `vm.Modal` first. The two globals in `App/App.js` (lines 26–33) are the whole bridge:

**`App/App.js`**
```js
        ShowModal = function (id) {
            coreui.Modal.getOrCreateInstance(document.getElementById(id)).show();
        };

        HideModal = function (id) {
            coreui.Modal.getOrCreateInstance(document.getElementById(id)).hide();
        };
```

**`App/Controller/UserAccounts.js`** — the real `NewAccount` / `EditAccount` pair
```js
        $scope.NewAccount = function () {
            vm.ModalHeader = "New";
            vm.Modal = { Role: "user" };
            ShowModal("AccountModal");
        };

        $scope.EditAccount = function (value) {
            vm.ModalHeader = "Edit";
            vm.Modal = angular.copy(value);
            ShowModal("AccountModal");
        };
```

The modal markup is the dist's with the title bound (`{{vm.ModalHeader}} Account`), inputs bound to `vm.Modal.*`, and the footer's Save calling `ng-click="Save()"` — the full view is in the [worked example](#worked-example--crud-grid-page). `angular.copy` matters: editing the row object directly would change the grid while the user types, and a cancelled edit would stick.

**5. Gotchas.** `$("#AccountModal").modal("show")` — OneMasaito's line — throws `TypeError: $(...).modal is not a function`; the translation is the first row of [`../COREUI_GUIDE.md` § 7](../COREUI_GUIDE.md#7-javascript-api-rules-bootstrap-4--jquery--coreui-v5). Keep the modal `div` inside the `ng-controller` div (as `UserAccounts.cshtml` does) or its `vm.*` bindings resolve against `mainController` and render empty. `HideModal` only after the server has answered — hiding first and then showing a growl error leaves the user with a closed modal and no idea what to fix, which is why the real `Save` checks `data.data.message == "Saved"` before hiding.

## Navs and tabs

**1. What the dist page shows.** A base `nav`, right-aligned and vertical navs, tabs and pills, fill/justified variants, and a working tab set whose panes switch on click. Dist: `components/navs-tabs.html`.

**2. Essential markup.**

**`components/navs-tabs.html`**
```html
<ul class="nav nav-tabs mb-3" role="tablist">                          <!-- nav-pills for pills; nav-underline is in style.css too -->
  <li class="nav-item" role="presentation">
    <button class="nav-link active" type="button" role="tab" data-coreui-toggle="tab" data-coreui-target="#home">Home</button>
  </li>
  <li class="nav-item" role="presentation">
    <button class="nav-link" type="button" role="tab" data-coreui-toggle="tab" data-coreui-target="#profile">Profile</button>
  </li>
</ul>
<div class="tab-content">
  <div class="tab-pane fade show active" id="home" role="tabpanel">…</div>   <!-- show active on the open pane only -->
  <div class="tab-pane fade" id="profile" role="tabpanel">…</div>
</div>
```

**3. Behaviour.** CSS only for a `nav` that only navigates. Switching panes is `coreui.Tab` via `data-coreui-toggle="tab"` (or `"pill"`) + `data-coreui-target`: it moves `active` between the `nav-link`s and `show active` between the panes. `nav-fill` / `nav-justified` stretch the items.

**4. In CoreUIDemo.** Both forms, chosen the same way as for [Collapse](#collapse). When the tabs only organise static markup, keep the data API. When the controller needs to know the tab (load data for it, reset it after a save, hide a tab by role), own the state in `vm.Tab` and drop the data attributes:

**`Views/Settings/UserAccounts.cshtml`**
```html
<ul class="nav nav-tabs mb-3">
    <li class="nav-item"><button class="nav-link" type="button" ng-class="{active: vm.Tab === 'active'}" ng-click="vm.Tab = 'active'">Active</button></li>
    <li class="nav-item"><button class="nav-link" type="button" ng-class="{active: vm.Tab === 'inactive'}" ng-click="vm.Tab = 'inactive'">Inactive</button></li>
</ul>
<div ng-show="vm.Tab === 'active'">…grid filtered to IsActive…</div>
<div ng-show="vm.Tab === 'inactive'">…</div>
```

**`App/Controller/UserAccounts.js`** — near the top of the controller
```js
        vm.Tab = "active";
```

`tab-content` / `tab-pane` are not needed in this version — they only exist for the fade that `coreui.Tab` drives.

**5. Gotchas.** Mixing the two (a `data-coreui-toggle="tab"` button that also sets `vm.Tab`) works until the controller changes `vm.Tab` on its own and the panes do not follow. Pick one owner per tab set.

## Pagination

**1. What the dist page shows.** Previous/1/2/3/Next, with icons, disabled and active states, sizes, and alignment. Dist: `components/pagination.html`.

**2. Essential markup.**

**`components/pagination.html`**
```html
<nav aria-label="Page navigation example">
  <ul class="pagination">                                              <!-- pagination-sm / pagination-lg -->
    <li class="page-item disabled"><a class="page-link" href="#">Previous</a></li>
    <li class="page-item active"><a class="page-link" href="#">1</a></li>
    <li class="page-item"><a class="page-link" href="#">2</a></li>
    <li class="page-item"><a class="page-link" href="#">Next</a></li>
  </ul>
</nav>
```

**3. Behaviour.** CSS only; `active` and `disabled` are on the `li`.

**4. In CoreUIDemo.** OneMasaito's grids are unpaged (they filter through the header search box instead). Client-side paging when a list grows: `limitTo` with a begin offset does the slicing, the controller only computes the page numbers.

**`Views/Settings/UserAccounts.cshtml`**
```html
<tr ng-repeat="acc in vm.AccountList | filter: main.SearchBox | limitTo: vm.PageSize : (vm.Page - 1) * vm.PageSize">…</tr>

<nav>
    <ul class="pagination justify-content-end mb-0">
        <li class="page-item" ng-class="{disabled: vm.Page === 1}"><a class="page-link" href ng-click="vm.Page = vm.Page - 1">Previous</a></li>
        <li class="page-item" ng-repeat="p in vm.Pages" ng-class="{active: p === vm.Page}"><a class="page-link" href ng-click="vm.Page = p">{{p}}</a></li>
        <li class="page-item" ng-class="{disabled: vm.Page === vm.Pages.length}"><a class="page-link" href ng-click="vm.Page = vm.Page + 1">Next</a></li>
    </ul>
</nav>
```

**`App/Controller/UserAccounts.js`**
```js
        vm.PageSize = 10;
        vm.Page = 1;
        vm.Pages = [];

        $scope.Init = function () {
            $http({ method: "POST", url: "/Settings/GetAccounts" }).then(function (data) {
                vm.AccountList = data.data.accountList;
                vm.Page = 1;
                vm.Pages = [];
                for (var i = 1; i <= Math.ceil(vm.AccountList.length / vm.PageSize); i++) {
                    vm.Pages.push(i);
                }
            });
        };
```

**5. Gotchas.** `disabled` on `page-item` is only a look; the `ng-click` still fires, so guard it (`ng-click="vm.Page > 1 && (vm.Page = vm.Page - 1)"`) or clamp in the controller. `vm.Pages` is computed from the full list, so a header search that narrows the rows leaves empty trailing pages; recompute from the filtered length, or reset `vm.Page = 1` with `ng-change` on the search box, if that matters.

## Placeholders

**1. What the dist page shows.** A real card next to its skeleton: grey bars where the title, text and button will be, then widths, colours, sizes and the glow/wave animations. Dist: `components/placeholders.html`.

**2. Essential markup.**

**`components/placeholders.html`**
```html
<div class="card" aria-hidden="true">
  <div class="card-body">
    <div class="h5 card-title placeholder-glow"><span class="placeholder col-6"></span></div>   <!-- glow on the parent -->
    <p class="card-text placeholder-glow">
      <span class="placeholder col-7"></span> <span class="placeholder col-4"></span>          <!-- col-*: bar width -->
    </p>
    <a class="btn btn-primary disabled placeholder col-6" href="#" tabindex="-1"></a>
  </div>
</div>
```

**3. Behaviour.** CSS only. `placeholder` makes an inline block the height of the text; `col-*` gives it a width; `placeholder-glow` or `placeholder-wave` on the parent animates every placeholder inside; `placeholder-sm` / `placeholder-lg` change the height.

**4. In CoreUIDemo.** OneMasaito shows a full-page `.loader` (the spinning circle in `Content/Site.css`) while `mainController` fetches the user. For the per-page fetch a skeleton in place of the grid reads better:

**`Views/Settings/UserAccounts.cshtml`**
```html
<p class="placeholder-glow" ng-show="!vm.Loaded">
    <span class="placeholder col-12"></span>
    <span class="placeholder col-12"></span>
    <span class="placeholder col-8"></span>
</p>
<div class="table-responsive" ng-show="vm.Loaded">…</div>
```

**`App/Controller/UserAccounts.js`** — in `$scope.Init`
```js
            vm.Loaded = false;
            $http({ method: "POST", url: "/Settings/GetAccounts" }).then(function (data) {
                vm.AccountList = data.data.accountList;
                vm.Loaded = true;
            });
```

**5. Gotchas.** A failed request never reaches the `.then` success callback, so the skeleton glows forever; pass a second function to `.then` (`function () { vm.Loaded = true; growl.error("Could not load accounts", { ttl: 5000 }); }`) so the page recovers.

## Popovers

**1. What the dist page shows.** A button that toggles a titled popover on click, four placements, and a dismiss-on-next-click variant. Dist: `components/popovers.html`.

**2. Essential markup.**

**`components/popovers.html`** — based on the dist page
```html
<!-- written as title; the dist page shows it post-init as data-coreui-original-title -->
<button class="btn btn-lg btn-danger" type="button" data-coreui-toggle="popover"
        title="Popover title" data-coreui-content="And here's some amazing content.">Click to toggle popover</button>
<button class="btn btn-secondary" type="button" data-coreui-toggle="popover" data-coreui-container="body"
        data-coreui-placement="right" data-coreui-content="Right popover">Popover on right</button>
```

**3. Behaviour.** `coreui.Popover` — the only component here that is **not** initialised by the data attribute alone. `data-coreui-toggle="popover"` is a selector the dist's `js/popovers.js` looks for: `for (const element of document.querySelectorAll('[data-coreui-toggle="popover"]')) { new coreui.Popover(element); }`. The title comes from `title` (copied into `data-coreui-original-title` on init, which is why the dist markup shows that attribute), the body from `data-coreui-content`, the side from `data-coreui-placement`, `data-coreui-trigger="focus"` dismisses on the next click, and `data-coreui-container="body"` keeps it out of an `overflow` wrapper such as `table-responsive`.

**4. In CoreUIDemo.** CoreUIDemo does not bundle `js/popovers.js`, and it would run before `ng-repeat` produced the rows anyway. Initialise in the controller after the data has rendered — inject `$timeout` and defer one digest:

**`Views/Settings/UserAccounts.cshtml`** — illustrative only (like the counter badge near [Badge](#badge)): a "why is this account inactive" hint in the status cell; `acc.DeactivatedReason` is not a field the real account model has, so treat this as a pattern to adapt, not code to paste in
```html
<i class="cil-info" ng-show="!acc.IsActive" data-coreui-toggle="popover" data-coreui-container="body"
   data-coreui-placement="left" title="Inactive" data-coreui-content="{{acc.DeactivatedReason}}"></i>
```

**`App/Controller/UserAccounts.js`**
```js
    .controller("accountsController", function ($scope, $location, $http, growl, $timeout) {
        var vm = this;

        $scope.Init = function () {
            $http({ method: "POST", url: "/Settings/GetAccounts" }).then(function (data) {
                vm.AccountList = data.data.accountList;

                $timeout(function () {
                    document.querySelectorAll('[data-coreui-toggle="popover"]').forEach(function (el) {
                        coreui.Popover.getOrCreateInstance(el);
                    });
                });
            });
        };
```

`$timeout` with no delay runs after the digest that renders the rows, so the elements exist; `getOrCreateInstance` makes the loop safe to run again after every `Init()`.

**5. Gotchas.** Rows that `ng-repeat` removes take their popover instance with them, but a popover that was *open* at that moment stays on screen — before re-fetching, loop the same selector and call `hide()` on each instance `coreui.Popover.getInstance(el)` returns. `data-coreui-content="{{…}}"` is read once at init, so a value that changes later needs `dispose()` and a fresh instance.

## Progress

**1. What the dist page shows.** Bars at 0–100 %, with labels, heights, colours, multiple bars in one track, striped and animated. Dist: `components/progress.html`.

**2. Essential markup.**

**`components/progress.html`**
```html
<div class="progress">                                                 <!-- the track; progress-thin for a 4px track -->
  <div class="progress-bar" role="progressbar" style="width: 25%" aria-valuenow="25" aria-valuemin="0" aria-valuemax="100"></div>
</div>
<div class="progress">
  <div class="progress-bar progress-bar-striped progress-bar-animated bg-success" style="width: 75%">75%</div>
</div>
```

**3. Behaviour.** CSS only; the value is the bar's inline `width`. `bg-*` colours the bar, `progress-bar-striped` adds the stripes, `progress-bar-animated` moves them. `progress-thin` and `progress-group` (a labelled row of bars) are CoreUI additions used by the dashboard widgets — see [06 · Widgets and charts](./06-widgets-charts.md).

**4. In CoreUIDemo.** A bar that follows a number in the controller:

**`Views/Reports/Index.cshtml`**
```html
<div class="progress progress-thin mb-2">
    <div class="progress-bar" role="progressbar" ng-style="{width: vm.Percent + '%'}" aria-valuenow="{{vm.Percent}}" aria-valuemin="0" aria-valuemax="100"></div>
</div>
<div class="progress" ng-show="vm.Generating">
    <div class="progress-bar progress-bar-striped progress-bar-animated" style="width: 100%">Generating…</div>
</div>
```

`style="width: {{vm.Percent}}%"` also works in current browsers, but `ng-style` never renders the raw `{{…}}` text into the attribute before the first digest.

## Spinners

**1. What the dist page shows.** The rotating border and the pulsing dot, in every colour, in small size, placed with flex and float utilities, and inside buttons. Dist: `components/spinners.html`.

**2. Essential markup.**

**`components/spinners.html`**
```html
<div class="spinner-border" role="status"><span class="visually-hidden">Loading...</span></div>   <!-- text-* for colour -->
<div class="spinner-grow spinner-grow-sm" role="status"><span class="visually-hidden">Loading...</span></div>
<button class="btn btn-primary" type="button" disabled>
  <span class="spinner-border spinner-border-sm" aria-hidden="true"></span> Loading...
</button>
```

**3. Behaviour.** CSS only (a keyframe animation). `spinner-border-sm` / `spinner-grow-sm` fit the line height of a button.

**4. In CoreUIDemo.** The `vm.Saving` flag from [Buttons](#buttons), shown inside the button:

**`Views/Settings/UserAccounts.cshtml`**
```html
<button class="btn btn-primary" type="button" ng-click="Save()" ng-disabled="vm.Saving">
    <span class="spinner-border spinner-border-sm" aria-hidden="true" ng-show="vm.Saving"></span>
    Save
</button>
```

The `.loader` circle in `Content/Site.css` that `_Layout.cshtml` shows until `main.ItemLoad` is true could be replaced by `<div class="spinner-border text-primary" role="status">` with no other change — and `Site.css` would then have one rule fewer.

## Tables

**1. What the dist page shows.** The base table, coloured tables and rows, accented (striped, hover, active), bordered/borderless, small, vertical alignment, nesting, captions, table head styles, and responsive wrappers. Dist: `components/tables.html`.

**2. Essential markup.**

**`components/tables.html`**
```html
<div class="table-responsive">                       <!-- horizontal scroll below the breakpoint instead of overflow -->
  <table class="table table-striped table-hover align-middle">   <!-- table-bordered, table-sm as needed -->
    <thead>
      <tr><th scope="col">#</th><th scope="col">First</th><th scope="col">Last</th></tr>
    </thead>
    <tbody class="table-group-divider">              <!-- heavier line between head and body -->
      <tr><th scope="row">1</th><td>Mark</td><td>Otto</td></tr>
      <tr class="table-warning"><th scope="row">2</th><td>Jacob</td><td>Thornton</td></tr>   <!-- table-<colour> tints a row/cell -->
    </tbody>
  </table>
</div>
```

**3. Behaviour.** CSS only. `table-striped` needs `tbody` rows to alternate; `table-hover` highlights the row under the pointer; `align-middle` on the table centres cell content vertically, which matters for rows that hold buttons. `table-<colour>` goes on `table`, `tr` or `td`.

**4. In CoreUIDemo.** The OneMasaito grid shape: a `card` holding a `table-responsive` wrapper, `table table-striped`, a New button in the first header cell, an actions cell first in each row, and the header search box from [01 · Header](./01-layout.md#header) filtering the rows through `filter: main.SearchBox` — the layout's `main` scope is visible from every page controller. This is the complete pattern, so it is the worked example below.

### Worked example — CRUD grid page

A trimmed version of the project's real user-accounts page: list, add, edit and deactivate, with one modal for add/edit and one for the deactivate confirmation. Field names (`Username`, `FullName`, `Role`, `IsActive`, `ID`) and endpoints (`/Settings/GetAccounts`, `/Settings/SaveNewAccount`, `/Settings/UpdateStatus`) are the real ones; the password-reset flow and the jQuery keypress guards are left out. Three more deviations from the real page, so a diff against the full files is not a surprise: (a) the status cell here is a `badge` ([08 · Component colour variants](./08-customizing.md#component-colour-variants)), where the real page renders a plain `td ng-class="{'text-success': acc.IsActive, 'text-danger': !acc.IsActive}"` (`Views/Settings/UserAccounts.cshtml` line 55); (b) the two password checks in the real `$scope.Save` (`App/Controller/UserAccounts.js` lines 44–48: an empty/null check, then a separate length check) are merged into the one check below; (c) the real `$scope.Save` also rejects a first or last name that fails a letters-only pattern (`App/Controller/UserAccounts.js` lines 50–60); those checks are dropped here. The full files are in [`../BUILD_GUIDE.md` § 9.3](../BUILD_GUIDE.md#93-appcontrolleruseraccountsjs) and [§ 9.4](../BUILD_GUIDE.md#94-viewssettingsuseraccountscshtml), and the server side in [§ 6.2](../BUILD_GUIDE.md#62-controllerssettingscontrollercs).

**`Views/Settings/UserAccounts.cshtml`**
```html
@{
    ViewBag.Title = "User Accounts";
}

<div ng-controller="accountsController as vm">

    <h1 class="h3 mb-2">Accounts</h1>

    <div class="card mb-4" ng-init="Init()">
        <div class="card-body">
            <div class="table-responsive">
                <table class="table table-striped align-middle">
                    <thead>
                        <tr>
                            <th>
                                <button class="btn btn-success" type="button" ng-click="NewAccount()" title="New"><i class="cil-plus"></i></button>
                            </th>
                            <th>UserName</th>
                            <th>Name</th>
                            <th>Role</th>
                            <th>Status</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr ng-repeat="acc in vm.AccountList | filter: main.SearchBox">
                            <td>
                                <button class="btn btn-info" type="button" ng-click="EditAccount(acc)" title="Edit"><i class="cil-pencil"></i></button>
                                <button class="btn btn-danger" type="button" ng-click="UpdateStatus(acc)" ng-show="acc.IsActive" title="Deactivate"><i class="cil-ban"></i></button>
                                <button class="btn btn-success" type="button" ng-click="UpdateStatus(acc)" ng-show="!acc.IsActive" title="Activate"><i class="cil-check-circle"></i></button>
                            </td>
                            <td>{{acc.Username}}</td>
                            <td>{{acc.FullName}}</td>
                            <td>{{acc.Role}}</td>
                            <td><span class="badge" ng-class="acc.IsActive ? 'text-bg-success' : 'text-bg-danger'">{{ acc.IsActive ? 'Active' : 'Inactive' }}</span></td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <!--CRUD MODAL-->
    <div class="modal fade" id="AccountModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">{{vm.ModalHeader}} Account</h4>
                    <button type="button" class="btn-close" data-coreui-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <div class="mb-3">
                        <label class="form-label">Username</label>
                        <input type="text" class="form-control" ng-model="vm.Modal.Username" ng-disabled="vm.ModalHeader === 'Edit'" />
                    </div>
                    <div class="mb-3" ng-show="vm.ModalHeader === 'New'">
                        <label class="form-label">Password</label>
                        <input type="password" class="form-control" ng-model="vm.Modal.Password" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label">First Name</label>
                        <input type="text" class="form-control" ng-model="vm.Modal.FirstName" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Last Name</label>
                        <input type="text" class="form-control" ng-model="vm.Modal.LastName" />
                    </div>
                    <div class="mb-3">
                        <label class="form-label">Role</label>
                        <select class="form-select" ng-model="vm.Modal.Role" ng-options="r for r in vm.RoleList"></select>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-primary" ng-click="Save()">Save</button>
                </div>
            </div>
        </div>
    </div>

    <!--UPDATE STATUS MODAL-->
    <div class="modal fade" id="UpdateStatusModal" tabindex="-1" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">{{vm.Status.IsActive ? 'Deactivate' : 'Activate'}} Account - {{vm.Status.FullName}}</h4>
                    <button type="button" class="btn-close" data-coreui-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">
                    <div class="mb-3">
                        <label class="form-label">Confirm Password</label>
                        <input type="password" class="form-control" ng-model="vm.Status.ConfirmPassword" />
                        <div class="form-text">Enter <b>your own</b> password to confirm this change.</div>
                    </div>
                </div>
                <div class="modal-footer">
                    <button type="button" class="btn btn-primary" ng-click="SaveStatus()">Save</button>
                </div>
            </div>
        </div>
    </div>

</div>
```

**`App/Controller/UserAccounts.js`**
```js
angular.module("useraccount", ["app"])

    .controller("accountsController", function ($scope, $location, $http, growl) {
        var vm = this;

        vm.RoleList = ["user", "manager", "admin"];

        $scope.Init = function () {
            $http({
                method: "POST",
                url: "/Settings/GetAccounts"
            }).then(function (data) {
                vm.AccountList = data.data.accountList;
            });
        };

        $scope.NewAccount = function () {
            vm.ModalHeader = "New";
            vm.Modal = { Role: "user" };
            ShowModal("AccountModal");
        };

        $scope.EditAccount = function (value) {
            vm.ModalHeader = "Edit";
            vm.Modal = angular.copy(value);
            ShowModal("AccountModal");
        };

        $scope.Save = function () {
            if (vm.Modal.Username == "" || vm.Modal.Username == null) {
                growl.error("Please input Username", { ttl: 5000 });
            }
            else if (vm.ModalHeader === "New" && (vm.Modal.Password == null || vm.Modal.Password.length < 6)) {
                growl.error("Password must be at least 6 characters", { ttl: 5000 });
            }
            else if (vm.Modal.FirstName == "" || vm.Modal.FirstName == null) {
                growl.error("Please input First Name", { ttl: 5000 });
            }
            else if (vm.Modal.LastName == "" || vm.Modal.LastName == null) {
                growl.error("Please input Last Name", { ttl: 5000 });
            }
            else if (vm.Modal.Role == "" || vm.Modal.Role == null) {
                growl.error("Please select Role", { ttl: 5000 });
            }
            else {
                $http({
                    method: "POST",
                    url: "/Settings/SaveNewAccount",
                    data: {
                        account: vm.Modal,
                        role: vm.Modal.Role
                    }
                }).then(function (data) {
                    PopUpMessage(data.data);

                    $scope.Init();

                    if (data.data.message == "Saved") {
                        HideModal("AccountModal");
                    }
                });
            }
        };

        $scope.UpdateStatus = function (value) {
            vm.Status = angular.copy(value);
            ShowModal("UpdateStatusModal");
        };

        $scope.SaveStatus = function () {
            if (vm.Status.ConfirmPassword == "" || vm.Status.ConfirmPassword == null) {
                growl.error("Please input Password to proceed", { ttl: 5000 });
            }
            else {
                $http({
                    method: "POST",
                    url: "/Settings/UpdateStatus",
                    data: {
                        account: vm.Status.ID,
                        password: vm.Status.ConfirmPassword
                    }
                }).then(function (data) {
                    if (data.data.errorMessage == "") {
                        growl.success("Account Status Successfully Changed", { ttl: 2000 });
                        $scope.Init();
                        HideModal("UpdateStatusModal");
                    }
                    else {
                        growl.error(data.data.errorMessage, { title: "Error", ttl: 2000 });
                        vm.Status.ConfirmPassword = "";
                    }
                });
            }
        };
    });
```

How the pieces fit:

- **List.** `ng-init="Init()"` on the card posts to `/Settings/GetAccounts`; `vm.AccountList` drives the rows. `filter: main.SearchBox` reads the header's search input from `mainController` — no wiring in this controller at all.
- **New / Edit.** One modal, two entry points. `vm.ModalHeader` names the mode: the title reads it, the password field shows only for `New`, the username locks for `Edit`. `angular.copy` isolates the edit from the grid until the server confirms.
- **Save.** Sequential `if` validation with `growl.error`, then one POST. `PopUpMessage` (a global in `App/App.js`, line 17) turns the server's `message` into `growl.success` or `growl.error`; `Init()` re-reads the grid in every case; `HideModal` only on `"Saved"`, so a server-side rejection leaves the modal open with the user's input intact.
- **Deactivate / Activate.** Same shape with its own modal: the row's `IsActive` picks which button is visible (`ng-show`) and which title the modal shows; the endpoint answers `{ errorMessage }` rather than `{ message }`, so this handler branches on the empty string instead of calling `PopUpMessage`.
- **Status.** The colour follows the row through `ng-class` on a `badge` — [08 · Component colour variants](./08-customizing.md#component-colour-variants).

**5. Gotchas.** `table-responsive` clips anything that overflows the wrapper, so a [dropdown](#dropdowns) or [popover](#popovers) inside the grid needs `data-coreui-container="body"` (popovers) or the plain button-per-action row used here. `ng-repeat` with `filter:` on a large list re-filters every digest; it is fine at the sizes OneMasaito's grids run at, and the [pagination](#pagination) entry is the next step if it is not.

## Toasts

**1. What the dist page shows.** A static toast, a live one shown by a button, translucent and stacked toasts, custom content, colour schemes, and placement in a container. Dist: `components/toasts.html`.

**2. Essential markup.**

**`components/toasts.html`**
```html
<div class="toast-container position-fixed top-0 end-0 p-3">           <!-- stacks toasts in a corner; the dist demos it position-static -->
  <div class="toast" id="liveToast" role="alert" aria-live="assertive" aria-atomic="true">   <!-- hidden until shown -->
    <div class="toast-header">
      <strong class="me-auto">Bootstrap</strong>
      <small>11 mins ago</small>
      <button class="btn-close" type="button" data-coreui-dismiss="toast" aria-label="Close"></button>
    </div>
    <div class="toast-body">Hello, world! This is a toast message.</div>
  </div>
</div>
```

**3. Behaviour.** `coreui.Toast`. Nothing shows a toast by attribute; the dist's `js/toasts.js` does `new coreui.Toast(document.getElementById('liveToast')).show()` on a button click. It auto-hides after 5 s (the `autohide` and `delay` options of `coreui.Toast` change that) and `data-coreui-dismiss="toast"` closes it early.

**4. In CoreUIDemo.** Optional. CoreUIDemo already has angular-growl for this — `growl.success` / `growl.error` render into `<div growl class="fading">` in `_Layout.cshtml`, and `PopUpMessage` in `App/App.js` maps server messages onto them — so a page needs no toast markup. If one page wants a persistent, richer notification (a header, an action link), keep one toast in the view and show it from the controller:

**`Views/Reports/Index.cshtml`**
```html
<div class="toast-container position-fixed top-0 end-0 p-3">
    <div class="toast" id="ReportReady" role="alert" aria-live="assertive" aria-atomic="true">
        <div class="toast-header">
            <strong class="me-auto">Report ready</strong>
            <button class="btn-close" type="button" data-coreui-dismiss="toast" aria-label="Close"></button>
        </div>
        <div class="toast-body">{{vm.ReadyName}} has finished generating.</div>
    </div>
</div>
```

**`App/Controller/Reports.js`** — in the `.then` of the request that finishes the report
```js
                vm.ReadyName = data.data.name;
                coreui.Toast.getOrCreateInstance(document.getElementById("ReportReady")).show();
```

**5. Gotchas.** Do not run two notification systems on one page; growl is fixed at the top and a `toast-container` in the same corner overlaps it. If toasts are adopted, put the container at `bottom-0 end-0`.

## Tooltips

**1. What the dist page shows.** Inline links with tooltips, four placements, an HTML tooltip, and tooltips on disabled elements. Dist: `components/tooltips.html`.

**2. Essential markup.**

**`components/tooltips.html`** — based on the dist page
```html
<!-- written as title; the dist page shows it post-init as data-coreui-original-title -->
<a href="#" data-coreui-toggle="tooltip" title="The last tip!">your own</a>
<button class="btn btn-secondary" type="button" data-coreui-toggle="tooltip" data-coreui-placement="right" title="Tooltip on right">Tooltip on right</button>
<button class="btn btn-secondary" type="button" data-coreui-toggle="tooltip" data-coreui-html="true" title="<em>Tooltip</em> with <b>HTML</b>">HTML</button>
```

**3. Behaviour.** `coreui.Tooltip`, initialised by script exactly like [Popovers](#popovers) — the dist's `js/tooltips.js` is `for (const element of document.querySelectorAll('[data-coreui-toggle="tooltip"]')) { new coreui.Tooltip(element); }`. The text is the element's `title`: the dist page writes most of them as `data-coreui-original-title` (nine occurrences), which is the attribute the component itself moves `title` into on init so the browser's native tooltip does not double up; the `data-coreui-` prefixed title attribute is not used anywhere in the dist (zero occurrences), so write `title` and let the component do the rename. `data-coreui-placement` and `data-coreui-html` as shown.

**4. In CoreUIDemo.** The row buttons in `UserAccounts.cshtml` already carry `title="Edit"` etc. and get the browser's native tooltip. To upgrade them to styled tooltips, add the toggle attribute and the same `$timeout` init as the popovers entry, in the same controller:

**`Views/Settings/UserAccounts.cshtml`**
```html
<button class="btn btn-info" type="button" ng-click="EditAccount(acc)" data-coreui-toggle="tooltip" title="Edit"><i class="cil-pencil"></i></button>
```

**`App/Controller/UserAccounts.js`** — `$timeout` is already in the signature from the popovers entry
```js
                $timeout(function () {
                    document.querySelectorAll('[data-coreui-toggle="tooltip"]').forEach(function (el) {
                        coreui.Tooltip.getOrCreateInstance(el);
                    });
                });
```

**5. Gotchas.** A tooltip on a button that hides itself on click (`ng-show="acc.IsActive"` after `UpdateStatus`) can be left hanging, since the element never got its `mouseleave`; hide it in the click handler (`coreui.Tooltip.getOrCreateInstance(el).hide()`, with `el` the button itself, obtained by adding `$event` to the call — `ng-click="UpdateStatus(acc, $event)"` — and reading `$event.currentTarget` inside `$scope.UpdateStatus`) or put the toggle on a wrapping `span` that stays. `title="{{…}}"` is read once at init, the same as popover content.
