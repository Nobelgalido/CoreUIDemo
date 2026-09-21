# Accounts Grid Status Filter + Ghost Buttons Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add an *Active | Inactive | All* client-side status filter beside the "+" button on the accounts grid, and turn the grid's five solid action buttons into CoreUI ghost (chrome-less) icons.

**Architecture:** Two additions to `accountsController` (`vm.StatusFilter`, `$scope.StatusMatch`) and a second `filter:` on the grid's `ng-repeat`. The header `<th>` gets a flex wrapper holding the ghost "+" and a `btn-group-sm` of three `btn-outline-secondary` segments. Row buttons swap `btn-success/info/warning/danger` for `btn-ghost-*` and lose the `icon-text-white-50` span; that CSS rule is deleted. Server untouched.

**Tech Stack:** ASP.NET MVC 5 (.NET Framework) + AngularJS 1.x + CoreUI v5.5.0 (Bootstrap 5). No automated test project — verification is a build + browser drive (IIS Express on `http://localhost:60837`, Playwright MCP tools).

**Spec:** `docs/superpowers/specs/2026-09-21-status-filter-ghost-buttons-design.md`

## Global Constraints

- **Do not run `git add` / `git commit` / `git push`.** The user commits. Report changed files + a suggested message instead (`CLAUDE.md`).
- **Docs must change in the same turn as source** (`CLAUDE.md`; a Stop hook enforces it). Task 3 is that reconciliation — do not end the session before it.
- Filter values are exactly `"active"`, `"inactive"`, `"all"`; default `"active"`. Labels exactly `Active`, `Inactive`, `All`.
- Ghost classes: `btn-ghost-secondary` ("+", Edit, Reset Password), `btn-ghost-danger` (Deactivate), `btn-ghost-success` (Activate). All exist in `Content/build/css/style.css`.
- The flex wrapper is a `<div>` inside the `<th>`, never on the `<th>` itself.
- Source files in the working copy (`App/Controller/UserAccounts.js`, `Views/Settings/UserAccounts.cshtml`, `Content/Site.css`) are **LF** (git autocrlf normalises on commit); `UserAccounts.js` and `UserAccounts.cshtml` carry a UTF-8 BOM — preserve the existing line endings and the BOM (use the Edit tool, don't rewrite files through a shell). `docs/*.md` are CRLF; `docs/coreui/*.md` are LF.
- `docs/BUILD_GUIDE.md` code blocks are complete, byte-exact copies of the files they list. After editing a source file, re-splice the whole block, don't hand-edit it.
- Test DB: SQL Server `localhost`, db `loginDemo`, table `USERS_ACCOUNTS`, `sa` / password in `Web.config`. Seed users: `admin/admin123` (admin), `jsmith`, `mjones`, `msmith` — all `IS_ACTIVE = 1`. Leave it exactly like that when done.

---

### Task 1: Controller filter state + predicate

**Files:**
- Modify: `App/Controller/UserAccounts.js:8` (after `vm.ChangePassword = {};`)

**Interfaces:**
- Produces: `vm.StatusFilter: "active" | "inactive" | "all"` and `$scope.StatusMatch(acc: {IsActive: boolean}) → boolean`. Task 2's markup binds to exactly these names.

- [ ] **Step 1: Add the state and predicate**

Insert after line 8 (`vm.ChangePassword = {};`), before `var namePattern`:

```javascript

        vm.StatusFilter = "active";

        $scope.StatusMatch = function (acc) {
            if (vm.StatusFilter === "all") return true;
            return vm.StatusFilter === "active" ? acc.IsActive : !acc.IsActive;
        };
```

Result (lines 1–16 of the file):

```javascript
angular.module("useraccount", ["app"])

    .controller("accountsController", function ($scope, $location, $http, growl) {
        var vm = this;

        vm.RoleList = ["user", "manager", "admin"];

        vm.ChangePassword = {};

        vm.StatusFilter = "active";

        $scope.StatusMatch = function (acc) {
            if (vm.StatusFilter === "all") return true;
            return vm.StatusFilter === "active" ? acc.IsActive : !acc.IsActive;
        };

        var namePattern = /^[a-zA-Z ]+$/;
```

- [ ] **Step 2: Verify the predicate in isolation (node, no browser)**

Run from the repo root (Git Bash):

```bash
node -e '
var vm = {}; var $scope = {};
vm.StatusFilter = "active";
$scope.StatusMatch = function (acc) { if (vm.StatusFilter === "all") return true; return vm.StatusFilter === "active" ? acc.IsActive : !acc.IsActive; };
var A = {IsActive:true}, I = {IsActive:false};
var r = [];
r.push($scope.StatusMatch(A)===true && $scope.StatusMatch(I)===false);
vm.StatusFilter = "inactive"; r.push($scope.StatusMatch(A)===false && $scope.StatusMatch(I)===true);
vm.StatusFilter = "all";      r.push($scope.StatusMatch(A)===true && $scope.StatusMatch(I)===true);
console.log(r.every(Boolean) ? "PASS" : "FAIL " + r);
'
```

Expected: `PASS`

- [ ] **Step 3: Confirm file encoding survived**

```bash
head -c 3 App/Controller/UserAccounts.js | xxd | head -1   # expect: ef bb bf
grep -c $'\r$' App/Controller/UserAccounts.js               # expect: same as total line count
wc -l App/Controller/UserAccounts.js
```

---

### Task 2: Grid markup — header cell, ng-repeat, ghost buttons, CSS cleanup

**Files:**
- Modify: `Views/Settings/UserAccounts.cshtml:15-21` (header `<th>`), `:29` (`ng-repeat`), `:30-50` (row-action cell)
- Modify: `Content/Site.css:22-26` (delete the helper rule)

**Interfaces:**
- Consumes: `vm.StatusFilter`, `StatusMatch` from Task 1.

- [ ] **Step 1: Replace the header cell**

Replace lines 15–21 of `Views/Settings/UserAccounts.cshtml`:

```html
                            <th>
                                <button class="btn btn-success" ng-click="NewAccount()">
                                    <span class="icon-text-white-50">
                                        <i class="cil-plus"></i>
                                    </span>
                                </button>
                            </th>
```

with:

```html
                            <th>
                                <div class="d-flex align-items-center gap-2">
                                    <button class="btn btn-ghost-secondary" ng-click="NewAccount()" title="New Account">
                                        <i class="cil-plus"></i>
                                    </button>
                                    <div class="btn-group btn-group-sm" role="group" aria-label="Status filter">
                                        <button type="button" class="btn btn-outline-secondary" ng-class="{ active: vm.StatusFilter === 'active' }" ng-click="vm.StatusFilter = 'active'">Active</button>
                                        <button type="button" class="btn btn-outline-secondary" ng-class="{ active: vm.StatusFilter === 'inactive' }" ng-click="vm.StatusFilter = 'inactive'">Inactive</button>
                                        <button type="button" class="btn btn-outline-secondary" ng-class="{ active: vm.StatusFilter === 'all' }" ng-click="vm.StatusFilter = 'all'">All</button>
                                    </div>
                                </div>
                            </th>
```

- [ ] **Step 2: Add the second filter to the row loop**

Replace:

```html
                        <tr ng-repeat="acc in vm.AccountList | filter: main.SearchBox">
```

with:

```html
                        <tr ng-repeat="acc in vm.AccountList | filter: main.SearchBox | filter: StatusMatch">
```

- [ ] **Step 3: Restyle the four row-action buttons**

Replace the `<td>` that holds them (everything from `<td>` on the line after the `<tr ng-repeat…>` through its `</td>`):

```html
                            <td>
                                <button class="btn btn-info" ng-click="EditAccount(acc)" title="Edit">
                                    <span class="icon-text-white-50">
                                        <i class="cil-pencil"></i>
                                    </span>
                                </button>
                                <button class="btn btn-warning" ng-click="UpdatePassword(acc)" title="Reset Password">
                                    <span class="icon-text-white-50">
                                        <i class="cil-lock-locked"></i>
                                    </span>
                                </button>
                                <button class="btn btn-danger" ng-click="UpdateStatus(acc)" ng-show="acc.IsActive" title="Deactivate">
                                    <span class="icon-text-white-50">
                                        <i class="cil-ban"></i>
                                    </span>
                                </button>
                                <button class="btn btn-success" ng-click="UpdateStatus(acc)" ng-show="!acc.IsActive" title="Activate">
                                    <span class="icon-text-white-50">
                                        <i class="cil-check-circle"></i>
                                    </span>
                                </button>
                            </td>
```

with:

```html
                            <td>
                                <button class="btn btn-ghost-secondary" ng-click="EditAccount(acc)" title="Edit">
                                    <i class="cil-pencil"></i>
                                </button>
                                <button class="btn btn-ghost-secondary" ng-click="UpdatePassword(acc)" title="Reset Password">
                                    <i class="cil-lock-locked"></i>
                                </button>
                                <button class="btn btn-ghost-danger" ng-click="UpdateStatus(acc)" ng-show="acc.IsActive" title="Deactivate">
                                    <i class="cil-ban"></i>
                                </button>
                                <button class="btn btn-ghost-success" ng-click="UpdateStatus(acc)" ng-show="!acc.IsActive" title="Activate">
                                    <i class="cil-check-circle"></i>
                                </button>
                            </td>
```

- [ ] **Step 4: Delete the CSS helper**

In `Content/Site.css` remove these five lines (the comment, the rule, and the blank line after it):

```css
/* OneMasaito's helper for icon-only buttons in the accounts grid. */
.icon-text-white-50 {
    color: rgba(255, 255, 255, 0.5);
}

```

- [ ] **Step 5: Static checks**

```bash
grep -rn "icon-text-white-50" --include=*.cshtml --include=*.css --include=*.js Views Content/Site.css App   # expect: no output
grep -c "btn-ghost" Views/Settings/UserAccounts.cshtml                                                        # expect: 5
grep -n "filter: StatusMatch" Views/Settings/UserAccounts.cshtml                                           # expect: 1 line
grep -c $'\r$' Views/Settings/UserAccounts.cshtml; wc -l Views/Settings/UserAccounts.cshtml                   # expect equal
head -c 3 Views/Settings/UserAccounts.cshtml | xxd | head -1                                                  # expect: ef bb bf
```

- [ ] **Step 6: Build**

```bash
"C:/Program Files/Microsoft Visual Studio/18/Community/MSBuild/Current/Bin/MSBuild.exe" CoreUIDemo.csproj -p:Configuration=Debug -v:m -nologo
```

Expected: last line `CoreUIDemo -> …\bin\CoreUIDemo.dll`, no `error` lines.

- [ ] **Step 7: Run and drive the page**

Start IIS Express (PowerShell):

```powershell
$cfg = "C:\Users\monst\OneDrive\Desktop\WORK\CoreUIDemo\CoreUIDemo\.vs\CoreUIDemo.slnx\config\applicationhost.config"
$p = Start-Process -FilePath "C:\Program Files\IIS Express\iisexpress.exe" -ArgumentList "/config:`"$cfg`"", "/site:CoreUIDemo" -PassThru -WindowStyle Hidden
Start-Sleep 6; "PID $($p.Id)"
```

Then with the Playwright MCP tools (`ToolSearch "select:mcp__plugin_playwright_playwright__browser_navigate,…browser_evaluate,…browser_take_screenshot,…browser_console_messages,…browser_close"`):

1. `browser_navigate` → `http://localhost:60837/`; `browser_evaluate`:
   ```js
   () => { document.querySelector('#username').value='admin'; document.querySelector('#password').value='admin123';
           ['#username','#password'].forEach(s => document.querySelector(s).dispatchEvent(new Event('input',{bubbles:true})));
           document.querySelector('form').dispatchEvent(new Event('submit',{bubbles:true,cancelable:true})); return 'submitted'; }
   ```
   then `browser_navigate` → `http://localhost:60837/Settings/UserAccounts`.
2. `browser_evaluate` — filter behaviour with all four seed users active:
   ```js
   async () => {
     const sleep = ms => new Promise(r => setTimeout(r, ms));
     const s = angular.element(document.querySelector('[ng-controller="accountsController as vm"]')).scope();
     await sleep(500);
     const rows = () => [...document.querySelectorAll('tbody tr')].filter(r => r.offsetParent !== null).length;
     const activeSeg = () => document.querySelector('.btn-group .btn.active')?.textContent.trim();
     const out = { defaultFilter: s.vm.StatusFilter, defaultSeg: activeSeg(), rowsActive: rows() };
     s.$apply(() => s.vm.StatusFilter = 'inactive'); out.rowsInactive = rows(); out.segInactive = activeSeg();
     s.$apply(() => s.vm.StatusFilter = 'all');      out.rowsAll = rows();      out.segAll = activeSeg();
     s.$apply(() => s.vm.StatusFilter = 'active');
     out.ghostButtons = document.querySelectorAll('tbody .btn-ghost-secondary, tbody .btn-ghost-danger, tbody .btn-ghost-success, thead .btn-ghost-secondary').length;
     out.leftoverSolid = document.querySelectorAll('thead .btn-success, tbody .btn-info, tbody .btn-warning, tbody .btn-danger, tbody .btn-success').length;
     out.whiteSpan = document.querySelectorAll('.icon-text-white-50').length;
     const plus = document.querySelector('thead .btn-ghost-secondary'); const cs = getComputedStyle(plus);
     out.plusBg = cs.backgroundColor; out.plusBorder = cs.borderStyle + ' ' + cs.borderColor;
     return out;
   }
   ```
   Expected: `defaultFilter: "active"`, `defaultSeg: "Active"`, `rowsActive: 4`, `rowsInactive: 0`, `segInactive: "Inactive"`, `rowsAll: 4`, `segAll: "All"`, `ghostButtons: 17` (1 "+" + 4 rows × 4), `leftoverSolid: 0`, `whiteSpan: 0`, `plusBg: "rgba(0, 0, 0, 0)"`, `plusBorder` starting with `solid rgba(0, 0, 0, 0)` or `none` (transparent either way).
3. `browser_take_screenshot` (scale `css`) — **look at it**: header shows a grey "+" then `[Active | Inactive | All]` with *Active* filled grey; row icons have no fill/border; table columns still aligned (no `display:flex` on the `<th>`).
4. Deactivate + search AND-check, through the real UI path, via `browser_evaluate`:
   ```js
   async () => {
     const sleep = ms => new Promise(r => setTimeout(r, ms));
     const s = angular.element(document.querySelector('[ng-controller="accountsController as vm"]')).scope();
     const rows = () => [...document.querySelectorAll('tbody tr')].filter(r => r.offsetParent !== null).map(r => r.children[1].textContent.trim());
     const m = s.vm.AccountList.find(a => a.Username === 'msmith');
     s.$apply(() => s.UpdateStatus(m)); await sleep(400);
     s.$apply(() => s.vm.Status.ConfirmPassword = 'admin123');
     document.querySelector('#UpdateStatusModal form').dispatchEvent(new Event('submit',{bubbles:true,cancelable:true}));
     await sleep(1200);
     const out = { afterDeactivate_active: rows() };
     s.$apply(() => s.vm.StatusFilter = 'inactive'); out.inactive = rows();
     const main = angular.element(document.querySelector('[ng-controller="mainController as main"]')).scope();
     main.$apply(() => main.main.SearchBox = 'smith'); out.inactive_search_smith = rows();
     s.$apply(() => s.vm.StatusFilter = 'all');       out.all_search_smith = rows();
     main.$apply(() => main.main.SearchBox = '');
     out.activateIconGreen = !!document.querySelector('tbody tr .btn-ghost-success:not(.ng-hide)');
     // reactivate to restore seed state
     const m2 = s.vm.AccountList.find(a => a.Username === 'msmith');
     s.$apply(() => s.UpdateStatus(m2)); await sleep(400);
     s.$apply(() => s.vm.Status.ConfirmPassword = 'admin123');
     document.querySelector('#UpdateStatusModal form').dispatchEvent(new Event('submit',{bubbles:true,cancelable:true}));
     await sleep(1200);
     s.$apply(() => s.vm.StatusFilter = 'active'); out.afterReactivate_active = rows();
     return out;
   }
   ```
   Expected: `afterDeactivate_active: ["admin","jsmith","mjones"]`; `inactive: ["msmith"]`; `inactive_search_smith: ["msmith"]`; `all_search_smith: ["jsmith","msmith"]`; `activateIconGreen: true`; `afterReactivate_active`: 4 usernames including `msmith`.
5. `browser_console_messages` level `error` → expected `Errors: 0`.
6. `browser_close`; stop IIS Express: `Stop-Process -Id <PID> -Force -Confirm:$false`.
7. Confirm DB is back to seed: `sqlcmd -S localhost -U sa -P <Web.config password> -d loginDemo -Q "SET NOCOUNT ON; SELECT USERNAME, IS_ACTIVE FROM USERS_ACCOUNTS" -W` → four rows, all `1`. Also `Remove-Item -Recurse -Force .playwright-mcp`.

---

### Task 3: Documentation sync

**Files:**
- Modify: `docs/BUILD_GUIDE.md` — § 7.3 prose (line ~1175) + `css` block (starts line ~1179); § 9.3 `js` block (starts ~1873) + bullets (~2072); § 9.4 `cshtml` block (starts ~2087), "OneMasaito parity" paragraph (~2256), DEVIATION paragraph (~2258), VERIFY list (~2260–2266); § 10 table (rows 4/15, add filter rows)
- Modify: `docs/ARCHITECTURE.md:175,179` (§ 6.2 Kept / Changed)
- Modify: `docs/COREUI_GUIDE.md:47` (folder map line for `Site.css`) + the button-class mapping table (search `btn-` in § "JS API" / "Icons" area; add a row if none exists)
- Modify: `docs/coreui/03-components.md:780` (worked-example deviation list)

**Interfaces:** none — prose + verbatim listings.

- [ ] **Step 1: Re-splice the three BUILD_GUIDE code blocks from the working tree (CRLF-safe)**

```bash
python - <<'EOF'
import io
p='docs/BUILD_GUIDE.md'
raw=io.open(p,encoding='utf-8',newline='').read(); assert '\r\n' in raw
t=raw.replace('\r\n','\n'); lines=t.split('\n')
def block_range(heading):
    i=next(n for n,l in enumerate(lines) if l.startswith(heading))
    s=next(n for n in range(i,len(lines)) if lines[n].startswith('```'))+1
    e=next(n for n in range(s,len(lines)) if lines[n]=='```')
    return s,e  # body is lines[s:e]
for heading,f in [('### 9.4 `Views/Settings/UserAccounts.cshtml`','Views/Settings/UserAccounts.cshtml'),
                  ('### 9.3 `App/Controller/UserAccounts.js`','App/Controller/UserAccounts.js'),
                  ('### 7.3 `Content/Site.css`','Content/Site.css')]:
    s,e=block_range(heading)
    src=io.open(f,encoding='utf-8-sig',newline='').read().replace('\r\n','\n').rstrip('\n').split('\n')
    lines[s:e]=src; print(f, e-s, '->', len(src))
io.open(p,'w',encoding='utf-8',newline='').write('\n'.join(lines).replace('\n','\r\n'))
EOF
```

Expected: three `… N -> M` lines; then verify:

```bash
python - <<'EOF'
import io
t=io.open('docs/BUILD_GUIDE.md',encoding='utf-8',newline='').read()
print('bare LF:', t.count('\n')-t.count('\r\n'))   # expect 0
t=t.replace('\r\n','\n')
for f in ['Views/Settings/UserAccounts.cshtml','App/Controller/UserAccounts.js','Content/Site.css']:
    src=io.open(f,encoding='utf-8-sig',newline='').read().replace('\r\n','\n').rstrip('\n')
    print('MATCH' if src in t else 'MISSING', f)
EOF
```

- [ ] **Step 2: BUILD_GUIDE prose edits**

Apply with a CRLF-preserving Python replace (same pattern as Step 1: read, `replace('\r\n','\n')`, edit, write back with `'\r\n'`). Each `old` must occur exactly once.

a) § 7.3 intro — replace:
`the icon-button helper class OneMasaito's views use, and a small shim`
with:
`and a small shim`

b) § 9.3 bullets — insert **before** the line starting `- Required checks are written`:
```
- `vm.StatusFilter` (`"active"` default, `"inactive"`, `"all"`) and `$scope.StatusMatch(acc)` drive the status filter in the grid header — a second `filter:` on the `ng-repeat`, client-side like the search box. Not in OneMasaito.
```

