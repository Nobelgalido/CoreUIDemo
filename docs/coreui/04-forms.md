# 04 · Forms

One entry per page in the dist's `forms/` folder, in the order the dist's sidebar lists them. Each entry follows the five-part shape in [README · How to read an entry](./README.md#how-to-read-an-entry). Every control in part 4 is bound with `ng-model` to a field of the controller's `vm.Modal` object — the object the [modals entry](./03-components.md#modals) prepares in `NewAccount()` / `EditAccount()` and `Save()` posts to `/Settings/SaveNewAccount` — so the snippets drop straight into the `#AccountModal` form of the [CRUD grid page](./03-components.md#worked-example--crud-grid-page). Where a field is not on the real `UserModel` (`ID`, `Username`, `Password`, `FirstName`, `LastName`, `Role`, `IsActive`) it is an illustrative addition and says so.

Two things this chapter does not repeat. The Bootstrap 4 → 5 renames that OneMasaito's form markup needs (the old group wrapper → `mb-3`, `<select class="form-control">` → `form-select`, `custom-control` → `form-check`, no more append/prepend wrappers) are tabulated in [`../COREUI_GUIDE.md` § 10](../COREUI_GUIDE.md#10-bootstrap-4--bootstrap-5-class-renames); the grid and spacing utilities the layouts below lean on are in [08 · Spacing and layout utilities](./08-customizing.md#spacing-and-layout-utilities-the-template-leans-on).

- [Form control](#form-control) · [Select](#select) · [Checks and radios](#checks-and-radios) · [Range](#range) · [Input group](#input-group)
- [Floating labels](#floating-labels) · [Layout](#layout) · [Chip input](#chip-input) · [Validation](#validation)

Which pages need JavaScript: only chip input (`coreui.ChipInput`) and the dist's version of validation (a plain `submit` listener, quoted in full). Everything else is CSS only, and AngularJS does the binding. One jQuery use survives from OneMasaito — the keypress filter on the name inputs — and it is called out in the form-control entry as the exception it is.

## Form control

**1. What the dist page shows.** A labelled text input and a textarea, then sizing (`form-control-sm` / `form-control-lg`), a disabled input, a read-only input, a read-only plain-text input inside a horizontal row (`form-control-plaintext`), file inputs, a colour picker and datalists. Dist: `forms/form-control.html`.

**2. Essential markup.**

**`forms/form-control.html`**
```html
<div class="mb-3">                                                  <!-- mb-3 replaces the BS4 group wrapper -->
  <label class="form-label" for="exampleFormControlInput1">Email address</label>
  <input class="form-control" id="exampleFormControlInput1" type="email" placeholder="name@example.com">
</div>
<div class="mb-3">
  <label class="form-label" for="exampleFormControlTextarea1">Example textarea</label>
  <textarea class="form-control" id="exampleFormControlTextarea1" rows="3"></textarea>
</div>

<input class="form-control form-control-sm" type="text" placeholder=".form-control-sm">
<input class="form-control form-control-lg" type="text" placeholder=".form-control-lg">

<div class="row mb-3">                                              <!-- read-only value shown as text -->
  <label class="col-sm-2 col-form-label" for="staticEmail">Email</label>
  <div class="col-sm-10">
    <input class="form-control-plaintext" id="staticEmail" type="text" readonly value="email@example.com">
  </div>
</div>

<input class="form-control" id="formFile" type="file">              <!-- file inputs take form-control too -->
```

**3. Behaviour.** CSS only. `form-control` styles every text-like input, `textarea` and `type="file"`; `form-control-plaintext` replaces it (not adds to it) when a value is displayed rather than edited; `form-text` under an input is the small help line. `readonly` keeps the field in the tab order and submits its value, `disabled` does neither.

**4. In CoreUIDemo.** The real modal fields, with the two attribute directives that turn `readonly` / `disabled` into expressions. `ng-disabled="vm.ModalHeader === 'Edit'"` is what locks the username on edit; `ng-readonly` is the same idea for a value the user may copy but not change. `form-text` is what the update-status modal already uses for its "Enter your own password" hint.

**`Views/Settings/UserAccounts.cshtml`** — inside `#AccountModal`'s `modal-body`
```html
<div class="mb-3">
    <label class="form-label">Username</label>
    <input type="text" class="form-control" ng-model="vm.Modal.Username" ng-disabled="vm.ModalHeader === 'Edit'" />
    <div class="form-text" ng-show="vm.ModalHeader === 'Edit'">Usernames cannot be changed after creation.</div>  <!-- illustrative: not in the real modal -->
</div>
<div class="mb-3">
    <label class="form-label">First Name</label>
    <input type="text" class="form-control" id="firstName" ng-model="vm.Modal.FirstName" />
</div>
<div class="mb-3">
    <label class="form-label">Notes</label>                          <!-- illustrative: not on UserModel -->
    <textarea class="form-control" rows="3" ng-model="vm.Modal.Notes"></textarea>
</div>
<div class="row mb-3" ng-show="vm.ModalHeader === 'Edit'">
    <label class="col-sm-3 col-form-label">Account ID</label>
    <div class="col-sm-9">
        <input type="text" class="form-control-plaintext" ng-readonly="vm.ModalHeader === 'Edit'" ng-model="vm.Modal.ID" />
    </div>
</div>
```

The `id="firstName"` is there for the one piece of jQuery the page keeps — OneMasaito's letters-only keypress filter, which CoreUIDemo carries over unchanged (`App/Controller/UserAccounts.js` lines 85–100; full file in [`../BUILD_GUIDE.md` § 9.3](../BUILD_GUIDE.md#93-appcontrolleruseraccountsjs)):

**`App/Controller/UserAccounts.js`**
```js
        $("#firstName").keypress(function (event) {
            var inputValue = event.which;

            if (!(inputValue >= 65 && inputValue <= 90) && !(inputValue >= 97 && inputValue <= 122) && inputValue != 32) {
                event.preventDefault();
            }
        });
```

It runs once when the controller is constructed, which works because the modal's inputs are in the server-rendered view from the start (not inside an `ng-if` or `ng-repeat`). The same rule is enforced again in `Save()` with `namePattern.test(...)` and on the server, so the filter is a convenience, not the validation.

**5. Gotchas.** `ng-model` on a `type="file"` input does nothing — AngularJS 1.8 has no file binding; read `document.getElementById(id).files` in the controller when you need the file, and post it with `FormData`, not the JSON `$http` call the rest of the app uses. `form-control-plaintext` with `ng-model` still writes to the model if the `readonly` attribute is forgotten; keep both.

## Select

**1. What the dist page shows.** A `form-select` with a placeholder option, sizing (`form-select-lg` / `form-select-sm`), a `multiple` select, a `size="3"` list box and a disabled select. Dist: `forms/select.html`.

**2. Essential markup.**

**`forms/select.html`**
```html
<select class="form-select" aria-label="Default select example">   <!-- form-select, never form-control, on a select -->
  <option selected>Open this select menu</option>
  <option value="1">One</option>
  <option value="2">Two</option>
  <option value="3">Three</option>
</select>

<select class="form-select form-select-sm">…</select>
<select class="form-select" multiple>…</select>                     <!-- list box; model becomes an array -->
```

**3. Behaviour.** CSS only. `form-select` draws the chevron as a background image and hides the native arrow; `multiple` and `size` drop the chevron and show the list.

**4. In CoreUIDemo.** The real Role dropdown binds a list of strings; a list of objects uses the `select as label for item in list` form, exactly as OneMasaito's views do (`ng-options="sta.ID as sta.Description for sta in vm.CSRStatusList"` in `AllAccountsMonitoring.cshtml`). The empty `<option value="">` is the placeholder AngularJS shows while the model is `null`; without it, AngularJS inserts its own `<option value="?">` and the select opens on a blank row that the user cannot select back to.

**`Views/Settings/UserAccounts.cshtml`**
```html
<div class="mb-3">
    <label class="form-label">Role</label>
    <select class="form-select" ng-model="vm.Modal.Role" ng-options="r for r in vm.RoleList"></select>
</div>
<div class="mb-3">                                                   <!-- illustrative: a Department lookup -->
    <label class="form-label">Department</label>
    <select class="form-select" ng-model="vm.Modal.DepartmentId"
            ng-options="d.Id as d.Name for d in vm.DepartmentList">
        <option value="">Select…</option>
    </select>
</div>
```

**`App/Controller/UserAccounts.js`**
```js
        vm.RoleList = ["user", "manager", "admin"];

        // illustrative: loaded once, the same way Init() loads the grid
        $http({ method: "POST", url: "/Settings/GetDepartments" }).then(function (data) {
            vm.DepartmentList = data.data.departmentList;            // [{ Id: 1, Name: "Finance" }, …]
        });
```

`d.Id as d.Name` stores the `Id` in `vm.Modal.DepartmentId` and shows the `Name`; select the item itself (`d as d.Name for d in vm.DepartmentList track by d.Id`) to store the whole object. With `multiple`, the model is an array of whichever the `as` clause selects.

**5. Gotchas.** OneMasaito also has `<option ng-repeat="…" value="{{project.Project}}">` inside a select. It works only for string values: `value="{{…}}"` is always a string, so a numeric `Id` comes back as `"3"` and never equals `3`, and the initial model is not matched until the repeat has rendered — so the select flashes blank on every edit. `ng-options` avoids both. `ng-change="RefreshList()"` still works alongside `ng-options`.

## Checks and radios

**1. What the dist page shows.** Checkboxes, an indeterminate checkbox, disabled states, radios, switches (`form-switch`, plus `form-switch-lg` / `form-switch-xl`), inline checks, checks without labels, and toggle buttons (`btn-check`) singly and in groups. Dist: `forms/checks-radios.html`.

**2. Essential markup.**

**`forms/checks-radios.html`**
```html
<div class="form-check">                                            <!-- wrapper: input + label pair -->
  <input class="form-check-input" id="flexCheckDefault" type="checkbox">
  <label class="form-check-label" for="flexCheckDefault">Default checkbox</label>
</div>

<div class="form-check">
  <input class="form-check-input" id="flexRadioDefault1" type="radio" name="flexRadioDefault">
  <label class="form-check-label" for="flexRadioDefault1">Default radio</label>
</div>

<div class="form-check form-switch">                                <!-- same input, rendered as a switch -->
  <input class="form-check-input" id="flexSwitchCheckDefault" type="checkbox">
  <label class="form-check-label" for="flexSwitchCheckDefault">Default switch checkbox input</label>
</div>

<div class="form-check form-check-inline">…</div>                   <!-- side by side -->

<input class="btn-check" id="btn-check" type="checkbox" autocomplete="off">   <!-- hidden input… -->
<label class="btn btn-primary" for="btn-check">Single toggle</label>          <!-- …styled by its label -->
```

**3. Behaviour.** CSS only. `form-check-input` replaces the native box with a background image that reacts to `:checked`; `form-switch` on the wrapper changes that image to a slider; `form-check-inline` floats the wrappers; `form-check-reverse` (in `style.css`, not on the dist page) puts the label first. `btn-check` hides the input and lets the `:checked` state style the sibling `label.btn` — the pattern the [button group](./03-components.md#button-group) entry uses.

**4. In CoreUIDemo.** A checkbox binds a boolean; a radio group binds one value per input. `ng-value` (not `value`) keeps the model typed — `ng-value="true"` stores a boolean, `value="true"` stores the string `"true"`. The `name` attribute is still needed on radios so the browser knows they are one group.

**`Views/Settings/UserAccounts.cshtml`**
```html
<div class="form-check form-switch mb-3">                            <!-- illustrative: not in the real #AccountModal, see below -->
    <input class="form-check-input" id="isActive" type="checkbox" role="switch" ng-model="vm.Modal.IsActive" />  <!-- role="switch" is an addition; not on the dist page -->
    <label class="form-check-label" for="isActive">Active</label>
</div>

<div class="mb-3">                                                   <!-- Role as radios instead of a select -->
    <div class="form-check form-check-inline" ng-repeat="r in vm.RoleList">
        <input class="form-check-input" id="role-{{r}}" type="radio" name="role" ng-model="vm.Modal.Role" ng-value="r" />
        <label class="form-check-label" for="role-{{r}}">{{r}}</label>
    </div>
</div>

<input class="btn-check" id="showInactive" type="checkbox" autocomplete="off" ng-model="vm.ShowInactive" />
<label class="btn btn-outline-secondary" for="showInactive">Show inactive</label>
```

`vm.Modal.IsActive` is the real `bool` on `UserModel`, so the switch posts `true` / `false` with no conversion — but the switch itself is not in the real `#AccountModal`. `sp_UpdateUser` (called from `Services/UserService.cs` ~line 177: `db.sp_UpdateUser((int)_account.ID, _account.Username, _account.FirstName, _account.LastName, currentPassword, _role, _account.IsActive)`) does accept `IsActive`, so a switch here would technically save; but the real page keeps status changes on the grid's own buttons, which route through `AdminUpdateStatus` and require the admin's password to confirm before a status flips. A switch inside the save modal would let `IsActive` change alongside every other edit with no password confirmation, bypassing that flow. For a source that stores `"Y"` / `"N"` instead, `ng-true-value="'Y'" ng-false-value="'N'"` on the checkbox does the mapping (note the inner quotes — the attribute takes an expression). `vm.ShowInactive` is an illustrative page flag: `ng-repeat="acc in vm.AccountList | filter: (vm.ShowInactive ? '' : { IsActive: true })"` would use it.

**5. Gotchas.** OneMasaito's `ng-checked="true"` next to `ng-model` (`RPTMonitoring/Index.cshtml`) fights the model — `ng-checked` sets the DOM state without updating `vm`, so the box looks ticked while the model is `undefined`. Initialise the model in the controller (`vm.Modal = { Role: "user", IsActive: true }`) instead. Interpolated ids inside `ng-repeat` (`id="role-{{r}}"`) are fine for labels' `for`; two radio groups on one view need different `name`s.

## Range

**1. What the dist page shows.** A slider, a disabled slider, one with `min` / `max`, and one with `step="0.5"`. Dist: `forms/range.html`.

**2. Essential markup.**

**`forms/range.html`**
```html
<label class="form-label" for="customRange1">Example range</label>
<input class="form-range" id="customRange1" type="range">

<input class="form-range" id="customRange3" type="range" min="0" max="5" step="0.5">
```

**3. Behaviour.** CSS only. `form-range` restyles the track and thumb; `min` / `max` / `step` are the native attributes (defaults 0, 100, 1).

**4. In CoreUIDemo.** AngularJS 1.8 has a dedicated `input[range]` binding: the model is a number (not a string) and is clamped to `min` / `max`. There is no range on `UserModel`; this is an illustrative "session timeout" setting with the live value interpolated next to the label.

**`Views/Settings/UserAccounts.cshtml`**
```html
<div class="mb-3">
    <label class="form-label" for="timeout">Session timeout: {{vm.Modal.TimeoutMinutes}} min</label>
    <input class="form-range" id="timeout" type="range" min="5" max="120" step="5" ng-model="vm.Modal.TimeoutMinutes" />
</div>
```

**`App/Controller/UserAccounts.js`**
```js
        $scope.NewAccount = function () {
            vm.ModalHeader = "New";
            vm.Modal = { Role: "user", TimeoutMinutes: 30 };         // give the slider a start value
            ShowModal("AccountModal");
        };
```

**5. Gotchas.** An undefined model puts the thumb in the middle — and it does not stay blank: in browsers with native range support, AngularJS 1.8's range directive re-reads the DOM value through `$setViewValue(element.val())` after every `$render`, so an undefined `vm.Modal.TimeoutMinutes` is overwritten with the browser's midpoint value on the first render, before any drag. Always seed the model instead of leaving it undefined. AngularJS binds range on `input`, so the label updates while dragging, not only on release.

## Input group

**1. What the dist page shows.** Text add-ons before, after and on both sides of an input, two inputs sharing an add-on, an add-on on a textarea, wrapping, sizing (`input-group-sm` / `input-group-lg`), checkbox and radio add-ons, multiple inputs and add-ons, buttons and dropdowns as add-ons, and a select or file input inside the group. Dist: `forms/input-group.html`.

**2. Essential markup.**

**`forms/input-group.html`**
```html
<div class="input-group mb-3">
  <span class="input-group-text" id="basic-addon1">@</span>          <!-- add-on: a direct child, no wrapper -->
  <input class="form-control" type="text" placeholder="Username" aria-describedby="basic-addon1">
</div>

<div class="input-group input-group-sm mb-3">…</div>

<div class="input-group mb-3">
  <input class="form-control" type="text" placeholder="Recipient's username">
  <button class="btn btn-outline-secondary" id="button-addon2" type="button">Button</button>   <!-- button add-on -->
</div>
```

**3. Behaviour.** CSS only. `input-group` is a flex row; every direct child that is a `form-control`, `form-select`, `input-group-text` or `btn` gets its corners squared off where it touches a neighbour. Order in the DOM is the order on screen — there is no prepend/append class in Bootstrap 5.

**4. In CoreUIDemo.** The one input group already in the project is the header search box that [01 · Notification links (demo)](./01-layout.md#notification-links-demo) puts in `_Layout.cshtml`; it binds `main.SearchBox`, which every grid filters on. A button add-on takes `ng-click` like any other button.

**`Views/Shared/_Layout.cshtml`** — the real header search
```html
<div class="input-group ms-3" style="max-width:500px;">
    <input type="text" class="form-control" placeholder="Search for..." aria-label="Search" ng-model="main.SearchBox">
    <span class="input-group-text"><i class="cil-search"></i></span>
</div>
```

**`Views/Settings/UserAccounts.cshtml`** — a load-on-click filter, OneMasaito's `Project` + `Load` pattern in BS5 form
```html
<div class="input-group mb-3" style="max-width:400px;">
    <select class="form-select" ng-model="vm.SelectedRole" ng-options="r for r in vm.RoleList">
        <option value="">All roles</option>
    </select>
    <button class="btn btn-primary" type="button" ng-click="Init()"><i class="cil-search"></i> Load</button>
</div>
```

**5. Gotchas.** OneMasaito wraps that button in Bootstrap 4's append wrapper `div` (`AllAccountsMonitoring.cshtml` line 28). Under CoreUI the wrapper is not styled, so the button loses its joined corners and sits a few pixels off — drop the wrapper and put the button directly inside the group, per the rename table in [`../COREUI_GUIDE.md` § 10](../COREUI_GUIDE.md#10-bootstrap-4--bootstrap-5-class-renames). Inline `style="max-width:…"` is how the layout caps the header search; there is no `w-*` utility narrower than `w-25`.

## Floating labels

**1. What the dist page shows.** Inputs whose label sits inside the box and floats up once there is a value, across four cards: Basic (email and password inputs), Textareas, Selects, and Layout (a `row g-2` grid of floated fields). Dist: `forms/floating-labels.html`.

**2. Essential markup.**

**`forms/floating-labels.html`**
```html
<div class="form-floating mb-3">
  <input class="form-control" id="floatingInput" type="email" placeholder="name@example.com">   <!-- placeholder is required -->
  <label for="floatingInput">Email address</label>                                              <!-- label AFTER the input -->
</div>
<div class="form-floating">
  <input class="form-control" id="floatingPassword" type="password" placeholder="Password">
  <label for="floatingPassword">Password</label>
</div>
```

**3. Behaviour.** CSS only. `form-floating` positions the label over the input and uses `:placeholder-shown` (input empty) versus `:not(:placeholder-shown)` (has a value) to move it — so the `placeholder` attribute must be present, even if its text is never visible, and the `label` must come after the control. A `form-select` inside floats the same way. A textarea needs an explicit height because the label eats the default one.

**4. In CoreUIDemo.** A compact alternative to the label-above pattern for the login card in `Views/Home/Login.cshtml` — the real page uses `form-label` above each input (lines 33–47); this is the same two fields floated. `ng-model` needs nothing extra: AngularJS sets the value, the browser flips the placeholder state, the CSS moves the label.

**`Views/Home/Login.cshtml`** — floated variant of the real form
```html
<form class="row gap-3" autocomplete="off" novalidate>
    <div class="form-floating">
        <input class="form-control" id="username" type="text" placeholder="Username" ng-model="vm.Username" />
        <label for="username">Username</label>
    </div>
    <div class="form-floating">
        <input class="form-control" id="password" type="password" placeholder="Password" ng-model="vm.Password" />
        <label for="password">Password</label>
    </div>
    <div>
        <button class="btn btn-primary w-100" type="button" ng-click="TryLogin()">Login</button>
    </div>
</form>
```

**5. Gotchas.** Forget the `placeholder` and the label never floats — it stays over the typed text. `form-floating` does not combine with `form-control-sm` / `-lg` (the height is fixed by the label). Browser autofill on the login page may fill the value without firing `input` (behaviour varies by browser), so the label can float while `vm.Username` stays empty until the user touches the field; the real page's `form-label` layout avoids the mismatch, which is one reason it was kept.

## Layout

**1. What the dist page shows.** Stacked fields with `mb-3`, a `row g-3` grid of columns, a full sign-up form on the grid (`col-md-6`, `col-12`, `col-md-2`), horizontal forms with `col-form-label`, label sizing (`col-form-label` with a `-sm` / `-lg` suffix), column sizing and auto-sizing, and an inline form on `row row-cols-lg-auto g-3 align-items-center`. Dist: `forms/layout.html`.

**2. Essential markup.**

**`forms/layout.html`**
```html
<form class="row g-3">                                              <!-- g-3: gutter between columns AND rows -->
  <div class="col-md-6">
    <label class="form-label" for="inputEmail4">Email</label>
    <input class="form-control" id="inputEmail4" type="email">
  </div>
  <div class="col-md-6">
    <label class="form-label" for="inputPassword4">Password</label>
    <input class="form-control" id="inputPassword4" type="password">
  </div>
  <div class="col-12">
    <label class="form-label" for="inputAddress">Address</label>
    <input class="form-control" id="inputAddress" type="text" placeholder="1234 Main St">
  </div>
</form>

<form>                                                              <!-- horizontal: label and control on one row -->
  <div class="row mb-3">
    <label class="col-sm-2 col-form-label" for="inputEmail3">Email</label>   <!-- col-form-label: padding to line up with the input -->
    <div class="col-sm-10">
      <input class="form-control" id="inputEmail3" type="email">
    </div>
  </div>
</form>

<form class="row row-cols-lg-auto g-3 align-items-center">          <!-- inline: each col-12 shrinks to content at lg+ -->
  <div class="col-12">
    <label class="visually-hidden" for="inlineFormSelectPref">Preference</label>
    <select class="form-select" id="inlineFormSelectPref">…</select>
  </div>
  <div class="col-12">
    <button class="btn btn-primary" type="submit">Submit</button>
  </div>
</form>
```

**3. Behaviour.** CSS only. Forms lay out with the ordinary grid — `row` + `col-*` — plus two classes specific to forms: `col-form-label` matches a label's line height and padding to the input beside it, and `row-cols-lg-auto` makes every column shrink-to-fit above the `lg` breakpoint (below it the `col-12`s stack). Bootstrap 4's row and inline form classes are gone; `row g-3` and `row-cols-lg-auto` replace them ([`../COREUI_GUIDE.md` § 10](../COREUI_GUIDE.md#10-bootstrap-4--bootstrap-5-class-renames)).

**4. In CoreUIDemo.** The real `#AccountModal` stacks five fields on `mb-3`. Widening the dialog to `modal-lg` and putting the two names side by side is the same markup on the grid — nothing in the controller changes, because `ng-model` does not care where the input sits.

**`Views/Settings/UserAccounts.cshtml`** — two-column version of the CRUD modal body
```html
<div class="modal fade" id="AccountModal" tabindex="-1" aria-hidden="true">
    <div class="modal-dialog modal-lg">
        <div class="modal-content">
            <div class="modal-header">
                <h4 class="modal-title">{{vm.ModalHeader}} Account</h4>
                <button type="button" class="btn-close" data-coreui-dismiss="modal" aria-label="Close"></button>
            </div>
            <div class="modal-body">
                <div class="row g-3">
                    <div class="col-md-6">
                        <label class="form-label">Username</label>
                        <input type="text" class="form-control" ng-model="vm.Modal.Username" ng-disabled="vm.ModalHeader === 'Edit'" />
                    </div>
                    <div class="col-md-6" ng-show="vm.ModalHeader === 'New'">
                        <label class="form-label">Password</label>
                        <input type="password" class="form-control" ng-model="vm.Modal.Password" />
                    </div>
                    <div class="col-md-6">
                        <label class="form-label">First Name</label>
                        <input type="text" class="form-control" id="firstName" ng-model="vm.Modal.FirstName" />
                    </div>
                    <div class="col-md-6">
                        <label class="form-label">Last Name</label>
                        <input type="text" class="form-control" id="lastName" ng-model="vm.Modal.LastName" />
                    </div>
                    <div class="col-md-6">
                        <label class="form-label">Role</label>
                        <select class="form-select" ng-model="vm.Modal.Role" ng-options="r for r in vm.RoleList"></select>
                    </div>
                    <div class="col-md-6 d-flex align-items-end">
                        <div class="form-check form-switch">                     <!-- illustrative addition; see the checks-radios entry -->
                            <input class="form-check-input" id="isActive" type="checkbox" role="switch" ng-model="vm.Modal.IsActive" />
                            <label class="form-check-label" for="isActive">Active</label>
                        </div>
                    </div>
                </div>
            </div>
            <div class="modal-footer">
                <button type="button" class="btn btn-primary" ng-click="Save()">Save</button>
            </div>
        </div>
    </div>
</div>
```

`ng-show` on a `col-md-6` hides the column and the grid closes the gap, so on Edit the username takes the left half and First Name moves up beside it — acceptable here; give the password column `ng-if` instead if the layout must stay put (the column is removed rather than hidden, same visual result, but see the [form-control gotcha](#form-control) about keypress filters and `ng-if`). A horizontal layout (`row mb-3` + `col-sm-3 col-form-label` + `col-sm-9`) suits a settings page with long labels better than a modal.

**5. Gotchas.** `g-3` on the row replaces the `mb-3` on each field — keep both and the vertical spacing doubles. The `d-flex align-items-end` on the switch's column is what lines a label-less control up with the inputs beside it; without it the switch sits at the top of the cell. The switch itself is an addition for this layout example, not part of the real `#AccountModal` — see [Checks and radios § 4](#checks-and-radios) for why status stays on the grid's buttons instead.

## Chip input

**1. What the dist page shows.** A tag editor: typed values become removable chips inside a `form-control`-like box. Basic use with a label and preset chips, contextual chip colours (`chip-primary`, `chip-outline`, …), sizes (`chip-input-sm` / `chip-input-lg`), an empty editor, a disabled and a read-only editor, and two editors inside a `row g-3` layout. Dist: `forms/chip-input.html`.

**2. Essential markup.**

**`forms/chip-input.html`**
```html
<div class="chip-input" data-coreui-chip-input data-coreui-name="skills" data-coreui-placeholder="Add a skill...">
  <span class="chip">JavaScript</span>                              <!-- preset values: plain chips inside the box -->
  <span class="chip">TypeScript</span>
  <span class="chip">Accessibility</span>
</div>

<div class="chip-input chip-input-sm mb-3" data-coreui-chip-input data-coreui-placeholder="Add small tag..."></div>
<div class="chip-input mb-3" data-coreui-chip-input data-coreui-disabled="true" data-coreui-removable="false" data-coreui-placeholder="Input disabled">…</div>
<div class="chip-input" data-coreui-chip-input data-coreui-readonly="true" data-coreui-placeholder="Read-only values">…</div>
```

The dist also places a `<label for="…">` as the first child of the box; the component reads its `for` to id the text field it creates. That label's class has no rule in `style.css`, so it is left out here — a `form-label` above the box does the same job.

**3. Behaviour.** `coreui.ChipInput` — CoreUI-only, no Bootstrap equivalent. On `DOMContentLoaded` every `[data-coreui-chip-input]` is instantiated; the component appends a text field and a `<input type="hidden">` whose `name` is `data-coreui-name` and whose value is the chips joined with `,`. Attributes the dist page uses: `data-coreui-name`, `data-coreui-placeholder`, `data-coreui-disabled`, `data-coreui-readonly`, `data-coreui-removable` (the `data-coreui-theme` / `data-coreui-toggle` hits on that page belong to the shell, not the component). Verified in `coreui.bundle.min.js`: Enter or the separator (`,`) turns the text into a chip, blur does too (`createOnBlur` default `true`), Backspace on an empty field focuses the last chip; instance methods `add(value)`, `remove(valueOrEl)`, `clear()`, `getValues()` (returns an array) and `getSelectedValues()`; further options `maxChips`, `separator`, `selectable`, `chipClassName` are constructor config only. Events on the element: `add`, `remove`, `change` (carries `values`), `select`, `input`, each suffixed `.coreui.chip-input`. There is no `.value` property on the instance — `getValues()` is the read API.

**4. In CoreUIDemo.** The component owns the chips' DOM, so `ng-model` and `ng-repeat` stay out of the box; the controller talks to the instance at the two moments it matters — filling it when the modal opens, reading it on save. Illustrative: `Skills` is not on `UserModel`.

**`Views/Settings/UserAccounts.cshtml`**
```html
<div class="mb-3">
    <label class="form-label">Skills</label>
    <div class="chip-input" id="skills" data-coreui-chip-input data-coreui-name="skills" data-coreui-placeholder="Add a skill..."></div>
</div>
```

**`App/Controller/UserAccounts.js`**
```js
        var skills = function () {
            return coreui.ChipInput.getOrCreateInstance(document.getElementById("skills"));
        };

        $scope.EditAccount = function (value) {
            vm.ModalHeader = "Edit";
            vm.Modal = angular.copy(value);
            skills().clear();
            angular.forEach(vm.Modal.Skills || [], function (s) { skills().add(s); });
            ShowModal("AccountModal");
        };

        $scope.Save = function () {
            vm.Modal.Skills = skills().getValues();                     // ["JavaScript", "TypeScript"]
            // …the existing if / else-if checks, then the $http POST
        };
```

`getOrCreateInstance` rather than `getInstance` so the helper also works if the box was added after `DOMContentLoaded`. To react as chips change (a live count, say), listen for `change.coreui.chip-input` on the element and wrap the model update in `$scope.$applyAsync` — the event is fired by CoreUI, outside AngularJS's digest.

**5. Gotchas.** `clear()` and `add()` each fire `change.coreui.chip-input`; do the fill before `ShowModal` so the events run while the modal is hidden. `add()` returns `null` for a duplicate, an empty string, or when `maxChips` is reached — it does not throw. The hidden input's `name` is only useful for a classic form post, which the app never does; `getValues()` is the path to `vm.Modal`.

## Validation

**1. What the dist page shows.** A `row g-3` form with `required` inputs, a select, a checkbox and an input group, each followed by `valid-feedback` / `invalid-feedback`; the same form with tooltip feedback (`invalid-tooltip`); browser-default validation; and server-side style with `is-valid` / `is-invalid` set on the inputs. Dist: `forms/validation.html`.

**2. Essential markup.**

**`forms/validation.html`**
```html
<form class="row g-3 needs-validation" novalidate>                  <!-- novalidate: no browser bubbles; needs-validation: the JS hook -->
  <div class="col-md-4">
    <label class="form-label" for="validationCustom01">First name</label>
    <input class="form-control" id="validationCustom01" type="text" value="Mark" required>
    <div class="valid-feedback">Looks good!</div>
  </div>
  <div class="col-md-4">
    <label class="form-label" for="validationCustomUsername">Username</label>
    <div class="input-group has-validation">                        <!-- has-validation: feedback inside a group -->
      <span class="input-group-text" id="inputGroupPrepend">@</span>
      <input class="form-control" id="validationCustomUsername" type="text" required>
      <div class="invalid-feedback">Please choose a username.</div>
    </div>
  </div>
  <div class="col-12">
    <button class="btn btn-primary" type="submit">Submit form</button>
  </div>
</form>
```

**3. Behaviour.** CSS plus one page script. `invalid-feedback` / `valid-feedback` are `display: none` until either (a) the form has `was-validated` and the sibling control is `:invalid` / `:valid` by HTML5 rules, or (b) the control itself carries `is-invalid` / `is-valid`. Route (a) is what the dist page wires up with this script, quoted in full:

**`forms/validation.html`** — the page's own `<script>`
```js
      // Example starter JavaScript for disabling form submissions if there are invalid fields
      (function () {
        "use strict";
        // Fetch all the forms we want to apply custom Bootstrap validation styles to
        var forms = document.querySelectorAll(".needs-validation");
        // Loop over them and prevent submission
        Array.prototype.slice.call(forms).forEach(function (form) {
          form.addEventListener(
            "submit",
            function (event) {
              if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
              }
              form.classList.add("was-validated");
            },
            false,
          );
        });
      })();
```

`needs-validation` has no CSS of its own; it is only the selector this script looks for.

**4. In CoreUIDemo.** The project uses none of the above. Its buttons are `type="button"` with `ng-click`, so no `submit` event ever fires and the dist script would never run; and the forms are `novalidate` (`Login.cshtml` line 33) so the browser stays silent too. Validation is OneMasaito's convention — sequential checks in the controller, one `growl.error` per failure, the first failure stops the chain — exactly as the real `Save()` does (`App/Controller/UserAccounts.js` lines 39–65):

**`App/Controller/UserAccounts.js`** — the real shape, trimmed
```js
        $scope.Save = function () {

            if (vm.Modal.Username == "" || vm.Modal.Username == null) {
                growl.error("Please input Username", { ttl: 5000 });
            }
            else if (vm.ModalHeader === "New" && (vm.Modal.Password == "" || vm.Modal.Password == null)) {
                growl.error("Please input Password", { ttl: 5000 });
            }
            else if (vm.ModalHeader === "New" && vm.Modal.Password.length < 6) {
                growl.error("Password must be at least 6 characters", { ttl: 5000 });
            }
            else if (vm.Modal.FirstName == "" || vm.Modal.FirstName == null) {
                growl.error("Please input First Name", { ttl: 5000 });
            }
            else if (!namePattern.test(vm.Modal.FirstName)) {
                growl.error("First Name must contain letters only", { ttl: 5000 });
            }
            // …Last Name, Role…
            else {
                $http({ method: "POST", url: "/Settings/SaveNewAccount", data: { account: vm.Modal, role: vm.Modal.Role } })
                    .then(function (data) {
                        PopUpMessage(data.data);
                        $scope.Init();
                        if (data.data.message == "Saved") {
                            HideModal("AccountModal");
                        }
                    });
            }
        };
```

The server repeats the client-side checks it can — the name-pattern check and the password-length check, both in `Services/UserService.cs`'s `SaveAccount` (`Regex.IsMatch(_account.FirstName ?? "", NamePattern)` / `...LastName...`, then `_account.Password.Length < PasswordMinLength`) — plus a duplicate-username check that has no client-side equivalent, in `Controllers/SettingsController.cs`'s `SaveNewAccount` (`UserService.CheckUserNameDuplicate(account.Username)`, checked before `SaveAccount` is even called). Either layer answers `{ message }`; `PopUpMessage` (`App/App.js` line 17) growls it, and the modal stays open on anything but `"Saved"`. (`Services/AccountService.cs` is unrelated — it holds `LoginToSession` / `LogoutFromSession`, not account validation.) The browser copy is for immediacy, the server copy is the one that holds.

A middle path, when a field should also turn red: keep the growl chain, add a `vm.Submitted` flag, and let `ng-class` apply `is-invalid` — route (b) above, no `was-validated`, no script. The feedback `div` shows because the sibling has the class, and `ng-show` keeps AngularJS in charge of the message.

**`Views/Settings/UserAccounts.cshtml`**
```html
<div class="mb-3">
    <label class="form-label">Username</label>
    <input type="text" class="form-control" ng-model="vm.Modal.Username"
           ng-class="{ 'is-invalid': vm.Submitted && !vm.Modal.Username }" />
    <div class="invalid-feedback" ng-show="vm.Submitted && !vm.Modal.Username">Please input Username</div>
</div>
```

**`App/Controller/UserAccounts.js`**
```js
        $scope.NewAccount = function () {
            vm.Submitted = false;                                    // reset when the modal is (re)opened
            vm.ModalHeader = "New";
            vm.Modal = { Role: "user" };
            ShowModal("AccountModal");
        };

        $scope.Save = function () {
            vm.Submitted = true;                                     // first line; the growl chain follows unchanged
            if (vm.Modal.Username == "" || vm.Modal.Username == null) {
                growl.error("Please input Username", { ttl: 5000 });
            }
            // …
        };
```

CoreUIDemo uses the growl convention alone. The middle path is documented because it costs one flag and no new dependencies if a page ever needs inline marks; the dist's `needs-validation` route is not used anywhere in the project.

**5. Gotchas.** AngularJS adds its own `ng-invalid` / `ng-valid` classes to inputs with `required`, and nothing in `style.css` styles them — only `is-invalid` / `is-valid` render. `was-validated` on a form whose inputs have `required` marks every empty field red at once, including the password field hidden by `ng-show` (hidden inputs still count for `checkValidity()`); the `vm.Submitted` route marks only what its expressions name. `invalid-feedback` only needs to be a *later* sibling of the control within the same parent (or inside a `has-validation` input group) — `style.css`'s rules (`.is-invalid ~ .invalid-feedback`, `.was-validated :invalid ~ .invalid-feedback`) use the general sibling combinator `~`, which matches any sibling after the control, not just the very next one, so a `form-text` in between does not break it. What does break it: putting the feedback `div` inside a different wrapper than the control — `~` only matches siblings sharing the same parent, so a feedback element nested one level deeper, or in a neighbouring column `div`, never shows.
