# 02 · Authentication pages

The six pages under `authentication/` in the dist are the only ones that do not use the shell from [01-layout.md](./01-layout.md): no sidebar, no header, just a centred card on a tinted background. CoreUIDemo has exactly one of them today (`Views/Home/Login.cshtml`); this chapter documents the frame they all share, one entry per dist page in the [usual five-part shape](./README.md#how-to-read-an-entry), the show/hide-password button, and the gotchas of pages that run without `_Layout.cshtml`.

Endpoints that `Controllers/HomeController.cs` really has: `GET Login`, `POST Login`, `POST Logout`, `POST ChangePassword`, `GetCurrentUser`, `Index`. Every other action named below (`Register`, `ResetPassword`, `CheckEmail`, `PasswordChanged`) is **not implemented in CoreUIDemo** — those entries show the page and the AngularJS module skeleton with a hypothetical endpoint name, nothing more.

- [The shared frame](#the-shared-frame)
- [Login](#login)
- [Register](#register)
- [Reset password](#reset-password)
- [Check email](#check-email)
- [Change password](#change-password)
- [Password changed](#password-changed)
- [Show/hide password button](#showhide-password-button)
- [Gotchas](#gotchas)

## The shared frame

The first 40 lines of `login.html` and `register.html` are byte-identical (`diff` prints nothing): the same `<head>`, the same stylesheet links, the same `js/config.js` and `js/color-modes.js` script tags — all six dist auth pages load `color-modes.js`. But none of them has a `[data-coreui-theme-value]` button, which is what that script's `DOMContentLoaded` listener looks for, so on these pages it throws a `TypeError` instead of doing anything useful (see [08 · Customizing](./08-customizing.md)'s "Turning things off" note and [`../COREUI_GUIDE.md` § 9](../COREUI_GUIDE.md#9-dark-mode)). That is why CoreUIDemo's `Login.cshtml` drops the script entirely and pins `data-coreui-theme="light"` instead (or uses the § 9 one-liner). Below `<body>` all six pages open with the same three wrappers and the same card; only the card contents, the footer line, and the container width differ (`32rem` for login and register, `28rem` for the four narrower pages).

**`authentication/login.html`** — the frame, contents elided
```html
<div class="bg-body-tertiary min-vh-100 d-flex flex-row align-items-center">   <!-- full-height, vertically centred -->
  <div class="container" style="max-width: 32rem">                            <!-- inline max-width, no custom CSS -->
    <div class="d-flex flex-column gap-4">                                     <!-- logo / card / footer stack -->
      <svg role="img" aria-label="CoreUI Logo Full" height="48" …>…</svg>      <!-- CoreUIDemo: <img> of the Masaito logo -->
      <div class="card p-4">
        <div class="card-body d-flex flex-column gap-4">
          <h2 class="h5 text-center">Login to your account</h2>
          …fields…
        </div>
      </div>
      <div class="text-center text-body-secondary">Need an account? <a href="…">Sign up</a></div>
    </div>
  </div>
</div>
```

Every page other than login adds `text-center` to the stack `div` (register included); the pages with a form — register, reset password, change password — also add `text-start` to the field container so headings centre while labels stay left. The only `data-coreui-*` attributes in the whole folder are `data-coreui-toggle="tooltip"` and `data-coreui-original-title` — both on the eye button, see [below](#showhide-password-button). Nothing else needs JavaScript.

**In CoreUIDemo** the outer `div` becomes the `<body>` itself: `<body class="bg-body-tertiary min-vh-100 d-flex flex-row align-items-center" ng-controller="loginController as vm">` (`Views/Home/Login.cshtml` line 21), so the view sets `Layout = null` at the top and carries its own `<head>` with the three bundles rendered in the same order as `_Layout.cshtml` (`~/Content/css`, `~/bundles/scripts`, `~/bundles/angular`). `<html>` carries `ng-app="login"` — its own AngularJS module, not `app`, because `mainController` would immediately call `/Home/GetCurrentUser` on a page that by definition has no session. The reasons, and what was cut from the dist page, are in [`../COREUI_GUIDE.md` § 6](../COREUI_GUIDE.md#6-login-page--authenticationloginhtml--logincshtml); do not repeat them in a new view, copy `Login.cshtml`. The page is pinned to `data-coreui-theme="light"` because it never loads `color-modes.js`; the one-liner that makes it follow the saved theme instead is in [`../COREUI_GUIDE.md` § 9](../COREUI_GUIDE.md#9-dark-mode) ("Login page follows the saved theme") and applies unchanged to every page in this chapter.

## Login

**1. What the dist page shows.** Email + password (with the eye button), "I forgot password" link beside the password label, "Remember me" checkbox, a full-width "Sign in" button, an "or" divider, two `btn btn-outline w-100` social buttons, and a "Need an account?" footer. Dist: `authentication/login.html`.

**2. Essential markup.** The frame above, plus:

**`authentication/login.html`** — card contents
```html
<form class="row gap-3" action="./" method="get" autocomplete="off" novalidate>   <!-- CoreUIDemo: no action, no method -->
  <div>
    <label class="form-label" for="email">Email address</label>
    <input class="form-control" id="email" type="email" placeholder="your@email.com" autocomplete="off">
  </div>
  <div>
    <div class="d-flex justify-content-between">                                <!-- label left, link right -->
      <label class="form-label" for="password">Password</label>
      <a href="./authentication/reset-password.html">I forgot password</a>
    </div>
    <div class="input-group">…password + eye button, see below…</div>
  </div>
  <div><label class="form-check">…"Remember me" checkbox, same `form-check` markup as register's terms checkbox…</label></div>
  <div><button class="btn btn-primary w-100" type="submit">Sign in</button></div>
</form>
```

`row gap-3` is the field layout used on every page here: each field is a bare `div` child of the `row`, so it becomes a full-width column with a `1rem` gap — no `mb-3`, no `col-*`.

**3. Behaviour.** CSS only, except the tooltip on the eye button (`coreui.Tooltip`, initialised by the inline script at the end of `login.html`).

**4. In CoreUIDemo.** Already built: [`Views/Home/Login.cshtml`](../../Views/Home/Login.cshtml) and [`App/Controller/Login.js`](../../App/Controller/Login.js), mapped line by line in [`../COREUI_GUIDE.md` § 6](../COREUI_GUIDE.md#6-login-page--authenticationloginhtml--logincshtml). The pattern every other page in this chapter copies: `ng-model="vm.Username"` / `vm.Password` on the inputs, `ng-submit="TryLogin()"` on the `<form>` (no `action`) with a `type="submit"` button, `$http` POST to `/Home/Login`, `growl.error` on a non-empty `errorMessage`, `window.location.href` on success.

**5. Gotchas.** Enter submits because the `<form>` has `ng-submit` and the button is `type="submit"`; AngularJS's `ngSubmit` calls `preventDefault()` only when the form has no `action`, so never add one. (OneMasaito's `Login.js` bound Enter with `$(document).on('keypress', …)` instead — CoreUIDemo dropped that.) The dist's `btn btn-outline` (no colour suffix) is a real class in `style.css`, and its ruleset does define a hover state — the hover background and text pick up the tertiary-background and body-text CSS custom properties, i.e. a subtle tint rather than a colour change. CoreUIDemo dropped the social buttons because OneMasaito's login has none, not because the class was unstyled ([`../COREUI_GUIDE.md` § 6](../COREUI_GUIDE.md#6-login-page--authenticationloginhtml--logincshtml)).

## Register

**1. What the dist page shows.** Name, email, password (eye button), an "I accept the terms and conditions" checkbox, "Create new account", the same divider + social buttons, and "Already have an account? Sign in". Dist: `authentication/register.html`. Its `action` points at `check-email.html`.

**2. Essential markup.** Same frame as login with `text-center` on the stack; the fields differ only by the first input and the checkbox label:

**`authentication/register.html`** — the parts that differ from login
```html
<form class="row gap-3 text-start" action="./authentication/check-email.html" method="get" autocomplete="off" novalidate>
  <div>
    <label class="form-label" for="name">Name</label>
    <input class="form-control" id="name" type="text" placeholder="Your name" autocomplete="off">
  </div>
  …email, password as on login…
  <div>
    <label class="form-check">
      <input class="form-check-input" type="checkbox">
      <span class="form-check-label">I accept the <a href="#">terms and conditions</a></span>
    </label>
  </div>
  <div><button class="btn btn-primary w-100" type="submit">Create new account</button></div>
</form>
```

**3. Behaviour.** CSS only (tooltip on the eye button as on login; `register.html` itself does not even include the tooltip init script).

**4. In CoreUIDemo — not implemented; sketch.** Copy `Login.cshtml` to `Views/Home/Register.cshtml`, change `ng-app="login"` to `ng-app="register"`, the controller to `registerController as vm`, the title, and the card contents. One `ng-model` per field, `ng-model="vm.AcceptTerms"` on the checkbox, and the button calls `Register()`:

**`Views/Home/Register.cshtml`** — card contents; frame and `<head>` as `Login.cshtml`
```html
<form class="row gap-3 text-start" autocomplete="off" novalidate>
    <div>
        <label class="form-label" for="name">Name</label>
        <input class="form-control" id="name" type="text" placeholder="Your name" ng-model="vm.Name" />
    </div>
    <div>
        <label class="form-label" for="username">Username</label>
        <input class="form-control" id="username" type="text" placeholder="Choose a username" ng-model="vm.Username" />
    </div>
    <div>
        <label class="form-label" for="password">Password</label>
        <div class="input-group">…the show/hide field from the section below, ng-model="vm.Password"…</div>
    </div>
    <div>
        <label class="form-check">
            <input class="form-check-input" type="checkbox" ng-model="vm.AcceptTerms" />
            <span class="form-check-label">I accept the <a href="#">terms and conditions</a></span>
        </label>
    </div>
    <div><button class="btn btn-primary w-100" type="button" ng-click="Register()">Create new account</button></div>
</form>
```

**`App/Controller/Register.js`** — module name matches `ng-app`; `/Home/Register` is hypothetical
```js
angular.module("register", ["angular-growl"])
    .controller("registerController", ['$scope', '$location', '$http', 'growl', function ($scope, $location, $http, growl) {
        var vm = this;
        vm.AcceptTerms = false;

        $scope.Register = function () {
            if (!vm.AcceptTerms) {
                growl.error("Please accept the terms and conditions", { title: "Error!", ttl: 3000 });
                return;
            }
            $http({
                method: "POST",
                url: "/Home/Register",                       // does not exist in HomeController.cs
                data: { name: vm.Name, username: vm.Username, password: vm.Password }
            }).then(function (data) {
                if (data.data.errorMessage != "") {
                    growl.error(data.data.errorMessage, { title: "Error!", ttl: 3000 });
                }
                else {
                    window.location.href = "/Home/CheckEmail";
                }
            });
        };
    }]);
```

The module depends on `angular-growl` only, exactly as `Login.js` does — not on `app`, which is the layout's module. Add the file to the `~/bundles/angular` bundle in [`App_Start/BundleConfig.cs`](../../App_Start/BundleConfig.cs) next to `Login.js`; the bundle lists controller files by name. The server side, when written, is a `[HttpPost] JsonResult Register(...)` returning `Json(new { errorMessage = … })` like `Login`. The `Register` action does not exist in `HomeController.cs` — flagged so nobody wires the button to a 404. Sequential `if` validation with `growl.error` is the OneMasaito idiom; the `is-invalid` / `invalid-feedback` alternative is in [04 · Validation](./04-forms.md#validation).

## Reset password

**1. What the dist page shows.** A `28rem` card: "Reset password" heading, a sentence of instructions, one email field, and a full-width button with a send icon — "Send reset instructions". Dist: `authentication/reset-password.html`; `action` points at `check-email.html`.

**2. Essential markup.**

**`authentication/reset-password.html`** — below the `h2.h5` + `text-body-secondary` intro `div`
```html
<form class="row gap-3 text-start" action="./authentication/check-email.html" method="get" autocomplete="off" novalidate>
  <div>
    <label class="form-label" for="email">Email address</label>
    <input class="form-control" id="email" type="email" placeholder="your@email.com" autocomplete="off">
  </div>
  <div>
    <button class="btn btn-primary w-100" type="submit"><svg class="icon me-2">…</svg>Send reset instructions</button>
  </div>
</form>
```

**3. Behaviour.** CSS only.

**4. In CoreUIDemo — not implemented; sketch.** `Views/Home/ResetPassword.cshtml` is `Login.cshtml` with `ng-app="resetpassword"`, `ng-controller="resetPasswordController as vm"`, `style="max-width: 28rem"` on the container, and this card body. The icon is the font glyph, since the dist ships no free-icon SVG sprite ([05-icons.md](./05-icons.md)).

**`Views/Home/ResetPassword.cshtml`** — card contents
```html
<div>
    <h2 class="h5">Reset password</h2>
    <div class="text-body-secondary">Enter your email address and we will send you instructions on how to reset your password.</div>
</div>
<form class="row gap-3 text-start" autocomplete="off" novalidate>
    <div>
        <label class="form-label" for="email">Email address</label>
        <input class="form-control" id="email" type="email" placeholder="your@email.com" ng-model="vm.Email" />
    </div>
    <div>
        <button class="btn btn-primary w-100" type="button" ng-click="SendReset()"><i class="cil-send me-2"></i>Send reset instructions</button>
    </div>
</form>
```

**`App/Controller/ResetPassword.js`** — same shape as `Register.js`; `/Home/ResetPassword` is hypothetical
```js
angular.module("resetpassword", ["angular-growl"])
    .controller("resetPasswordController", ['$scope', '$location', '$http', 'growl', function ($scope, $location, $http, growl) {
        var vm = this;
        $scope.SendReset = function () {
            $http({ method: "POST", url: "/Home/ResetPassword", data: { email: vm.Email } })   // not in HomeController.cs
                .then(function (data) { /* growl.error on errorMessage, else window.location.href = "/Home/CheckEmail" */ });
        };
    }]);
```

Add the file to `~/bundles/angular` as for register. Note that CoreUIDemo's `UserModel` has a username, not an email — the `loginDemo` schema would need an email column and a token table before this page is more than a sketch.

## Check email

**1. What the dist page shows.** A `28rem` card with a heading, a sentence containing the address in `fw-semibold`, a full-width "Back to Home" button, and a "Didn't receive an email? Resend" footer. Dist: `authentication/check-email.html`.

**2. Essential markup.**

**`authentication/check-email.html`** — card contents
```html
<h2 class="h5 mb-0">Check your email</h2>
<div class="text-body-secondary">Please click the link sent to <span class="fw-semibold">your@email.com</span> to verify your account. Thank you!</div>
<div><a class="btn btn-primary w-100" href="./">Back to Home</a></div>
```

**3. Behaviour.** CSS only. `a.btn` instead of `button` because it is a link.

**4. In CoreUIDemo — not implemented; minimal view.** A static page: `Layout = null`, no `ng-app`, no controller file. It still needs a plain MVC action to be served (`public ActionResult CheckEmail() { return View(); }` — not in `HomeController.cs` today), and it only needs the `~/Content/css` bundle. To show the address, pass it as `ViewBag.Email` from the action, or leave the wording generic as below.

**`Views/Home/CheckEmail.cshtml`** — complete file
```html
@{
    Layout = null;
}
<!DOCTYPE html>
<html data-coreui-theme="light">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0, shrink-to-fit=no" />
    @Styles.Render("~/Content/css")            @* no script bundles: nothing on the page runs *@
    <title>Check your email</title>
</head>
<body class="bg-body-tertiary min-vh-100 d-flex flex-row align-items-center">
    <div class="container" style="max-width: 28rem">
        <div class="d-flex flex-column gap-4 text-center">
            <div><img style="height:48px;" src="~/Src/Image/masaito-logo-light-gradient.svg" alt="Masaito Development Corporation" /></div>  @* logo as Login.cshtml *@
            <div class="card p-4">
                <div class="card-body d-flex flex-column gap-4">
                    <h2 class="h5 mb-0">Check your email</h2>
                    <div class="text-body-secondary">Please click the link we sent you to finish setting up your account.</div>
                    <div><a class="btn btn-primary w-100" href="~/Home/Login">Back to Login</a></div>
                </div>
            </div>
        </div>
    </div>
</body>
</html>
```

**5. Gotchas.** With no AngularJS on the page there is no `growl`, so there is nothing to report errors with; anything that can fail belongs on the page before this one.

## Change password

**1. What the dist page shows.** A `28rem` card titled "Reset password" — the dist reuses the heading — with "New password" and "Confirm new password", each an `input-group` with the eye button, and "Set new password". Dist: `authentication/change-password.html`; `action` points back at `login.html`. There is no "current password" field: the dist page is the last step of the forgot-password flow, reached from an email link.

**2. Essential markup.**

**`authentication/change-password.html`** — one of the two structurally identical fields
```html
<div>
  <label class="form-label" for="new-password">New password</label>
  <div class="input-group">…`form-control` + eye button, verbatim in the show/hide section below…</div>
</div>
```

**3. Behaviour.** CSS only; the tooltip needs `coreui.Tooltip` init, which this dist page does not include either.

**4. In CoreUIDemo.** Not a page. A logged-in user changes the password from the header's user dropdown, which opens the `#PasswordModal` in [`Views/Shared/_Layout.cshtml`](../../Views/Shared/_Layout.cshtml) (lines 164–193): three `form-control` inputs bound to `main.ChangePassword.CurrentPassword` / `.NewPassword` / `.ConfirmPassword` inside a `<form ng-submit="ChangePassword(main.ChangePassword)">` with a `type="submit"` button. That function lives on `mainController` in [`App/App.js`](../../App/App.js): length ≥ 6 and match checks with `growl.error`, then `POST /Home/ChangePassword` with `{ password: value }` — a real action, `HomeController.ChangePassword(ChangePasswordModel)` — and `HideModal("PasswordModal")` on success. `OpenPasswordModal()` on the dropdown item clears the three fields before the modal opens. Modals themselves are in [03 · Modals](./03-components.md#modals). If a standalone page is ever needed (forced reset from an email link), it is the [reset-password](#reset-password) sketch with two password fields, `vm.NewPassword` / `vm.ConfirmPassword`, the two `growl.error` checks copied from `App.js`, and a hypothetical `POST /Home/SetNewPassword` that reads its token from `window.location.search` (`$location.search()` needs html5Mode or a `#!` URL).

**5. Gotchas.** The modal's body and footer are wrapped in a `<form ng-submit=…>`, so its *Change Password* button is deliberately `type="submit"` and *Cancel* must stay `type="button"` — a Cancel without an explicit type would submit the form. The form has no `action`, which is what lets `ngSubmit` prevent the page reload.

## Password changed

**1. What the dist page shows.** A `28rem` card: "Your password has been changed", a confirmation sentence, "Back to Login". Dist: `authentication/password-changed.html`.

**2. Essential markup.**

**`authentication/password-changed.html`** — card contents
```html
<h2 class="h5 mb-0">Your password has been changed</h2>
<div class="text-body-secondary">Your password has been successfully changed. You can now log in with your new password.</div>
<div><a class="btn btn-primary w-100" href="./authentication/login.html">Back to Login</a></div>
```

**3. Behaviour.** CSS only.

**4. In CoreUIDemo — not implemented; minimal view.** Identical to [check-email](#check-email) with the three lines above as the card contents and `href="~/Home/Login"`; a plain `return View()` action, no controller file, `Layout = null`. CoreUIDemo's modal flow does not need it: success is `growl.success("Password Successfully Changed")` and the modal closes.

## Show/hide password button

The dist puts the password input in an `input-group` ([04 · Input group](./04-forms.md#input-group)) with an `input-group-text` add-on holding an unstyled button and an inline eye SVG. The button is decorative in the dist — no script in the folder toggles the input type; only the tooltip is wired, and only on `login.html`.

**`authentication/login.html`** — verbatim
```html
<div class="input-group">
  <input class="form-control" id="password" type="password" placeholder="Your password" autocomplete="off">
  <span class="input-group-text">
    <button class="bg-transparent border-0 p-0 link-secondary" type="button" data-coreui-toggle="tooltip" aria-label="Show password" data-coreui-original-title="Show password">
      <svg class="icon" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512">…</svg>
    </button>
  </span>
</div>
```

**AngularJS version.** Bind the input's type to a flag and flip it from the button. `ng-attr-type` is the safe choice: AngularJS 1.8.2 registers the attribute-interpolation directive at priority 100 (`angular.js` line 11244) with a pre-link that writes the attribute, and the `input` directive (line 27566, default priority 0) reads `attr.type` once in its own pre-link (line 27574) — pre-links run in descending priority, so the input directive already sees `password`, not `{{…}}`. Plain `type="{{…}}"` would let the browser parse an invalid type (rendered as text) before AngularJS bootstraps. Both `password` and `text` map to the same text input handler, so `ng-model` keeps working after the flip. The free icon font has no plain eye glyph; `cil-low-vision` is the eye in the set.

**`Views/Home/Register.cshtml`** — the field, and the same on any password input
```html
<div class="input-group">
    <input class="form-control" id="password" ng-attr-type="{{vm.ShowPassword ? 'text' : 'password'}}" placeholder="Your password" ng-model="vm.Password" />
    <span class="input-group-text">
        <button class="bg-transparent border-0 p-0 link-secondary" type="button" ng-click="vm.ShowPassword = !vm.ShowPassword"
                data-coreui-toggle="tooltip" title="Show or hide password" aria-label="Show or hide password"><i class="cil-low-vision"></i></button>
    </span>
</div>
```

**Tooltip.** As on every tooltip, `data-coreui-toggle="tooltip"` does nothing until `coreui.Tooltip` is created for the element ([03 · Tooltips](./03-components.md#tooltips)). The button is static markup on a `Layout = null` page, so the controller can create it directly — no `$timeout`, no `ng-repeat` involved:

**`App/Controller/Register.js`** — inside the controller body
```js
        document.querySelectorAll('[data-coreui-toggle="tooltip"]').forEach(function (el) {
            coreui.Tooltip.getOrCreateInstance(el);
        });
```

Keep the `title` fixed ("Show or hide password"): a `title="{{…}}"` that flips with the flag is read once at init and would never update. A `btn btn-outline-secondary` button add-on (the 04 input-group variant) works too, with a visible border instead of the dist's bare glyph.

## Gotchas

- **No `action`, use `ng-submit`.** The dist's `<form action="…" method="get">` performs a real navigation on submit. In CoreUIDemo the element keeps `autocomplete="off" novalidate` but has no `action` and carries `ng-submit="TryLogin()"`; the button is `type="submit"`. AngularJS's `ngSubmit` prevents the default only on a form with no `action` — add one and Enter submits a GET to the page itself and the handler never runs. This one attribute covers both the click and Enter; the jQuery `$(document).on('keypress', …)` handler OneMasaito's `Login.js` used for Enter is gone.
- **`growl` container inside `<body>`.** `_Layout.cshtml` provides `<div growl class="fading"></div>` for every layout page; these pages have no layout, so each view must include it itself (`Login.cshtml` line 22), and the page module must list `"angular-growl"` as a dependency or `growl` cannot be injected.
- **Register the module and the file.** `ng-app="<name>"` must name a module that is loaded before `DOMContentLoaded`, or AngularJS throws `$injector:modulerr` and the page stays inert. Every controller file is listed by name in `~/bundles/angular` (`App_Start/BundleConfig.cs`), so a new page means a new line there.
- **`Layout = null` is per view.** `Views/_ViewStart.cshtml` points every view at `_Layout.cshtml`; only these pages override it, as [01 · Body and footer](./01-layout.md#body-and-footer) notes. A view that forgets the override renders the login card inside the sidebar shell.
- **Theme.** CoreUIDemo's versions of these pages do not load `color-modes.js` and are pinned to `data-coreui-theme="light"`. Leaving the attribute off without adding the § 9 one-liner gives an unthemed page that still looks light, but `bg-body-tertiary` and `text-body-secondary` then never follow the user's saved dark preference.
- **Removed dist pieces.** "Remember me", "I forgot password", the divider and the social buttons are pure markup — copy them back from the dist page, but their links need actions (`~/Home/ResetPassword`, `~/Home/Register`) that do not exist yet.