c) § 9.4 intro — after the sentence ending `and no form element.` append:
` The header cell holds the "+" button and an *Active | Inactive | All* `btn-group-sm` (selected segment carries `active`, bound to `vm.StatusFilter`); the row loop is `filter: main.SearchBox | filter: StatusMatch`.`

d) § 9.4 parity paragraph — replace:
`the "+" button in the header cell, the same row-action buttons in the same colours,`
with:
`the "+" button in the header cell, the same row-action buttons with the same icons and `ng-click`s,`

e) § 9.4 DEVIATION paragraph — replace:
`⚠️ **DEVIATION** — Department column and dropdown → Role column`
with:
`⚠️ **DEVIATION** — status filter (*Active | Inactive | All*, default *Active*) beside the "+" button, absent in OneMasaito; the "+" / Edit / Reset Password buttons are `btn-ghost-secondary` and Deactivate / Activate are `btn-ghost-danger` / `btn-ghost-success` instead of OneMasaito's solid `btn-success` / `btn-info` / `btn-warning` / `btn-danger`, with the `icon-text-white-50` span (and its `Site.css` rule) removed because a half-white icon is invisible on a transparent button; Department column and dropdown → Role column`

f) § 9.4 VERIFY — replace step 2:
`2. Open **User Account** → the grid shows the `admin` row, Role `admin`, Status **Active** in green.`
with:
`2. Open **User Account** → the header cell shows a plain "+" icon then **Active | Inactive | All** with *Active* filled; the grid shows the `admin` row, Role `admin`, Status **Active** in green; the row's Edit / Reset / Deactivate icons have no background. Click **Inactive** → no rows; **All** → the `admin` row again; back to **Active**.`

