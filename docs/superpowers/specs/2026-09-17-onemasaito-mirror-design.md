# CoreUIDemo — OneMasaito User-Module Mirror: Design Spec

Date: 2026-09-17
Status: approved for planning

## 1. Goal

Convert the existing `CoreUIDemo` repository into a strict, class-for-class mirror of
OneMasaito's login + user-account module (the `HomeController` / `SettingsController` slice),
running on the `loginDemo` schema in `Database/script.sql`, themed with
`coreui-free-bootstrap-admin-template-v5.5.0-dist`. Then document it.

**Scope of this work item is documentation only.** The deliverables in section 2 contain the
complete code and steps; the repository conversion itself is carried out by following
`docs/BUILD_GUIDE.md`, not as part of producing the docs.

Constraints agreed with the user:

1. **Convert the existing repo** — keep git history, the generated EDMX, and already-vendored
   CoreUI assets; delete everything that has no counterpart in OneMasaito.
2. **Exact mirror of OneMasaito's architecture and security posture** — plaintext passwords,
   hand-rolled FormsAuth cookie, no anti-forgery, no `[Authorize]`, `CurrentUser == null`
   redirect only on GET view actions. Weaknesses are *documented*, not fixed.
3. **Use every stored procedure the schema provides**; fall back to OneMasaito's direct-EF
   `SaveChanges()` pattern only where no sproc exists.
