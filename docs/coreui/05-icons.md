# 05 · Icons

CoreUI ships three icon sets, each with its own catalogue page in the dist's `icons/` folder. This chapter covers how to read a catalogue and use a name, the sizing and colour conventions CoreUIDemo follows, why the font — not inline SVG — is the right choice in this dist, how to add the two sets that aren't vendored, and the AngularJS patterns for making an icon reactive to model state.

- [What the three catalogue pages are](#what-the-three-catalogue-pages-are) · [Font usage](#font-usage) · [The dist uses inline SVG; CoreUIDemo uses the font — why](#the-dist-uses-inline-svg-coreuidemo-uses-the-font--why)
- [Adding brand or flag icons](#adding-brand-or-flag-icons) · [Icons with AngularJS](#icons-with-angularjs) · [Reference: 41 icons an admin app needs](#reference-41-icons-an-admin-app-needs)

## What the three catalogue pages are

| Set | Dist catalogue | Class prefix | Count | Vendored in CoreUIDemo? |
|---|---|---|---|---|
| CoreUI Icons Free | `icons/coreui-icons-free.html` | `cil-*` | 562 | **Yes** — `Content/vendor/@coreui/icons/css/free.min.css` |
| CoreUI Icons Brand | `icons/coreui-icons-brand.html` | `cib-*` | 828 | No |
| CoreUI Icons Flag | `icons/coreui-icons-flag.html` | `cif-*` | 198 | No |

Free is the only set copied into the repo (bundle entry in `App_Start/BundleConfig.cs`: `~/Content/vendor/@coreui/icons/css/free.min.css`); brand and flag are dist-only until you add them ([below](#adding-brand-or-flag-icons)). Flag is the only set whose dist folder ships individual SVG files (`vendors/@coreui/icons/svg/flag/cif-*.svg` — `ls vendors/@coreui/icons/svg` shows only a `flag/` subfolder); free and brand have no SVG files in this dist at all, only the font.

To find a name, open the matching catalogue page in a browser. Every entry is the icon rendered at `icon-xxl` size with its exact class name printed underneath as plain text, e.g.:

```html
<!-- icons/coreui-icons-free.html -->
<i class="icon icon-xxl mt-5 mb-2 cil-3d"></i>
<div>cil-3d</div>
```

The text under the glyph *is* the class to use — `cil-3d` → `<i class="cil-3d"></i>`. This holds for all three catalogues (brand shows `cib-*`, flag shows `cif-*`), so the lookup step is identical regardless of which set you're browsing.

## Font usage

The base pattern, from [`../COREUI_GUIDE.md` § 8](../COREUI_GUIDE.md#8-icons): `<i class="cil-user"></i>`. No wrapper markup, no JavaScript — the class alone draws the glyph.

**Sizing.** `style.css` defines `icon`, `icon-sm`, `icon-lg`, `icon-xl`, `icon-xxl` (and an escape-hatch `icon-custom-size` that turns off the built-in width/height/font-size so an inline `style` or a utility class controls the size instead). An icon with no size class is 1rem; `icon-sm` is 0.875rem, `icon-lg` 1.25rem, `icon-xl` 1.5rem, `icon-xxl` 2rem. Two of these carry a fixed meaning elsewhere in the theme, both already used in `_Layout.cshtml` (§ 5 of `COREUI_GUIDE.md`):

- **Sidebar:** `nav-icon`, not `icon`, e.g. `<i class="nav-icon cil-speedometer"></i>`. `.sidebar-nav .nav-icon` in `style.css` sets its own width/height/colour off CSS custom properties scoped to the sidebar nav link, so it lines up in the nav rail and follows the active/hover/disabled state — it is not sized with `icon-sm`/`icon-lg`.
- **Header:** `icon icon-lg`, e.g. `<i class="icon icon-lg cil-menu"></i>` for the header toggler and `<i class="icon icon-lg cil-user"></i>` in the avatar dropdown.
- **Buttons and inline text:** plain `icon` with a size class, or no size class at all for text-sized glyphs, e.g. `<i class="cil-pencil"></i>` inside a `btn btn-info` ([`03-components.md`](./03-components.md), the CRUD grid's row buttons).

**Colour** follows Bootstrap's text utilities, since an `<i class="cil-*">` glyph is just text-coloured content: `<i class="cil-check-circle text-success"></i>`. There is no separate colour class for icons beyond this.

**Spacing.** When an icon sits before a label inside the same element, `me-2` is the convention already used in the header's avatar dropdown (`<i class="cil-lock-locked me-2"></i>Change Password`) — one utility class, no custom CSS.

CoreUIDemo's existing icon choices (sidebar, header, grid buttons) are already tabulated in [`../COREUI_GUIDE.md` § 8](../COREUI_GUIDE.md#8-icons); this chapter does not repeat that table.

## The dist uses inline SVG; CoreUIDemo uses the font — why

Open `index.html` and look at any of its icons — the sidebar nav icons, the header toggler, the avatar glyph. You might expect a compact sprite reference (`<svg><use xlink:href="…/free.svg#…"></use></svg>`, with the icon's class name after the `#`), but that is not what this dist does. `grep -m3 -o 'xlink:href="[^"]*"' index.html` finds exactly one hit anywhere in the file, `xlink:href="#a"`, which belongs to the sidebar-brand logo's own `<svg>` (an internal gradient/clip reference), not to any icon. Every icon element instead inlines its own complete, unique `<path>` data:

```html
<!-- index.html -->
<svg class="nav-icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512">
  <path fill="var(--ci-primary-color, currentcolor)" d="M425.706 142.294A240 240 0 0 0 16 312v88h144v-32H48v-56c0-114.691…" class="ci-primary" />
</svg>
```

There is no sprite file to reference even if you wanted the `<use>` approach: `vendors/@coreui/icons/svg/` in this dist contains **only** a `flag/` folder of `cif-*.svg` files (established in [`../COREUI_GUIDE.md` § 1](../COREUI_GUIDE.md#1-what-the-dist-contains-and-what-we-use)) — free and brand ship only as an icon font (`free.min.css` / `brand.min.css` + `fonts/`), never as individual or sprited SVG. Reproducing the dashboard's look by hand would mean copy-pasting a different multi-line `<path>` blob into every Razor view per icon, with no way to change one occurrence without touching the markup. Even the dist's own catalogue pages don't do this: `icons/coreui-icons-free.html`, `-brand.html`, and `-flag.html` all render every glyph through the font (`<i class="icon icon-xxl cil-3d"></i>`), not inline SVG. The font is both the only complete option for free/brand icons in this dist and the one the dist's own reference pages use — and it renders identically to the inline SVG at every size CoreUIDemo uses (`nav-icon`, `icon icon-lg`, `icon icon-xxl` on the catalogue itself).

## Adding brand or flag icons

Brand and flag are structured differently from each other, so "copy the CSS and its assets" means different files for each.

**Brand** (`cib-*`) is a font, wired the same way `free.min.css` already is. `brand.min.css` resolves its glyphs through `@font-face` `url()`s pointing at `../fonts/CoreUI-Icons-Brand.{eot,svg,ttf,woff}` — the same relative-path pattern `free.min.css` uses for `CoreUI-Icons-Free.*`. Copy both:

```
vendors/@coreui/icons/css/brand.min.css              → Content/vendor/@coreui/icons/css/brand.min.css
vendors/@coreui/icons/fonts/CoreUI-Icons-Brand.*      → Content/vendor/@coreui/icons/fonts/CoreUI-Icons-Brand.*
```

**Flag** (`cif-*`) is not a font at all — `flag.min.css` sets `[class^=cif-]{background-size:contain;...}` once, then gives each `.cif-xx` class a `background-image: url(../svg/flag/cif-XX.svg)` pointing at one of the 198 individual files already sitting in `vendors/@coreui/icons/svg/flag/`. Copy the CSS and that whole folder:

```
vendors/@coreui/icons/css/flag.min.css                → Content/vendor/@coreui/icons/css/flag.min.css
vendors/@coreui/icons/svg/flag/                        → Content/vendor/@coreui/icons/svg/flag/
```

Either way, the asset folder (`fonts/` for brand, `svg/` for flag) **must stay a sibling of `css/`** — the stylesheet's `url()`s are relative, and ASP.NET's bundle-relative URL rewriting only holds up when that relationship is preserved; this is exactly the reasoning `../COREUI_GUIDE.md` already gives for `free.min.css`'s `fonts/` folder in [§ 3](../COREUI_GUIDE.md#3-folder-layout-mirrors-onemasaito). Then add the new stylesheet's virtual path to the `~/Content/css` bundle in `App_Start/BundleConfig.cs`, **after** `free.min.css`, so a `cil-*` and a `cib-*`/`cif-*` class can sit on the same page without one set's rules overriding the other's cascade order.

## Icons with AngularJS

**Swapping an icon by model state** uses `ng-class` on the `<i>`'s class list, the same way any other conditional class is written in this codebase (`UserAccounts.cshtml` does exactly this for row colour: `ng-class="{'text-success': acc.IsActive, 'text-danger': !acc.IsActive}"`). Applied to an icon:

```html
<i ng-class="acc.Status === 1 ? 'cil-lock-locked' : 'cil-lock-unlocked'"></i>
```

(`cil-lock-locked` and `cil-lock-unlocked` are both confirmed present in `free.min.css`.)

**Two separate elements shown/hidden with `ng-show`** is the pattern OneMasaito actually ships, in the CRUD grid's row actions (`Views/Settings/UserAccounts.cshtml`, quoted in full in [`03-components.md`](./03-components.md#worked-example--crud-grid-page)):

```html
<button class="btn btn-danger" ng-click="UpdateStatus(acc)" ng-show="acc.IsActive" title="Deactivate">
    <i class="cil-ban"></i>
</button>
<button class="btn btn-success" ng-click="UpdateStatus(acc)" ng-show="!acc.IsActive" title="Activate">
    <i class="cil-check-circle"></i>
</button>
```

This is the safer of the two techniques when the icon sits inside a whole button whose colour, click handler, and tooltip text also change with the state — one `ng-class` expression would otherwise have to juggle four unrelated attributes at once. Reach for `ng-class` only when it's the icon's class alone that changes.

**Gotcha.** Never write `class="cil-{{x}}"` (or similar string interpolation) on an element that also carries static classes CoreUI's JS needs at init — `data-coreui-toggle="dropdown"`/`"popover"`/`"tooltip"`, `nav-link`, etc. Angular replaces the whole `class` attribute's interpolated portion on each digest, and on the first render (before `{{x}}` has a value) the element briefly has a malformed class list; if CoreUI's data-API scans and initialises the element in that window, the component reads a stale class list and never re-initialises after Angular corrects it. Keep the icon name on its own `<i>` (as both examples above do) rather than folding it into a class the JS also inspects.

## Reference: 41 icons an admin app needs

Every name below is confirmed present in `free.min.css` (`grep -c "\.cil-<name>:" vendors/@coreui/icons/css/free.min.css` returns 1 for each).

| Purpose | Icon |
|---|---|
| Sidebar toggler / header menu | `cil-menu` |
| Search | `cil-search` |
| Single user | `cil-user` |
| Multiple users / accounts | `cil-people` |
| Settings / preferences | `cil-settings` |
| Locked state / reset password | `cil-lock-locked` |
| Unlocked state | `cil-lock-unlocked` |
| Log out | `cil-account-logout` |
| Add / new record | `cil-plus` |
| Edit | `cil-pencil` |
| Delete | `cil-trash` |
| Confirm / success | `cil-check` |
| Cancel / close | `cil-x` |
| Deactivate / block | `cil-ban` |
| Active status | `cil-check-circle` |
| Warning | `cil-warning` |
| Informational | `cil-info` |
| Notifications | `cil-bell` |
| Message / mail | `cil-envelope-closed` |
| Dashboard home | `cil-home` |
| Dashboard / speedometer | `cil-speedometer` |
| Generic list view | `cil-list` |
| Folder | `cil-folder` |
| File | `cil-file` |
| Download | `cil-cloud-download` |
| Upload | `cil-cloud-upload` |
| Print | `cil-print` |
| Date picker | `cil-calendar` |
| Time / duration | `cil-clock` |
| Reports / analytics | `cil-chart` |
| Bar chart | `cil-bar-chart` |
| Currency / amount | `cil-dollar` |
| Payment | `cil-credit-card` |
| Shopping cart | `cil-cart` |
| Shipping / delivery | `cil-truck` |
| Location / address | `cil-map` |
| Phone / contact | `cil-phone` |
| Filter a list | `cil-filter` |
| Sort ascending | `cil-sort-ascending` |
| Sort descending | `cil-sort-descending` |
| Row actions menu | `cil-options` |

No substitutions were needed — every name in the brief's list passed the `free.min.css` grep as given.