g) § 10 table — replace row 15:
`| 15 | Deactivate `msmith` → Confirm Password `admin123`. | `Account Status Successfully Changed`; `msmith` shows **Inactive** in red; the button turns green. |`
with:
`| 15 | Deactivate `msmith` → Confirm Password `admin123`. | `Account Status Successfully Changed`; `msmith` disappears from the *Active* view. Click **Inactive** → only `msmith`, **Inactive** in red, green Activate icon. Click **All** → every row. Type `smith` in the header search with *All* selected → `jsmith` and `msmith`; switch to *Inactive* → `msmith` only (search and filter combine). |`

- [ ] **Step 3: ARCHITECTURE.md (CRLF)**

a) In the `**Kept (behaviour identical):**` paragraph replace:
`accounts grid with New / Edit / Reset Password / Activate-Deactivate;`
with:
`accounts grid with New / Edit / Reset Password / Activate-Deactivate (same icons and calls, ghost-styled — see *Changed*);`

b) In the `**Changed:**` paragraph, before the final period of the paragraph (after `otherwise break it)`), append:
`; an *Active | Inactive | All* status filter (default *Active*) beside the "+" button, client-side via `vm.StatusFilter` / `StatusMatch` (user-requested, 2026-09-21); the grid's five action buttons are CoreUI ghost buttons (`btn-ghost-secondary` / `-danger` / `-success`) instead of solid colours, and the `icon-text-white-50` helper is gone (user-requested, 2026-09-21)`