4. **Input validation** (user-requested addition, done in OneMasaito's idiom):
   First Name and Last Name letters-only (`^[a-zA-Z ]+$`, space allowed to match OneMasaito's
   existing keypress filter); password minimum length 6.
5. Nothing else is added that OneMasaito does not have.

## 2. Deliverables

All documentation lives in the repo.

| Path | Content |
|---|---|
| `docs/BUILD_GUIDE.md` | Step-by-step conversion + build guide. Full code for every file, verify steps, git checkpoints. |
| `docs/COREUI_GUIDE.md` | CoreUI v5.5.0 implementation guide: vendoring layout, `BundleConfig`, `_Layout` from `index.html`, login card from `authentication/login.html`, JS-global/attribute rules, modal API, icons, dark mode. |
| `docs/ARCHITECTURE.md` | Folder map, request flows, endpoint table, OneMasaito-to-CoreUIDemo mapping (files, DB columns, features kept/dropped), known limitations. |
| `README.md` | What this is, prerequisites, links to the docs. |

## 3. Target folder structure

```
CoreUIDemo/
  App/App.js
  App/Controller/Login.js
  App/Controller/UserAccounts.js
  App_Start/BundleConfig.cs
  App_Start/FilterConfig.cs
  App_Start/Principal.cs
  App_Start/RouteConfig.cs
  Content/Site.css
  Content/build/css/style.css                         (CoreUI dist css/style.css)
  Content/vendor/@coreui/coreui/js/coreui.bundle.min.js
  Content/vendor/@coreui/icons/css/free.min.css
  Content/vendor/@coreui/icons/fonts/*
  Content/vendor/simplebar/css/simplebar.css
  Content/vendor/simplebar/js/simplebar.min.js
  Content/vendor/growl/angular-growl.min.css
  Controllers/HomeController.cs
  Controllers/SettingsController.cs
  Database/script.sql                                  (unchanged)
  Helpers/UniversalHelpers.cs
  Models/LoginDemoEntities.edmx (+ .tt, .Context.tt, generated USERS_ACCOUNTS.cs, vw_Users.cs)
  Models/UserModel.cs                                  (UserModel + ChangePasswordModel)
  Scripts/angular.min.js
  Scripts/angular-growl.min.js
  Scripts/jquery-3.7.1.min.js
  Scripts/js/color-modes.js                            (CoreUI dist js/color-modes.js)
  Services/AccountService.cs
  Services/UserService.cs
  Src/Image/coreui.svg, favicon assets
  Views/Home/Login.cshtml
  Views/Home/Index.cshtml
  Views/Settings/UserAccounts.cshtml
  Views/Shared/_Layout.cshtml
  Views/Shared/Error.cshtml
  Views/_ViewStart.cshtml
  Views/Web.config
  Global.asax / Global.asax.cs
  Web.config
  packages.config
  favicon.ico
```

### 3.1 Files deleted from the current repo (no OneMasaito counterpart)

- `Controllers/AccountController.cs`, `BaseController.cs`, `UsersController.cs`
- `Services/IUserService.cs`, `Services/Userservice.cs`
- `Helpers/ValidateAngularAntiForgeryTokenAttribute.cs`
- `Models/AuthenticatedUserData.cs`, `Models/ViewModels/` (all five)
- `App/Controller/Shared/`, `App/Controller/Users/`
- `Views/Account/`, `Views/Users/`, `Views/Home/About.cshtml`, `Views/Home/Contact.cshtml`,
  `Views/Shared/_LoginLayout.cshtml`
- `Global.asax.cs` -> remove `Application_PostAuthenticateRequest`
- `Web.config` -> remove `<authentication mode="Forms">`
- `FilterConfig.cs` -> remove `filters.Add(new AuthorizeAttribute())`
- Stray CoreUI/Bootstrap copies: `Content/bootstrap*.css*`, `Content/coreui*.css*`,
  `Content/css/`, `Content/vendors/`, `Content/icons/`, `Scripts/bootstrap*.js*`,
  `Scripts/coreui*.js*`, `Scripts/js/*` except `color-modes.js`
- `packages.config` -> remove the `bootstrap` package (OneMasaito lists it but never uses it;
  an unused package is not "structure").

## 4. Backend design

### 4.1 Controllers

**`HomeController`** — identical action set and signatures to OneMasaito:

| Action | Verb | Returns | Behaviour |
|---|---|---|---|
| `Login()` | GET | View | Login page, `Layout = null` |
| `Login(string username, string password)` | POST | `Json(new { errorMessage })` | `UserService.ValidateUserLogin` -> on success `AccountService.LoginToSession(user)` |
| `Logout()` | POST | `Json(serverResponse)` | `AccountService.LogoutFromSession` |
| `ChangePassword(ChangePasswordModel password)` | POST | `Json(new { errorMessage })` | `UserService.ChangePassword` |
| `GetCurrentUser()` | (any) | `Json(new { obj = currentUser })` | `UniversalHelpers.CurrentUser` |
| `Index()` | GET | View or redirect to Login | `CurrentUser == null` guard |

**`SettingsController`**:

| Action | Verb | Returns | Behaviour |
|---|---|---|---|
| `UserAccounts()` | GET | View or redirect | `CurrentUser == null` guard |
| `GetAccounts()` | POST | `Json(new { accountList })` | `UserService.GetAllAccount()` |
| `SaveNewAccount(UserModel account, string role)` | POST | `Json(new { message })` | `ID == 0` -> duplicate check -> `SaveAccount`; else `UpdateAccount`. Messages: "Duplicate Username", "Saved", "Error on Saving", or the validation/sproc message |
| `AdminChangePassword(long account, string password)` | POST | `Json(new { errorMessage })` | `UserService.AdminChangePassword` |
| `UpdateStatus(long account, string password)` | POST | `Json(new { errorMessage })` | `UserService.AdminUpdateStatus` — `password` is the *acting admin's* password |

Dropped: `UpdateUserAccess`, `GetUserReportAccess`, `UpdateReportAccess`, `BuildingPermitDropdown`.

### 4.2 Services (static classes, `using (var db = new loginDemoEntities())`)

**`AccountService`**: `LoginToSession(UserModel)`, `LogoutFromSession(out string)`. Ticket
construction identical to OneMasaito: `PrincipalSerializedModel { Username, Password, SessionID }`
-> `JavaScriptSerializer` -> `FormsAuthenticationTicket(1, username, now, now+30min, true, userData)`
-> `FormsAuthentication.Encrypt` -> `HttpCookie(FormsAuthentication.FormsCookieName)`.
`Session["session_status"] = "online"` kept.

**`UserService`**:

| Method | Data access | Notes |
|---|---|---|
| `ValidateUserLogin(username, password, out msg)` | LINQ `FirstOrDefault(r => r.USERNAME == u && r.PASSWORD == p)` | then `IS_ACTIVE` check -> "Account is Locked. Contact MIS Department"; else "Invalid Username or Password!!" |
| `ChangePassword(ChangePasswordModel, out msg)` | `db.sp_UpdateUserPassword((int)CurrentUser.ID, Current, New, Confirm)` | pre-check `NewPassword.Length >= 6` -> "Password must be at least 6 characters" |
| `GetAllAccount()` | LINQ over `vw_Users`, `orderby FIRST_NAME` | maps to `UserModel` (no password) |
| `CheckUserNameDuplicate(username)` | LINQ on `USERS_ACCOUNTS` with `Replace(" ","")` | as OneMasaito |
| `SaveAccount(UserModel, string role, out msg)` | `db.sp_InsertUserAccount(Username, Password, FirstName, LastName, true, role)` | pre-checks: first/last name regex, password length |
| `UpdateAccount(UserModel, string role, out msg)` | fetch current `PASSWORD` by ID, then `db.sp_UpdateUser((int)ID, Username, FirstName, LastName, currentPassword, role, IsActive)` | pre-check: first/last name regex |
| `AdminChangePassword(id, password, out msg)` | direct EF: set `PASSWORD`, `SaveChanges()` | pre-check: password length |
| `AdminUpdateStatus(id, password, out msg)` | verify `password == acting admin's PASSWORD`; if target `IS_ACTIVE` -> `db.sp_DeleteUser((int)id, (int)CurrentUser.ID)`; else direct EF `IS_ACTIVE = true`, `SaveChanges()` | messages "Wrong Password!", sproc RAISERROR text for self-deactivate |

`SaveAccount`/`UpdateAccount` return `bool` like OneMasaito but gain an `out string message`
so sproc `RAISERROR` text and validation messages reach the UI instead of a bare
"Error on Saving". (OneMasaito swallows these; surfacing the message reuses its existing
`catch (Exception error) { message = error.Message; }` idiom from `ChangePassword`.)

Server-side validation constants (private static in `UserService`):
`NamePattern = "^[a-zA-Z ]+$"`, `PasswordMinLength = 6`. Username has no format rule
(OneMasaito has none); it keeps required + duplicate check only.

### 4.3 Helpers / App_Start / Models

- **`Helpers/UniversalHelpers.CurrentUser`**: decrypt cookie -> deserialize
  `PrincipalSerializedModel` -> `new Principal(ticket.Name)` -> `HttpContext.Current.User = principal`
  -> LINQ `USERS_ACCOUNTS` by `USERNAME` -> `UserModel`. Returns `null` when no cookie.
  `SelectedTransactionModule` / `SelectedDocumentIDForUpload` dropped.
- **`App_Start/Principal.cs`**: `Principal : IPrincipal` (`IsInRole => true`),
  `PrincipalSerializedModel { Username, Password, SessionID }`, internal `IPrincipal`.
  `LotID`, `DocIDForUpload`, `PrincipalSelectedTransactionModule`, `PrincipalDocIDForUpload` dropped.
- **`App_Start/RouteConfig.cs`**: default `Home/Login`.
- **`App_Start/FilterConfig.cs`**: `HandleErrorAttribute` only.
- **`Global.asax.cs`**: `Application_Start` only.
- **`Web.config`**: no `<authentication>` element; connection string `loginDemoEntities`
  kept as-is (already present); EF `defaultConnectionFactory` switched to `SqlConnectionFactory`
  to match OneMasaito.
- **`Models/UserModel.cs`** (one file, two classes, as OneMasaito):

```csharp
public class UserModel
{
    public long ID { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string FullName { get { return FirstName + " " + LastName; } }
    public string Role { get; set; }
    public bool IsActive { get; set; }
}
public class ChangePasswordModel
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
    public string ConfirmPassword { get; set; }   // needed by sp_UpdateUserPassword
}
```

### 4.4 EDMX

Existing `Models/LoginDemoEntities.edmx` (context `loginDemoEntities`) is kept. It must expose:
entities `USERS_ACCOUNTS`, `vw_Users`; function imports `sp_InsertUserAccount`, `sp_UpdateUser`,
`sp_UpdateUserPassword`, `sp_DeleteUser` (all returning `None`). Guide includes a verify step
and a "how to add/refresh a function import" section.

### 4.5 Database mapping

| OneMasaito | loginDemo | Effect |
|---|---|---|
| `STATUS bigint?` (1 = active) | `IS_ACTIVE bit` | `UserModel.IsActive` bool; view shows Active/Inactive |
| `DEPARTMENT` FK + `DEPARTMENT` table | `ROLE varchar(50)` CHECK user/manager/admin | `<select>` with three fixed options |
| `USER_ACCESS` 12 flags | — | dropped; sidebar gates on `Role === 'admin'` |
| `MODULE_PRIVILEDGE`, `USER_REPORT_*` | — | dropped |
| `MODIFIED_BY`, `MODIFIED_DATE` | — | columns absent; not shown |
| `VW_ACCOUNTS` (incl. PASSWORD) | `vw_Users` (excl. PASSWORD) | `GetAllAccount` |
| `SP_INSERT_ACCOUNT` | `sp_InsertUserAccount` / `sp_UpdateUser` | see 4.2 |

Seed (in guide, not in `script.sql`): one admin row
`INSERT INTO USERS_ACCOUNTS (USERNAME, PASSWORD, FIRST_NAME, LAST_NAME, IS_ACTIVE, ROLE) VALUES ('admin','admin123','System','Administrator',1,'admin')`.

## 5. Front-end design

### 5.1 Angular topology (one module per page — OneMasaito's)

- `App/App.js`: `angular.module('app', ['angular-growl', 'login', 'useraccount'])` +
  `mainController` (`Init()` -> POST `/Home/GetCurrentUser`; `ChangePassword(value)`;
  `Logout()`; `OpenPasswordModal()`; global `PopUpMessage`).
- `App/Controller/Login.js`: `angular.module('login', ['angular-growl'])` + `loginController`
  (`TryLogin()`, Enter-key handler).
- `App/Controller/UserAccounts.js`: `angular.module('useraccount', ['app'])` +
  `accountsController` (`Init`, `NewAccount`, `EditAccount`, `Save`, `UpdatePassword`,
  `ChangePassword`, `UpdateStatus`, `SaveStatus`, `#firstName`/`#lastName` keypress filters).
- Client validation added in OneMasaito's sequential-`if` style:
  - `Save()`: username required -> password required (New only) -> password length >= 6
    (New only) -> first name required -> first name `/^[a-zA-Z ]+$/` -> last name required ->
    last name regex -> role required.
  - `ChangePassword()` (admin reset): new required -> confirm required -> length >= 6 -> match.
  - `App.js ChangePassword()`: length >= 6 -> match.
- All bundled in `~/bundles/angular`, loaded on every page.

### 5.2 Theme integration rules (CoreUI v5.5.0)

- JS global is `coreui`, not `bootstrap`. Modals: `coreui.Modal.getOrCreateInstance(el).show()/hide()`
  replaces `$('#id').modal('show'|'hide')`. Data attributes are `data-coreui-toggle`,
  `data-coreui-target`, `data-coreui-dismiss`.
- Icons: CoreUI Icons Free (`<i class="cil-pencil">` etc.) replace Font Awesome.
- `color-modes.js` loaded as a plain `<script>` in `<head>` (must run before first paint).
- Bootstrap-5 class renames where OneMasaito's markup used BS4 (`form-group`->`mb-3`,
  `ml-auto`->`ms-auto`, `close`->`btn-close`, `custom-control`->`form-check`).

