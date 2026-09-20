# 06 · Widgets and charts

The dist's `widgets.html` is a page of cards: stat cards with a value and a label, the same cards with a small chart under the value, cards with a coloured icon box, and rows of thin progress bars. `charts.html` is Chart.js on its own. This chapter covers the card patterns first (they are CSS only and work in CoreUIDemo today), then the chart vendors the dist ships, what to add to `App_Start/BundleConfig.cs` to enable them, and one chart driven by the `reportsController` from [01-layout.md](./01-layout.md#worked-example--adding-a-reports-page). Charts are **optional**: the dist ships the vendors, so they are in scope, but CoreUIDemo does not vendor them today.

- [Widgets are cards](#widgets-are-cards)
  - [Stat card](#stat-card)
  - [Stat card with sparkline](#stat-card-with-sparkline)
  - [Progress group](#progress-group)
  - [Icon card](#icon-card)
- [Charts — what the dist ships](#charts--what-the-dist-ships)
- [Enabling charts in CoreUIDemo](#enabling-charts-in-coreuidemo)
- [A chart driven by AngularJS](#a-chart-driven-by-angularjs)
- [Theme-aware charts](#theme-aware-charts)
- [Gotchas](#gotchas)

## Widgets are cards

`style.css` has no page-specific class for `widgets.html` (nothing in the page's `class="…"` attributes is named after it). Every item on the page is a `card` from [03-components.md → Cards](./03-components.md#cards) with utility classes doing the layout. The counts from `grep -oE 'class="card[^"]*"' widgets.html | sort | uniq -c` tell the story: 32 × `card`, 26 × `card-body`, 8 × `card-body p-3 d-flex align-items-center`, 8 × `card overflow-hidden`, 4 × `card-body pb-0 d-flex justify-content-between align-items-start`, and `card text-white bg-primary` / `bg-info` / `bg-warning` / `bg-danger` / `bg-success` for the coloured stat cards. Four patterns cover all of it.

### Stat card

**1. What the dist page shows.** A coloured card with a big number, a small delta beside it, a label, and a three-dots dropdown in the corner. The first row of `widgets.html`; also the first row of `index.html`.

**2. Essential markup.** The dist's inline SVG arrows and dots are replaced with the icon font per [05-icons.md](./05-icons.md#the-dist-uses-inline-svg-coreuidemo-uses-the-font--why).

**`widgets.html`**
```html
<div class="card text-white bg-primary">                                            <!-- bg-* + text-white colours the whole card -->
  <div class="card-body pb-0 d-flex justify-content-between align-items-start">   <!-- value/label left, dropdown right -->
    <div>
      <div class="fs-4 fw-semibold">26K <span class="fs-6 fw-normal">(-12.4% <i class="cil-arrow-bottom"></i>)</span></div>
      <div>Users</div>
    </div>
    <div class="dropdown">
      <button class="btn btn-transparent text-white p-0" type="button" data-coreui-toggle="dropdown" aria-expanded="false">
        <i class="icon cil-options"></i>
      </button>
      <div class="dropdown-menu dropdown-menu-end">
        <a class="dropdown-item" href="#">Action</a>
      </div>
    </div>
  </div>
</div>
```

`btn-transparent` is a CoreUI addition (one rule in `style.css`) that removes the button chrome so only the icon shows on the coloured background. `pb-0` on the body is there because the dist puts a chart under it (next pattern); use plain `card-body` when there is no chart.

**3. Behaviour.** CSS only for the card. The dropdown is `data-coreui-toggle="dropdown"` — see [03-components.md → Dropdowns](./03-components.md#dropdowns).

**4. In CoreUIDemo.** One row of stat cards from a list the controller fetches. `ng-class` picks the colour per item, so the markup is written once.

**`Views/Reports/Index.cshtml`**
```html
<div class="row g-4 mb-4">
    <div class="col-12 col-sm-6 col-xl-3" ng-repeat="s in vm.Stats">
        <div class="card text-white" ng-class="'bg-' + s.Color">
            <div class="card-body">
                <div class="fs-4 fw-semibold">{{s.Value | number}}</div>
                <div>{{s.Label}}</div>
            </div>
        </div>
    </div>
</div>
```

**`App/Controller/Reports.js`** — inside `reportsController`
```js
        vm.Stats = [];

        $scope.Init = function () {
            $http({
                method: "POST",
                url: "/Reports/GetReports"
            }).then(function (data) {
                vm.ReportList = data.data.reportList;
                vm.Stats = data.data.stats;   // [{ Label: "Users", Value: 26000, Color: "primary" }, ...]
            });
        };
```

`{{s.Value | number}}` is AngularJS's built-in `number` filter — `26000` renders as `26,000`; `{{s.Value | number:1}}` for one decimal. `Color` must be one of the theme colour names (`primary`, `info`, `warning`, `danger`, `success`, `secondary`, `dark`) so that `bg-` + the value is a class `style.css` defines.

**5. Gotchas.** A dropdown inside a `text-white` card needs `text-white` on the button too, or the icon takes the default body colour. `ng-repeat` rows create their dropdowns after `coreui.bundle.min.js` has run, which is fine — `data-coreui-toggle="dropdown"` is handled by delegated events, not by scanning the DOM at load.

### Stat card with sparkline

**1. What the dist page shows.** The same stat card with a 70 px line chart across the bottom, and further down a row of small cards with a 80 × 40 px bar chart under a centred number (`sparkline-chart-1` … `sparkline-chart-6`).

**2. Essential markup.**

**`widgets.html`**
```html
<div class="card text-white bg-primary">
  <div class="card-body pb-0 d-flex justify-content-between align-items-start">
    …value, label and dropdown as above…
  </div>
  <div class="mt-3 mx-3" style="height: 70px">                            <!-- fixed-height box the chart fills -->
    <canvas class="chart" id="statChart1" height="70"></canvas>
  </div>
</div>

<div class="card">
  <div class="card-body text-center">
    <div class="text-body-secondary small text-uppercase fw-semibold">Title</div>
    <div class="fs-6 fw-semibold py-3">1,123</div>
    <div class="mx-auto" style="height: 40px; width: 80px">
      <canvas class="chart chart-bar" id="sparkline-chart-1" height="40" width="80"></canvas>
    </div>
  </div>
</div>
```

The dist writes `class="c-chart-wrapper mt-3 mx-3"` on the box around the canvas; `c-chart-wrapper` has no rule in `style.css` (`grep -c '\.c-chart-wrapper' css/style.css` → 0), so it is omitted here. The inline `style="height: …"` is what does the work — see [Gotchas](#gotchas).

**3. Behaviour.** The card is CSS only. The `<canvas>` is drawn by `js/widgets.js` with `new Chart(document.getElementById(…), { type: 'line', … maintainAspectRatio: false … })` (its ids are the dist's own) — the chart part is [A chart driven by AngularJS](#a-chart-driven-by-angularjs) below.

**4. In CoreUIDemo.** Same view markup as the stat card with the fixed-height box and a `<canvas id="…">` appended to the card; the controller code is in [A chart driven by AngularJS](#a-chart-driven-by-angularjs). Without the chart vendors enabled, the box is simply empty space — the card still renders.

**5. Gotchas.** `new Chart` inside `ng-repeat` needs one unique `id` per canvas — see [Gotchas](#gotchas).

### Progress group

**1. What the dist page shows.** A labelled row with one or two thin bars per row: the "Traffic & Sales" card at the bottom of the dashboard. Dist: `index.html` (the pattern is not on `widgets.html`); the classes are in `style.css` at `.progress-group`, `.progress-group-prepend`, `.progress-group-header`, `.progress-group-bars`, `.progress-thin`.

**2. Essential markup.**

**`index.html`**
```html
<div class="progress-group mb-4">                                            <!-- flex row -->
  <div class="progress-group-prepend"><span class="text-body-secondary small">Monday</span></div>   <!-- fixed 100px label column -->
  <div class="progress-group-bars">                                          <!-- grows; 2px gap between bars -->
    <div class="progress progress-thin">                                     <!-- 4px track -->
      <div class="progress-bar bg-info" role="progressbar" style="width: 34%" aria-valuenow="34" aria-valuemin="0" aria-valuemax="100"></div>
    </div>
    <div class="progress progress-thin">
      <div class="progress-bar bg-danger" role="progressbar" style="width: 78%" aria-valuenow="78" aria-valuemin="0" aria-valuemax="100"></div>
    </div>
  </div>
</div>
```

The variant with the label *above* the bar uses `progress-group-header` instead of `progress-group-prepend` (`.progress-group-header + .progress-group-bars` gets `flex-basis: 100%`, so the bars wrap to a full-width second line):

**`index.html`**
```html
<div class="progress-group">
  <div class="progress-group-header">
    <i class="icon icon-lg me-2 cil-user"></i>
    <div>Male</div>
    <div class="ms-auto fw-semibold">43%</div>
  </div>
  <div class="progress-group-bars">
    <div class="progress progress-thin">
      <div class="progress-bar bg-warning" role="progressbar" style="width: 43%" aria-valuenow="43" aria-valuemin="0" aria-valuemax="100"></div>
    </div>
  </div>
</div>
```

**3. Behaviour.** CSS only. Base `progress` / `progress-bar` are in [03-components.md → Progress](./03-components.md#progress).

**4. In CoreUIDemo.** A list of `{ Label, Percent }` rows; `ng-style` sets the width, `aria-valuenow` is interpolated.

**`Views/Reports/Index.cshtml`**
```html
<div class="card mb-4">
    <div class="card-header">Traffic</div>
    <div class="card-body">
        <div class="progress-group mb-4" ng-repeat="t in vm.Traffic">
            <div class="progress-group-prepend"><span class="text-body-secondary small">{{t.Label}}</span></div>
            <div class="progress-group-bars">
                <div class="progress progress-thin">
                    <div class="progress-bar bg-info" role="progressbar" ng-style="{width: t.Percent + '%'}" aria-valuenow="{{t.Percent}}" aria-valuemin="0" aria-valuemax="100"></div>
                </div>
            </div>
        </div>
    </div>
</div>
```

**`App/Controller/Reports.js`** — `vm.Traffic = data.data.traffic;` in the same `.then` as `vm.Stats`.

**5. Gotchas.** `progress-group-prepend` is a fixed `flex: 0 0 100px`; a label longer than that wraps inside the column rather than pushing the bars. Use `progress-group-header` for long labels.

### Icon card

**1. What the dist page shows.** A card whose body is a coloured square holding a large icon, with a value and an uppercase caption beside it. Two spacings: `card-body p-3` (padding around everything, icon box `p-3`) and `card overflow-hidden` + `card-body p-0` (icon box `p-4` bleeds to the card edge; `overflow-hidden` clips it to the card's rounded corners).

**2. Essential markup.**

**`widgets.html`**
```html
<div class="card">
  <div class="card-body p-3 d-flex align-items-center">
    <div class="bg-primary text-white p-3 me-3">                             <!-- the coloured icon box -->
      <i class="icon icon-xl cil-settings"></i>
    </div>
    <div>
      <div class="fs-6 fw-semibold text-primary">$1.999,50</div>
      <div class="text-body-secondary text-uppercase fw-semibold small">Widget title</div>
    </div>
  </div>
</div>

<div class="card overflow-hidden">
  <div class="card-body p-0 d-flex align-items-center">
    <div class="bg-primary text-white p-4 me-3">
      <i class="icon icon-xl cil-settings"></i>
    </div>
    <div>…value and caption as above…</div>
  </div>
</div>
```

**3. Behaviour.** CSS only.

**4. In CoreUIDemo.** The same `ng-repeat` over `vm.Stats` as the stat card, with `Icon` carried per item. Bind the icon with `ng-class` on an `<i>` that already has the sizing classes, as [05-icons.md → Icons with AngularJS](./05-icons.md#icons-with-angularjs) does:

**`Views/Reports/Index.cshtml`**
```html
<div class="col-12 col-sm-6 col-xl-3" ng-repeat="s in vm.Stats">
    <div class="card">
        <div class="card-body p-3 d-flex align-items-center">
            <div class="text-white p-3 me-3" ng-class="'bg-' + s.Color">
                <i class="icon icon-xl" ng-class="s.Icon"></i>
            </div>
            <div>
                <div class="fs-6 fw-semibold" ng-class="'text-' + s.Color">{{s.Value | number}}</div>
                <div class="text-body-secondary text-uppercase fw-semibold small">{{s.Label}}</div>
            </div>
        </div>
    </div>
</div>
```

`s.Icon` is a class name such as `"cil-people"` or `"cil-cart"` — pick from the 41 in [05-icons.md](./05-icons.md#reference-41-icons-an-admin-app-needs).

**5. Gotchas.** None beyond the icon rule: `ng-class` must supply only the `cil-*` part, with `icon icon-xl` static on the element.

## Charts — what the dist ships

| Dist file | What it is | Loaded by |
|---|---|---|
| `vendors/chart.js/js/chart.umd.js` | Chart.js **v4.5.1** (UMD build, header of the file); exposes global `Chart` | `index.html`, `widgets.html`, `charts.html` |
| `vendors/@coreui/chartjs/js/coreui-chartjs.js` | CoreUI Chart.js plugin v4.2.0; exposes `coreui.ChartJS` with one export, `customTooltips`, an HTML tooltip renderer wired via `Chart.defaults.plugins.tooltip.external` | `index.html`, `widgets.html`, `charts.html` |
| `vendors/@coreui/chartjs/css/coreui-chartjs.css` | Styles for that tooltip (`chartjs-tooltip` and its header/body children) | same three pages, in `<head>` after `css/style.css` |
| `vendors/@coreui/utils/js/index.js` | CoreUI Utils v2.0.1; exposes `coreui.Utils` with `getStyle`, `getColor`, `hexToRgb`, `hexToRgba`, `rgbToHex`, `deepObjectsMerge`, `makeUid`, `omitByKeys`, `pickByKeys`. `js/widgets.js` and `js/main.js` call only `getStyle` | `index.html`, `widgets.html` (not `charts.html`) |

Script order in the dist, after `coreui.bundle.min.js` and `simplebar.min.js`: `chart.umd.js` → `coreui-chartjs.js` → `index.js` (utils) → the page script (`js/main.js`, `js/widgets.js` or `js/charts.js`). `coreui-chartjs.js` and `index.js` each attach to `window.coreui`, the same object the bundle created, so the bundle must come first.

None of these four files is vendored in CoreUIDemo today: `App_Start/BundleConfig.cs` lists `style.css`, `free.min.css`, `simplebar.css`, `angular-growl.min.css`, `Site.css` and the five scripts in [`../COREUI_GUIDE.md` § 4](../COREUI_GUIDE.md#4-bundles), and nothing else. Every card pattern above works without them; only the `<canvas>` stays blank.

## Enabling charts in CoreUIDemo

Copy from the dist into `Content/vendor/`, keeping the dist's folder names (the `.map` files are optional):

| From (dist) | To (repo) |
|---|---|
| `vendors/chart.js/js/chart.umd.js` | `Content/vendor/chart.js/js/chart.umd.js` |
| `vendors/@coreui/chartjs/js/coreui-chartjs.js` | `Content/vendor/@coreui/chartjs/js/coreui-chartjs.js` |
| `vendors/@coreui/chartjs/css/coreui-chartjs.css` | `Content/vendor/@coreui/chartjs/css/coreui-chartjs.css` |
| `vendors/@coreui/utils/js/index.js` | `Content/vendor/@coreui/utils/js/index.js` |

Then add them to the two existing bundles. The CSS goes after `simplebar.css` (the dist links it after `style.css`; anywhere before `Site.css` is equivalent). The three scripts go **after** `coreui.bundle.min.js` and **before** `angular.min.js`, in the dist's order:

**`App_Start/BundleConfig.cs`**
```csharp
            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/build/css/style.css",
                      "~/Content/vendor/@coreui/icons/css/free.min.css",
                      "~/Content/vendor/simplebar/css/simplebar.css",
                      "~/Content/vendor/@coreui/chartjs/css/coreui-chartjs.css",
                      "~/Content/vendor/growl/angular-growl.min.css",
                      "~/Content/Site.css"
                      ));

            var scriptsBundle = new ScriptBundle("~/bundles/scripts").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Content/vendor/@coreui/coreui/js/coreui.bundle.min.js",
                "~/Content/vendor/simplebar/js/simplebar.min.js",
                "~/Content/vendor/chart.js/js/chart.umd.js",
                "~/Content/vendor/@coreui/chartjs/js/coreui-chartjs.js",
                "~/Content/vendor/@coreui/utils/js/index.js",
                "~/Scripts/angular.min.js",
                "~/Scripts/angular-growl.min.js"
                );
            scriptsBundle.Transforms.Clear();
            bundles.Add(scriptsBundle);
```

Why before Angular: the bundles render in `<head>` and `~/bundles/angular` runs right after `~/bundles/scripts`, so `Chart` and `coreui.Utils` must be globals before any controller file is parsed. `Transforms.Clear()` is already on this bundle ([`../COREUI_GUIDE.md` § 4](../COREUI_GUIDE.md#4-bundles), rule 3) and covers the new files — `chart.umd.js` is another modern-toolchain output the old minifier would choke on, and all three are shipped ready to serve. Add the four files to `CoreUIDemo.csproj` the same way as any other vendored asset (Solution Explorer → **Show All Files** → **Include In Project**); the bundle serves them from disk under F5 regardless, but a publish only copies project items.

Since this is a change to `App_Start/`, `docs/COREUI_GUIDE.md` § 4's bundle table and `docs/BUILD_GUIDE.md` § 7.4's code block must be updated in the same turn, per `CLAUDE.md`.

Check: open any page → DevTools → Console → `Chart.version` prints `4.5.1` and `typeof coreui.Utils.getStyle` prints `function`.

## A chart driven by AngularJS

Extends the Reports page from [01-layout.md](./01-layout.md#worked-example--adding-a-reports-page): `ReportsController.GetReports` returns `{ reportList, labels, values }`, and the view gets a second card with a canvas. The controller creates the chart once the data is in scope.

**`Controllers/ReportsController.cs`** — extend `GetReports`
```csharp
        [HttpPost]
        public JsonResult GetReports()
        {
            return Json(new
            {
                reportList = new[] { new { Id = 1, Name = "Monthly" } },
                labels = new[] { "Jan", "Feb", "Mar", "Apr", "May", "Jun" },
                values = new[] { 65, 59, 84, 84, 51, 55 }
            });
        }
```

**`Views/Reports/Index.cshtml`** — after the Reports card, still inside the `ng-controller` div
```html
    <div class="card mb-4">
        <div class="card-header">Sales</div>
        <div class="card-body">
            <div style="height: 300px">
                <canvas id="salesChart" height="300"></canvas>
            </div>
        </div>
    </div>
```

**`App/Controller/Reports.js`**
```js
angular.module("reports", ["app"])

    .controller("reportsController", function ($scope, $location, $http, $timeout, growl) {
        var vm = this;

        vm.ReportList = [];
        vm.Chart = null;

        $scope.Init = function () {
            $http({
                method: "POST",
                url: "/Reports/GetReports"
            }).then(function (data) {
                vm.ReportList = data.data.reportList;

                $timeout(function () {
                    if (vm.Chart) { vm.Chart.destroy(); }
                    vm.Chart = new Chart(document.getElementById("salesChart"), {
                        type: "line",
                        data: {
                            labels: data.data.labels,
                            datasets: [{
                                label: "Sales",
                                data: data.data.values,
                                borderColor: coreui.Utils.getStyle("--cui-primary"),
                                backgroundColor: "transparent"
                            }]
                        },
                        options: {
                            plugins: { legend: { display: false } },
                            maintainAspectRatio: false
                        }
                    });
                });
            }, function () {
                growl.error("Could not load reports.");
            });
        };

        $scope.$on("$destroy", function () {
            if (vm.Chart) { vm.Chart.destroy(); vm.Chart = null; }
        });
    });
```

Points that matter:

- **`$timeout` is injected** alongside `$scope, $location, $http, growl`. It is the built-in `$timeout` service, no extra module.
- **The chart is created inside `$timeout`**, after the `.then` has put the data on `vm`. `$http`'s `.then` runs inside a digest; the `$timeout` callback runs after the digest has finished and the DOM has been updated, which is when a canvas behind `ng-show`/`ng-if` has its real size. For a canvas that is always visible this is a no-op that costs one tick.
- **`destroy()` before `new Chart`** on the same canvas. Chart.js v4 throws `Canvas is already in use. Chart with ID '0' must be destroyed before the canvas with ID 'salesChart' can be reused` if `Init()` runs twice (a refresh button, the theme listener below) without it.
- **`$scope.$on("$destroy", …)`** releases the chart when the controller scope goes away — with `ng-view`-less MVC pages that is a full navigation, so it is mostly hygiene, but it also covers a controller inside an `ng-if`.
- **`coreui.Utils.getStyle("--cui-primary")`** reads the computed value of a variable `style.css` defines (`getStyle` is `getComputedStyle(document.body).getPropertyValue(name)` under the hood). This is *reading* a template variable, not overriding one, so it is within the no-custom-CSS policy — and it is why the line follows the theme colour without any hard-coded hex. `js/widgets.js` does exactly this for its `pointBackgroundColor` values.
- **No `Chart.defaults` block.** `js/main.js` and `js/widgets.js` open with four `Chart.defaults.plugins.tooltip.*` lines that switch every chart to the CoreUI HTML tooltip (`external = coreui.ChartJS.customTooltips`). Those are page-level globals; if you want the CoreUI tooltip in CoreUIDemo, put the four lines once in `App/App.js` after the module declaration, not in each controller.

## Theme-aware charts

`js/color-modes.js` (unmodified in CoreUIDemo, see [`../COREUI_GUIDE.md` § 9](../COREUI_GUIDE.md#9-dark-mode)) dispatches `new Event('ColorSchemeChange')` on `document.documentElement` every time the theme is set (line 63). The dist's `js/main.js` (line 30) and `js/widgets.js` (line 32) listen for it and re-read their `--cui-*` colours with `coreui.Utils.getStyle`, then call `chart.update()`.

The cheapest equivalent in a controller that already rebuilds its chart in `Init()` is to re-run `Init()`; the `destroy()` guard above makes that safe:

**`App/Controller/Reports.js`** — inside `reportsController`, after `$scope.Init`
```js
        document.documentElement.addEventListener("ColorSchemeChange", function () {
            $scope.Init();
        });
```

This is optional. `--cui-primary` has nearly the same value in both themes (the dark theme lightens it slightly), so the line chart above looks right in dark mode without it; add the listener when a chart uses `--cui-body-color` or `--cui-border-color-translucent` for axis text and grid lines (as `js/main.js` does), because those *do* change. If you add it, remove it in `$destroy` — store the handler in a variable and call `removeEventListener` with the same reference — otherwise a controller inside `ng-if` leaks one listener per instantiation. `$scope.Init()` inside a native event handler runs outside a digest; the `$http` call inside it schedules its own digest, so nothing extra is needed.

## Gotchas

- **A canvas inside a hidden region has zero size.** `ng-show="false"` sets `display: none`; Chart.js measures the parent when the chart is created and gets 0 × 0, so the chart may render at 0×0 or skip its animation. Chart.js v4 attaches a `ResizeObserver` to the canvas's parent, so a chart created while hidden usually resizes itself once the region becomes visible, but don't rely on that alone — for a chart in a tab or collapsed card, call `vm.Chart.resize()` (or re-run `Init()`) when it becomes visible.
- **`maintainAspectRatio: false` requires a fixed-height parent.** With it, Chart.js fills the parent's box; without a parent height the canvas collapses to 0 and nothing draws (or, on resize, grows without bound). That is why the dist wraps every canvas on `widgets.html` and `index.html` in a fixed-height box such as `<div style="height: 70px">` and why the example above uses `<div style="height: 300px">`; `charts.html` instead wraps its canvases in a bare `c-chart-wrapper` div with no inline height, because those charts don't set `maintainAspectRatio: false`. The `height="300"` attribute on the canvas alone is not enough once `maintainAspectRatio` is off.
- **`new Chart` inside `ng-repeat` needs unique ids.** `document.getElementById("salesChart")` inside a loop returns the first match every time, and Chart.js refuses to draw twice on one canvas. Either give each canvas `id="chart-{{s.Id}}"` and create the charts in one loop over `vm.Stats` inside the `$timeout`, or keep sparkline charts to a fixed, hand-written set as `widgets.html` does.
- **`coreui.Utils` is `undefined`** — `index.js` is not in `~/bundles/scripts`, or is listed before `coreui.bundle.min.js` (it attaches to `window.coreui`, which the bundle creates). `Chart is not defined` in a controller means `chart.umd.js` is after `angular.min.js` or missing; see [Enabling charts in CoreUIDemo](#enabling-charts-in-coreuidemo).
- **`Chart.defaults.defaultFontColor`** in `js/main.js`/`js/widgets.js` is a Chart.js v2 name that v4 ignores (the v4 property is `Chart.defaults.color`). Copying that line does nothing; set `Chart.defaults.color = coreui.Utils.getStyle("--cui-body-color")` if you want axis text to follow the theme.
- **`c-chart-wrapper`** appears 10 times in `widgets.html` but is not defined in `style.css`. Do not rely on it; the inline height is what sizes the box.