- [ ] **Step 4: COREUI_GUIDE.md (CRLF)**

a) Line 47 — replace:
`Site.css                                   ← .loader spinner + .icon-text-white-50 (OneMasaito's sb-admin-2.css extras) + growl BS4 shim (§ 10)`
with:
`Site.css                                   ← .loader spinner (OneMasaito's sb-admin-2.css extra) + growl BS4 shim (§ 10)`

b) § 8 Icons — the table whose row is `| Grid: New / Edit / Reset Password / Deactivate / Activate | `cil-plus` / `cil-pencil` / `cil-lock-locked` / `cil-ban` / `cil-check-circle` |` (line ~193). Directly after the table (before the blank line that precedes `## 9. Dark mode`) add this paragraph:

```
The grid's action buttons are CoreUI ghost buttons — `btn-ghost-secondary` for New / Edit / Reset Password, `btn-ghost-danger` for Deactivate, `btn-ghost-success` for Activate — with a bare `<i class="cil-*">` inside: no fill or border at rest, a light tint on hover. OneMasaito's solid `btn-success` / `btn-info` / `btn-warning` / `btn-danger` and its `icon-text-white-50` half-white icon helper are gone (a half-white icon is invisible on a transparent button). The same header cell carries the *Active | Inactive | All* `btn-group btn-group-sm` of `btn-outline-secondary` segments bound to `vm.StatusFilter`; the selected segment gets `active`.
```