### 5.3 Views

- **`Views/Home/Login.cshtml`**: `Layout = null`, `<html ng-app="login">`,
  `body ng-controller="loginController as vm"`, `<div growl>`, CoreUI login card
  (username / password / Login button, `ng-click="TryLogin()"`). Renders `~/Content/css`,
  `~/bundles/scripts`, `~/bundles/angular`.
- **`Views/Shared/_Layout.cshtml`**: CoreUI `index.html` shell — `div.sidebar` (brand,
  nav: Dashboard `/Home/Index`; group "Settings" with "User Account" `/Settings/UserAccounts`,
  `ng-show="main.CurrentUser.Role === 'admin'"`), `div.wrapper` -> `header.header`
  (toggler, search input bound to `main.SearchBox`, user dropdown: Change Password modal,
  Logout modal), `div.body > div.container-fluid > @RenderBody()`, `footer.footer`.
  OneMasaito's loader (`ng-hide="main.ItemLoad"`), `ng-init="Init()"`, `<div growl>`,
  `#logoutModal`, `#PasswordModal` (current / new / confirm).
- **`Views/Home/Index.cshtml`**: simple dashboard card ("Welcome {{main.CurrentUser.FullName}}").
- **`Views/Settings/UserAccounts.cshtml`**: `ng-controller="accountsController as vm"`,
  card + table (`ng-repeat="acc in vm.AccountList | filter: main.SearchBox"`), columns:
  actions / Username / Name / Role / Status; row buttons: edit (`cil-pencil`), reset
  password (`cil-lock-locked`), lock/unlock toggle (`cil-ban` / `cil-check-circle`).
  Modals: `#AccountModal`, `#ChangePasswordModal`, `#UpdateStatusModal`.
  `#UserAccessModal` / `#ReportAccessModal` dropped.

