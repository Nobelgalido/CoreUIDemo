# CoreUIDemo — Architecture Reference

## 1. Overview

CoreUIDemo is an ASP.NET MVC 5 (.NET Framework 4.7.2) application that reproduces **OneMasaito's login and user-account module** as a standalone learning replica: Entity Framework 6 Database-First over the `loginDemo` SQL Server database, AngularJS 1.8.2 with one Angular module per page, a hand-rolled FormsAuthentication cookie, and the CoreUI Free Bootstrap Admin Template v5.5.0 for the UI.

It mirrors OneMasaito's architecture *and* its security posture on purpose (§ 7). It is not a production system.

| Document | Use it for |
|---|---|
| `BUILD_GUIDE.md` | Building / converting the repo, step by step, with every file's code |
| `COREUI_GUIDE.md` | The theme: what is vendored, bundles, layout anatomy, JS API rules, icons, dark mode |
| this file | Understanding the finished system: structure, flows, endpoints, what maps to what in OneMasaito, what is knowingly weak |
| `superpowers/specs/2026-09-17-onemasaito-mirror-design.md` | The approved design decisions |

## 2. Folder map

Every source file, what it is for, and the OneMasaito file it mirrors.

| Path | Responsibility | Mirrors (OneMasaito) |
|---|---|---|
| `App/App.js` | Root Angular module `app` (depends on `angular-growl`, `login`, `useraccount`) + `mainController`: loads the current user, self-service change-password, logout; global helpers `PopUpMessage`, `ShowModal`, `HideModal` | `App/App.js` |
| `App/Controller/Login.js` | Angular module `login` + `loginController`: posts credentials, redirects on success | `App/Controller/Login.js` |
| `App/Controller/UserAccounts.js` | Angular module `useraccount` + `accountsController`: grid load, create/edit, admin password reset, activate/deactivate, client-side validation | `App/Controller/UserAccounts.js` |
| `App_Start/BundleConfig.cs` | Bundles `~/Content/css`, `~/bundles/scripts`, `~/bundles/angular` | `App_Start/BundleConfig.cs` |
| `App_Start/FilterConfig.cs` | `HandleErrorAttribute` only | same |
| `App_Start/Principal.cs` | `Principal` (assigned to `HttpContext.User`), `PrincipalSerializedModel` (ticket payload), `IPrincipal` | `App_Start/Principal.cs` |
| `App_Start/RouteConfig.cs` | Default route `Home/Login` | same |
| `Content/Site.css` | `.loader` spinner, `.icon-text-white-50` | `Content/build/css/sb-admin-2.css` (those two rules) |
| `Content/build/css/style.css` | CoreUI theme (Bootstrap 5 + CoreUI layout) | `Content/build/css/sb-admin-2.css` |
| `Content/vendor/@coreui/coreui/js/coreui.bundle.min.js` | CoreUI/Bootstrap 5 JS + Popper | `Content/vendor/bootstrap/js/bootstrap.bundle.min.js` |
| `Content/vendor/@coreui/icons/` | CoreUI Icons Free font | `Content/vendor/fontawesome-free/` |
| `Content/vendor/simplebar/` | Sidebar scrollbar | — |
| `Content/vendor/growl/angular-growl.min.css` | Growl notification styles | same |
| `Controllers/HomeController.cs` | Login page + JSON login/logout/change-password/current-user, dashboard | `Controllers/HomeController.cs` |
| `Controllers/SettingsController.cs` | User Accounts page + JSON list/save/admin-reset/status | `Controllers/SettingsController.cs` |
| `Database/script.sql` | Schema: table, view, four sprocs | (OneMasaito's DB is not in its repo) |
| `Global.asax.cs` | `Application_Start` only — no auth pipeline hookup | same |
| `Helpers/UniversalHelpers.cs` | `CurrentUser`: cookie → ticket → DB row | `Helpers/UniversalHelpers.cs` |
| `Models/LoginDemoEntities.edmx` (+ generated) | EF6 context `loginDemoEntities`, entities `USERS_ACCOUNTS`, `vw_Users`, four function imports | `Models/OneMasaitoEntities.edmx` (`OneMasaito_LiveEntities`) |
| `Models/UserModel.cs` | `UserModel` DTO + `ChangePasswordModel` | `Models/UserModel.cs` |
| `Scripts/angular.min.js`, `angular-growl.min.js`, `jquery-3.7.0.js` (bundled as `jquery-{version}.js`) | Libraries | same |
| `Scripts/js/color-modes.js` | Theme switcher (loaded in `<head>`, unbundled) | — |
| `Services/AccountService.cs` | Builds / clears the FormsAuthentication cookie | `Services/AccountService.cs` |
| `Services/UserService.cs` | All user queries and writes; server-side validation | `Services/UserService.cs` |
| `Src/Image/` | Logo, favicon | `Src/Image/` |
| `Views/Home/Login.cshtml` | Login page (`Layout = null`, `ng-app="login"`) | `Views/Home/Login.cshtml` |
| `Views/Home/Index.cshtml` | Dashboard placeholder | `Views/Home/Index.cshtml` |
| `Views/Settings/UserAccounts.cshtml` | Accounts grid + 3 modals | `Views/Settings/UserAccounts.cshtml` |
| `Views/Shared/_Layout.cshtml` | CoreUI shell, `mainController`, loader, Logout + Change Password modals | `Views/Shared/_Layout.cshtml` |
| `Views/Shared/Error.cshtml`, `Views/_ViewStart.cshtml`, `Views/Web.config` | MVC template defaults | same |
| `Web.config` | Connection string `loginDemoEntities`; **no** `<authentication>` element | `Web.config` |

No `Areas/`, no DI container, no interfaces, no ViewModels folder, no test project — exactly like OneMasaito.

## 3. Request flows

### 3.1 Login

1. Browser at `/Home/Login` (`Login.cshtml`, Angular module `login`). User clicks **Login** or presses Enter → `loginController.TryLogin()`.
2. `POST /Home/Login` with JSON `{ username, password }`.
3. `HomeController.Login(string username, string password)` → `UserService.ValidateUserLogin`:
   - `db.USERS_ACCOUNTS.FirstOrDefault(r => r.USERNAME == username && r.PASSWORD == password)` — plaintext comparison inside the LINQ predicate.
   - Not found → `"Invalid Username or Password!!"`. Found but `IS_ACTIVE = 0` → `"Account is Locked. Contact MIS Department"`. Otherwise a `UserModel` including the password.
4. On success `AccountService.LoginToSession(user)`:
   - `PrincipalSerializedModel { Username, Password, SessionID }` → `JavaScriptSerializer` → JSON string.
   - `new FormsAuthenticationTicket(1, username, now, now + 30 min, isPersistent: true, json)` → `FormsAuthentication.Encrypt` → `Response.Cookies.Add(new HttpCookie(".ASPXAUTH", encrypted))`.
5. Controller returns `{ errorMessage: "" }` → `Login.js` sets `window.location.href = "/Home/Index"`; non-empty → growl.

Logout is the reverse in one step: the header's Logout item opens `#logoutModal`, its confirm button calls `mainController.Logout()` → `POST /Home/Logout` → `AccountService.LogoutFromSession` → `FormsAuthentication.SignOut()` (expires the cookie) → `""` → the browser navigates to `/Home/Login`.

### 3.2 Every authenticated page

1. `HomeController.Index()` / `SettingsController.UserAccounts()` read `UniversalHelpers.CurrentUser`; `null` → `RedirectToRoute(Home/Login)`; else `View()`.
2. `_Layout.cshtml` renders inside `<div ng-controller="mainController as main" ng-init="Init()">`; the whole shell is hidden behind the `.loader` while `main.ItemLoad` is `false`.
3. `mainController.Init()` → `POST /Home/GetCurrentUser` → `HomeController.GetCurrentUser()` → `UniversalHelpers.CurrentUser`:
   - Reads the `.ASPXAUTH` cookie → `FormsAuthentication.Decrypt` → deserializes `PrincipalSerializedModel` → `new Principal(ticket.Name)` → `HttpContext.Current.User = principal`.
   - Queries `USERS_ACCOUNTS` by `USERNAME` → `UserModel { ID, Username, FirstName, LastName, Role, IsActive }` (no password).
   - **Runs on every access of the property** — cookie decrypt plus a DB round-trip each time.
4. Response `{ obj: UserModel }` → `main.CurrentUser`; `main.ItemLoad = true` reveals the page. Sidebar `Settings` group is `ng-show="main.CurrentUser.Role === 'admin'"`.

### 3.3 Save an account (create or edit)

1. `accountsController.Save()` runs the client-side chain: username required → (create only) password required, ≥ 6 chars → first name required, `/^[a-zA-Z ]+$/` → last name required, regex → role required. Any failure → `growl.error` and stop.
2. `POST /Settings/SaveNewAccount` with `{ account: UserModel, role }`.
3. `SettingsController.SaveNewAccount(UserModel account, string role)`:
   - `account.ID == 0` → `UserService.CheckUserNameDuplicate` → `"Duplicate Username"` or `UserService.SaveAccount` → server-side name regex + password length → `sp_InsertUserAccount` (which raises on duplicate username / duplicate first+last name).
   - `account.ID != 0` → `UserService.UpdateAccount` → name regex → read current `PASSWORD` → `sp_UpdateUser` (which raises on the same duplicates, excluding this row).
4. `{ message: "Saved" }` or `{ message: <validation or RAISERROR text> }` (fallback `"Error on Saving"`).
5. `PopUpMessage(data)` growls; `Init()` reloads the grid; the modal closes only on `"Saved"`.

### 3.4 Errors

- Stored-procedure `RAISERROR` → `SqlException` wrapped by EF in `EntityCommandExecutionException` → caught in the service → `message = error.GetBaseException().Message` → JSON → growl. No custom exception types, no logging.
- Validation failures short-circuit before any DB call and return their message the same way.
- Anything unhandled → `HandleErrorAttribute` → `Views/Shared/Error.cshtml`.

## 4. Endpoints

| Route | Verb | Request body (JSON) | Response | Auth check |
|---|---|---|---|---|
| `/Home/Login` | GET | — | View | none (public page) |
| `/Home/Login` | POST | `{ username, password }` | `{ errorMessage }` — `""` on success | none |
| `/Home/Logout` | POST | — | `""` or an error string | none |
| `/Home/ChangePassword` | POST | `{ password: { CurrentPassword, NewPassword, ConfirmPassword } }` | `{ errorMessage }` | **none** — identifies the user from the cookie; an anonymous call fails with a null-reference message |
| `/Home/GetCurrentUser` | POST (GET also allowed) | — | `{ obj: UserModel \| null }` | none |
| `/Home/Index` | GET | — | View, or 302 → `/Home/Login` | `CurrentUser == null` redirect |
| `/Settings/UserAccounts` | GET | — | View, or 302 → `/Home/Login` | `CurrentUser == null` redirect — any logged-in user, not only admins |
| `/Settings/GetAccounts` | POST | — | `{ accountList: UserModel[] }` (no passwords) | **none** |
| `/Settings/SaveNewAccount` | POST | `{ account: UserModel, role }` | `{ message }` — `"Saved"` on success | **none** |
| `/Settings/AdminChangePassword` | POST | `{ account: long, password }` | `{ errorMessage }` | **none** |
| `/Settings/UpdateStatus` | POST | `{ account: long, password }` — `password` is the **acting admin's own** password | `{ errorMessage }` | **none** beyond the password re-check (which needs a cookie to find "the admin") |

`UserModel` on the wire: `{ ID, Username, Password, FirstName, LastName, FullName, Role, IsActive }` — `Password` is always `null` in responses (`vw_Users` has no password column; `CurrentUser` does not project it) and is only populated in the create request.

## 5. Data model

### 5.1 `dbo.USERS_ACCOUNTS`

| Column | Type | Notes |
|---|---|---|
| `ID` | `bigint identity` PK | Sprocs take it as `INT`; services cast |
| `USERNAME` | `nvarchar(50)` NOT NULL, UNIQUE | |
| `PASSWORD` | `nvarchar(255)` NOT NULL | Plaintext |
| `FIRST_NAME`, `LAST_NAME` | `nvarchar(50)` NULL | Index `IX_USERS_ACCOUNTS_NAME` |
| `IS_ACTIVE` | `bit` NOT NULL default 1 | Index `IX_USERS_ACCOUNTS_ID_ISACTIVE` |
| `ROLE` | `varchar(50)` NOT NULL default `'user'` | `CHK_UserRole`: `user` / `manager` / `admin` |

### 5.2 `dbo.vw_Users`

`ID, USERNAME, ROLE AS UserRole, FIRST_NAME, LAST_NAME, IS_ACTIVE` — unfiltered (active and inactive), no password. Read by `UserService.GetAllAccount`.

### 5.3 Stored procedures

| Procedure | Guards (each a `RAISERROR`, text shown to the user) | Called by |
|---|---|---|
| `sp_InsertUserAccount(@USERNAME, @PASSWORD, @FIRST_NAME, @LAST_NAME, @IS_ACTIVE = 1, @ROLE = 'user')` | `Username already exists.` · `An account for this First Name and Last Name already exists.` | `UserService.SaveAccount` |
| `sp_UpdateUser(@Userid, @UserName, @FirstName, @LastName, @Password, @Role, @isActive)` | `Cannot Update User, ID# does not exist!` · `Username already exists.` · `An account for this First Name and Last Name already exists.` — overwrites every column, so the caller passes the current password back | `UserService.UpdateAccount` |
| `sp_UpdateUserPassword(@UserId, @CurrentPassword, @NewPassword, @ConfirmPassword)` | `New password and confirmation do not match.` · `New password must be different from the current password.` · `Current password is incorrect.` | `UserService.ChangePassword` |
| `sp_DeleteUser(@UserID, @ActingUserId)` | `You cannot deactivate your own account.` — sets `IS_ACTIVE = 0`; there is no re-activate sproc | `UserService.AdminUpdateStatus` (deactivate branch) |

Direct EF `SaveChanges()` (OneMasaito's pattern) is used where no sproc exists: `AdminChangePassword` (set `PASSWORD`) and the re-activate branch of `AdminUpdateStatus` (set `IS_ACTIVE = 1`).

### 5.4 Server-side validation (in `UserService`)

| Rule | Where | Message |
|---|---|---|
| First / Last Name match `^[a-zA-Z ]+$` | `SaveAccount`, `UpdateAccount` | `First Name and Last Name may contain letters and spaces only` |
| Password length ≥ 6 | `SaveAccount`, `AdminChangePassword`, `ChangePassword` | `Password must be at least 6 characters` |
| Username not duplicate | `SettingsController.SaveNewAccount` (C#), then the sprocs | `Duplicate Username` / `Username already exists.` |

The same name/password rules run first in the browser (`UserAccounts.js`, `App.js`) with OneMasaito's sequential-`if` + growl style; the keypress filters on `#firstName` / `#lastName` (OneMasaito's own) block non-letter keys as they are typed.

## 6. OneMasaito → CoreUIDemo mapping

### 6.1 Database

| OneMasaito | CoreUIDemo | Consequence |
|---|---|---|
| `USER_ACCOUNTS.STATUS bigint?` (1 = active) | `USERS_ACCOUNTS.IS_ACTIVE bit` | `UserModel.IsActive bool`; "Locked" login message on `0` |
| `USER_ACCOUNTS.DEPARTMENT` FK + `DEPARTMENT` table | `USERS_ACCOUNTS.ROLE` (`user` / `manager` / `admin`) | Department dropdown → Role `<select>` with three fixed values |
| `USER_ACCOUNTS.MODIFIED_BY`, `MODIFIED_DATE` | — | Columns and grid columns gone |
| `USER_ACCESS` (12 per-module flags, tri-state) | — | Sidebar gating uses `Role === 'admin'` instead; User Access modal gone |
| `MODULE_PRIVILEDGE` | — | gone |
| `USER_REPORT_LIST`, `USER_REPORT_ACCESS` | — | Report Access modal gone |
| `VW_ACCOUNTS` (joins access, **includes PASSWORD**) | `vw_Users` (no password) | Grid never receives passwords; `UpdateAccount` reads the current password server-side |
| `SP_INSERT_ACCOUNT` (insert **and** update) | `sp_InsertUserAccount` + `sp_UpdateUser` | Two sprocs, two service methods |
| `SP_INSERT_USER_ACCESS`, `SP_INSERT_REPORT_ACCESS` | — | gone |
| direct EF for password change | `sp_UpdateUserPassword` | Confirm-match / must-differ / current-correct guards move into SQL |
| direct EF for status toggle | `sp_DeleteUser` (deactivate) + direct EF (activate) | Self-deactivation blocked by the sproc |

### 6.2 Features

**Kept (behaviour identical):** login with growl errors; Enter-to-login; logout via confirm modal; self-service change password with confirm field; loader until the current user is known; header search box filtering the grid; accounts grid with New / Edit / Reset Password / Activate-Deactivate; Username disabled and Password hidden in Edit mode; letters-only keypress filter on names; admin must re-enter **their own** password to toggle a status; `PopUpMessage` "Successfully Saved" / error growl; 30-minute persistent auth cookie carrying username + password; `UniversalHelpers.CurrentUser` DB lookup per access; `IsInRole → true`; manual `CurrentUser == null` redirects on GET views; JSON actions unguarded.

**Dropped (no data or no module):** Department list; User Access modal and `UpdateUserAccess`; Report Access modal, `GetUserReportAccess`, `UpdateReportAccess`; `BuildingPermitDropdown`; `IFCAService` entity/project lists in `GetCurrentUser`; `LotID` / `DocIDForUpload` ticket fields and the two `AccountService` methods that re-issued the cookie for them; `SelectedTransactionModule` / `SelectedDocumentIDForUpload`; Modified By / Modified Date columns; the "Patch Notes" dashboard content; the `#sidebarToggle` jQuery block; DataTables, Font Awesome, Chart.js, moment, jquery-easing, respond, angular-file-upload; the unused `bootstrap` / `bootstrap.less` NuGet packages; Google Fonts.

**Changed:** SB Admin 2 (Bootstrap 4) → CoreUI v5.5.0 (Bootstrap 5) markup and `data-coreui-*` attributes; `$('#x').modal()` → `coreui.Modal` via `ShowModal` / `HideModal`; `fas fa-*` → `cil-*`; `Status` → `IsActive`; `Department` → `Role`; `SaveNewAccount(UserModel, long department)` → `(UserModel, string role)`; `SaveAccount` / `UpdateAccount` gain `out string message` so sproc errors reach the UI; catch blocks use `GetBaseException().Message`; `GetCurrentUser` allows GET; `angular.copy` on edit; modals close only on success; name regex + password length validation (user-requested).

### 6.3 Angular module topology

Identical to OneMasaito, reduced to three files: `app` (layout, `mainController`) lists `login` and `useraccount` as dependencies; `useraccount` lists `app` (a cycle Angular tolerates); `login` is independent because the login page has no layout. All three load on every page through `~/bundles/angular`.

## 7. Known limitations (inherited by design)

Each item is OneMasaito's behaviour, reproduced deliberately. The "production fix" column is what a real fork would do; none of it is applied here.

| # | Limitation | Where | Production fix |
|---|---|---|---|
| 1 | **Passwords are stored and compared in plaintext.** | `USERS_ACCOUNTS.PASSWORD`; `UserService.ValidateUserLogin` (`r.PASSWORD == _password` in the LINQ predicate), `AdminUpdateStatus`, and all four sprocs compare plain strings | Hash with PBKDF2 (`Rfc2898DeriveBytes`) or BCrypt; verify in C# outside the LINQ predicate; the sprocs would receive hashes, and `sp_UpdateUserPassword`'s in-SQL comparison would move to C# |
| 2 | **The plaintext password is inside the auth ticket.** | `PrincipalSerializedModel.Password`, set by `AccountService.LoginToSession` | Put only the user id / username in the ticket |
| 3 | **Persistent cookie.** `isPersistent: true` — survives browser restart until the 30-minute expiry; no sliding expiration, no `HttpOnly` / `Secure` flags set explicitly. | `AccountService.LoginToSession` | `isPersistent: false`, `HttpOnly = true`, `Secure = true`, and `<authentication mode="Forms">` so the framework manages it |
| 4 | **No anti-forgery protection.** No `[ValidateAntiForgeryToken]`, no XSRF header — every POST is CSRF-able. | All controllers, `Login.js`, `App.js`, `UserAccounts.js` | Antiforgery cookie + `X-XSRF-TOKEN` header validated by a filter |
| 5 | **JSON actions have no authentication or authorization check.** `GetAccounts`, `SaveNewAccount`, `AdminChangePassword`, `UpdateStatus` can be called by anyone who knows the URL; `UserAccounts` (the page) only requires *some* logged-in user, not an admin. | `SettingsController` | `[Authorize(Roles = "admin")]` (with a real `IsInRole`) or a global `AuthorizeAttribute` + `[AllowAnonymous]` on Login |
| 6 | **`IsInRole` always returns `true`.** Harmless today because nothing calls it; a landmine if `[Authorize(Roles=…)]` is ever added. | `App_Start/Principal.cs` | Return `role == Role` from the ticket / DB |
| 7 | **`CurrentUser` decrypts the cookie and queries the DB on every access**, and controllers/services read it several times per request. | `Helpers/UniversalHelpers.cs` | Resolve once per request in `Application_PostAuthenticateRequest` and cache in `HttpContext.Items` |
| 8 | **Username enumeration.** `Invalid Username or Password!!` vs `Account is Locked. Contact MIS Department` tells an attacker which usernames exist. | `UserService.ValidateUserLogin` | One generic message |
| 9 | **A deactivated user's existing cookie keeps working** until it expires — `CurrentUser` does not check `IS_ACTIVE`. | `Helpers/UniversalHelpers.cs` | Filter `IS_ACTIVE` in the `CurrentUser` query |
| 10 | **`GetCurrentUser` answers GET as well as POST** (`JsonRequestBehavior.AllowGet`, added for easy verification). | `HomeController.GetCurrentUser` | Remove `AllowGet` |
| 11 | **Exceptions are swallowed** — `AccountService.LoginToSession` returns `false` on any error and the controller ignores it; service errors surface only as growl text, never logged. | `AccountService`, `UserService` | Log; distinguish expected (`RAISERROR`) from unexpected errors |
| 12 | **A tampered cookie throws** — `FormsAuthentication.Decrypt` is not wrapped; the user gets the error page until cookies are cleared. | `Helpers/UniversalHelpers.cs` | `try/catch` → treat as anonymous |
| 13 | **Validation is duplicated by hand** in three places (browser chain, `UserService`, sprocs) with no shared definition. | `UserAccounts.js`, `App.js`, `UserService`, `script.sql` | DataAnnotations on a ViewModel + `ModelState` (the approach the previous design used) |
| 14 | **`Session["session_status"]`** is written and never read. | `AccountService.LoginToSession` | Remove |