- [ ] **Step 5: docs/coreui/03-components.md (LF)**

Replace `Four more deviations from the real page` with `Five more deviations from the real page`, and replace:
`which behave the same. The full files are in`
with:
`which behave the same; (e) the real page's action buttons are ghost buttons (`btn-ghost-secondary` / `btn-ghost-danger` / `btn-ghost-success`, bare icon inside) and its header cell carries an *Active | Inactive | All* `btn-group-sm` filter (`vm.StatusFilter`, second `filter:` on the `ng-repeat`) — this example keeps solid buttons and no filter. The full files are in`

- [ ] **Step 6: Verify nothing stale remains**

```bash
grep -n "icon-text-white-50" docs/BUILD_GUIDE.md docs/ARCHITECTURE.md docs/COREUI_GUIDE.md README.md docs/coreui/*.md
```
Expected: only the historical mentions added in this task (BUILD_GUIDE § 9.4 DEVIATION, ARCHITECTURE *Changed*, COREUI_GUIDE table row / sentence, 03-components (e)). No hits inside code blocks.

```bash
grep -c $'\r$' docs/BUILD_GUIDE.md; wc -l docs/BUILD_GUIDE.md          # equal
grep -c $'\r$' docs/ARCHITECTURE.md; wc -l docs/ARCHITECTURE.md        # equal
grep -c $'\r$' docs/coreui/03-components.md                            # 0
```

- [ ] **Step 7: Update the spec status line**

In `docs/superpowers/specs/2026-09-21-status-filter-ghost-buttons-design.md` change `Status: approved, not yet implemented` to `Status: implemented and verified 2026-09-21`.

- [ ] **Step 8: Report — do not commit**

`git status --short` and `git diff --stat`, then tell the user the changed files and this suggested message:

```
feat: status filter beside "+" and ghost action buttons on the accounts grid

Active | Inactive | All btn-group (default Active) in the grid header,
client-side via vm.StatusFilter / StatusMatch as a second filter on the
ng-repeat. The five grid buttons become CoreUI ghost buttons; the
icon-text-white-50 helper is removed. Spec + docs synced.
```