### 5.4 Bundles (`BundleConfig.cs`) — same virtual names as OneMasaito

```
~/bundles/jquery, ~/bundles/jqueryval, ~/bundles/modernizr      (template defaults)
~/Content/css    : Content/build/css/style.css, Content/vendor/@coreui/icons/css/free.min.css,
                   Content/vendor/simplebar/css/simplebar.css, Content/vendor/growl/angular-growl.min.css,
                   Content/Site.css
~/bundles/scripts: Scripts/jquery-3.7.1.min.js, Content/vendor/@coreui/coreui/js/coreui.bundle.min.js,
                   Content/vendor/simplebar/js/simplebar.min.js, Scripts/angular.min.js,
                   Scripts/angular-growl.min.js
~/bundles/angular: App/App.js, App/Controller/Login.js, App/Controller/UserAccounts.js
```

`~/Content/css` as a bundle name collides with a physical `Content/css/` folder; that folder
is deleted in 3.1, so the OneMasaito name is safe to keep. The guide states this explicitly.

## 6. Error handling

- Sproc `RAISERROR` -> `SqlException`/`EntityCommandExecutionException` -> caught in the service
  -> `message = error.Message` -> JSON -> growl. No custom exception types.
- Validation failures short-circuit before any DB call and return the message the same way.
- Unhandled exceptions -> `HandleErrorAttribute` -> `Views/Shared/Error.cshtml`.

