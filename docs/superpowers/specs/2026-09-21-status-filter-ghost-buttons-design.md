# Accounts grid: status filter and ghost action buttons — Design Spec

Date: 2026-09-21
Status: implemented and verified 2026-09-21

## 1. Goal

Two changes to the accounts grid on `Views/Settings/UserAccounts.cshtml`, both confined to
the grid header cell and the row-action cell:

1. **Status filter.** An *Active | Inactive | All* segmented control beside the "+" (new
   account) button, defaulting to **Active**, that narrows `vm.AccountList` client-side the
   same way the header search box already does.
2. **Ghost action buttons.** The five solid-colour buttons ("+", Edit, Reset Password,
   Deactivate, Activate) lose their fill and border and become CoreUI ghost buttons — an icon
   with no button chrome, tinted only on hover — because the solid green/blue/yellow/red row
   clashes with the CoreUI theme.

Both are **deliberate deviations** from OneMasaito, which has no status filter and uses
solid `btn-success` / `btn-info` / `btn-warning` / `btn-danger` buttons. Nothing else on the
page — modals, actions, endpoints, validation — changes.

## 2. Decisions

| Decision | Choice | Why |
|---|---|---|
| Where the filter runs | client-side, a second `filter:` on the `ng-repeat` | The header search box already filters the loaded list in the browser; the list is small and fully loaded. No endpoint or service change, no § 6 doc changes. |
| Filter shape | `vm.StatusFilter` string (`"active"` / `"inactive"` / `"all"`) + `$scope.StatusMatch(acc)` predicate | A named predicate reads better in markup than an inline object/ternary and avoids Angular's loose object-matching. |
| Default | `"active"` | The usual admin view; deactivated accounts are one click away. |
| Wording | `Active`, `Inactive`, `All` | Matches the Status column (`Active` / `Inactive`) and the button titles (`Activate` / `Deactivate`). |
| Control style | `div.btn-group.btn-group-sm` of three `button.btn.btn-outline-secondary`; selected one carries `active` | Reads as one neutral control; CoreUI fills the active segment grey. Low colour, in keeping with change 2. |
| Placement | in the first `<th>`, right of the "+" icon, inside a `div.d-flex.align-items-center.gap-2` | Where the user asked for it; keeps the header row one line. The flex wrapper is a `<div>` inside the cell — `display:flex` on the `<th>` itself would take it out of table layout. |
| Action button style | `btn-ghost-secondary` for "+", Edit, Reset Password; `btn-ghost-danger` for Deactivate; `btn-ghost-success` for Activate | Ghost = transparent, no border, coloured text; hover adds a light tint. Severity colour kept only where it carries meaning (deactivate / activate). |
| `icon-text-white-50` wrapper | removed from all five buttons; the rule deleted from `Content/Site.css` | It paints the icon half-white — invisible on a transparent ghost button in light mode. The icon inherits the ghost button's text colour instead. Nothing else uses the class. |
| Combination with search | AND — `filter: main.SearchBox | filter: StatusMatch` | Both narrow the same list; order is irrelevant to the result. |

## 3. Design

### 3.1 `App/Controller/UserAccounts.js`

Two additions near the top of `accountsController`, after `vm.ChangePassword = {};`:

```javascript
        vm.StatusFilter = "active";

        $scope.StatusMatch = function (acc) {
            if (vm.StatusFilter === "all") return true;
            return vm.StatusFilter === "active" ? acc.IsActive : !acc.IsActive;
        };
```

No other JS change. `Init`, `Save`, `SaveStatus` etc. are untouched; `SaveStatus` already calls
`$scope.Init()` on success, which reloads `vm.AccountList`, and the predicate re-evaluates on the
next digest, so a row deactivated under *Active* disappears from that view and shows under
*Inactive* / *All*.

### 3.2 `Views/Settings/UserAccounts.cshtml`

Header cell — replaces the current `<th>` holding the "+" button:

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

Row loop:

```html
<tr ng-repeat="acc in vm.AccountList | filter: main.SearchBox | filter: StatusMatch">
```

Row-action cell — same four buttons, same `ng-click` / `ng-show` / `title`, restyled and
unwrapped:

```html
<button class="btn btn-ghost-secondary" ng-click="EditAccount(acc)" title="Edit"><i class="cil-pencil"></i></button>
<button class="btn btn-ghost-secondary" ng-click="UpdatePassword(acc)" title="Reset Password"><i class="cil-lock-locked"></i></button>
<button class="btn btn-ghost-danger" ng-click="UpdateStatus(acc)" ng-show="acc.IsActive" title="Deactivate"><i class="cil-ban"></i></button>
<button class="btn btn-ghost-success" ng-click="UpdateStatus(acc)" ng-show="!acc.IsActive" title="Activate"><i class="cil-check-circle"></i></button>
```

Everything else in the file (modals, forms, `ng-submit`, status cell) is unchanged.

### 3.3 `Content/Site.css`

Delete the `.icon-text-white-50` rule and its comment. The growl shim and the loader stay.

### 3.4 What does not change

`SettingsController`, `UserService`, `/Settings/GetAccounts` and its response shape, the
three modals and their validation, `_Layout.cshtml`, `App.js`, bundles, csproj.

## 4. Out of scope

- Persisting the selected filter (localStorage, query string). Page reload returns to *Active*.
- Server-side filtering or paging.
- Restyling any button outside the accounts grid (modal Save/Cancel, login, header).
- Changing the Status column's wording or colours.

## 5. Documentation to update

| Doc | Change |
|---|---|
| `docs/BUILD_GUIDE.md` | § 7 `Site.css` listing (rule removed) and the `icon-text-white-50` mention in its prose; § 9.3 listing + a bullet for `StatusFilter` / `StatusMatch`; § 9.4 listing, the "OneMasaito parity" paragraph (buttons no longer "same colours"), the DEVIATION paragraph (ghost buttons, filter, helper class gone), VERIFY steps (filter behaviour); § 10 rows for the filter. |
| `docs/ARCHITECTURE.md` | § 6.2 features: add the filter and ghost buttons under an *Added* / *Changed* entry rather than *Kept (behaviour identical)*. |
| `docs/COREUI_GUIDE.md` | the button-class mapping (solid → ghost) and the note that `icon-text-white-50` is gone. |
| `docs/coreui/03-components.md` | worked-example deviation list: the real page now uses ghost buttons and a status filter. |
| `README.md` | no change expected. |

## 6. Verification

Run the app (MSBuild + IIS Express on `:60837`), log in as `admin` / `admin123`, open
**Settings → User Account**:

1. Header cell shows a plain "+" icon then `[ Active | Inactive | All ]` with *Active* filled;
   the row-action icons have no background or border; hovering tints them.
2. With every seeded account active, the grid shows all rows under *Active*, none under
   *Inactive*, all under *All*.
3. Deactivate one account (red ban icon → own password → save). Under *Active* the row is
   gone; under *Inactive* it is the only row, its Activate icon green; under *All* it is
   listed as *Inactive* in red.
4. With *All* selected, type part of a username in the header search box: the list narrows
   to matching rows. Switch to *Inactive*: only matching inactive rows remain (AND).
5. Reactivate the account; restore the DB to its seed state.
6. Console: no errors on load or during the above.