## 7. Testing / verification

No automated test project (OneMasaito has none). The guide provides manual verify steps
per section and a final end-to-end checklist:

1. Fresh DB from `script.sql` + seed -> login as admin -> redirected to `/Home/Index`.
2. Sidebar shows Settings only for `admin`.
3. Create user: digits in first/last name rejected client- and server-side; short password
   rejected; duplicate username -> "Duplicate Username"; duplicate first+last -> sproc message.
4. Edit user: rename to existing username -> sproc message; role change persists.
5. Reset password -> login as that user works with new password; short password rejected.
6. Deactivate: wrong admin password -> "Wrong Password!"; self -> sproc message; other ->
   Inactive, that user's login -> "Account is Locked...".
7. Reactivate -> Active.
8. Self change password: mismatch/short rejected; wrong current -> "Current password is incorrect."
9. Logout -> `/Home/Login`; visiting `/Settings/UserAccounts` unauthenticated -> redirect.
10. Dark/light toggle persists across reloads (CoreUI color-modes).

## 8. Known limitations (documented in ARCHITECTURE.md, inherited by design)

Plaintext passwords; password inside the auth ticket; persistent 30-min cookie; no anti-forgery;
JSON actions have no auth check; `IsInRole` always `true`; `CurrentUser` hits the DB on every
read; distinguishable login-failure messages; `GetCurrentUser` answers GET and POST; a
deactivated user's existing cookie keeps working until it expires.
