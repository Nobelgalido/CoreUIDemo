# OneMasaito Mirror Documentation Set — Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Produce four documents in the CoreUIDemo repo — `docs/BUILD_GUIDE.md`, `docs/COREUI_GUIDE.md`, `docs/ARCHITECTURE.md`, `README.md` — that let a developer convert the current repo into a strict OneMasaito user-module mirror on the `loginDemo` schema with the CoreUI v5.5.0 theme.

**Architecture:** The docs describe (they do not perform) a conversion of an ASP.NET MVC 5 / EF6 Database-First / AngularJS 1.8.2 app. All code that the reader must type lives verbatim inside `BUILD_GUIDE.md`; `COREUI_GUIDE.md` covers only the theme; `ARCHITECTURE.md` is the reference after the build. A single "Shared Contract" (below) fixes every name so the four documents never disagree.

**Tech Stack (documented, not installed here):** .NET Framework 4.7.2, ASP.NET MVC 5.2.7, EF 6.5.1 (EDMX), AngularJS 1.8.2, angular-growl-v2 0.7.3, jQuery 3.7.1, CoreUI Free Bootstrap Admin Template v5.5.0 (Bootstrap 5), CoreUI Icons Free, SQL Server.

## Global Constraints

- Spec: `docs/superpowers/specs/2026-09-17-onemasaito-mirror-design.md` — the docs must not add anything the spec's §3 folder structure does not list.
- Exact OneMasaito security posture (spec §1 constraint 2): plaintext passwords, no anti-forgery, no `[Authorize]`, `CurrentUser == null` redirect on GET view actions only.
- Validation (spec §1 constraint 4): First/Last Name regex `^[a-zA-Z ]+$`; password minimum length `6`. Username: required + duplicate check only.
- Stored procedures used: `sp_InsertUserAccount`, `sp_UpdateUser`, `sp_UpdateUserPassword`, `sp_DeleteUser`. Direct EF `SaveChanges()` only in `AdminChangePassword` and the re-activate branch of `AdminUpdateStatus`.
- EF context class name: `loginDemoEntities` (already generated in the repo). Connection-string name: `loginDemoEntities`.
- Every code block in the docs is complete and compilable as written — no `// ...` elisions inside a file listing.
- Docs are Markdown, written for a developer who has never seen OneMasaito. Every OneMasaito deviation is called out inline with the reason.
- Repo paths are relative to `CoreUIDemo/CoreUIDemo/` (the web project, where `CoreUIDemo.csproj` is). The dist template lives at `../coreui-free-bootstrap-admin-template-v5.5.0-dist/`. OneMasaito lives at `../../OneMasaito/OneMasaito/`.

---

## Shared Contract (single source of truth for all four docs)

Every task copies names from here. Do not invent alternatives.

### C1. Endpoints

| Route | Verb | Request | Response |
|---|---|---|---|
| `/Home/Login` | GET | — | View (`Layout = null`) |
| `/Home/Login` | POST | `{ username, password }` | `{ errorMessage }` (`""` = success) |
| `/Home/Logout` | POST | — | `"" ` or error string |
| `/Home/ChangePassword` | POST | `{ password: { CurrentPassword, NewPassword, ConfirmPassword } }` | `{ errorMessage }` |
| `/Home/GetCurrentUser` | POST (also answers GET) | — | `{ obj: UserModel | null }` |
| `/Home/Index` | GET | — | View or redirect `/Home/Login` |
| `/Settings/UserAccounts` | GET | — | View or redirect `/Home/Login` |
| `/Settings/GetAccounts` | POST | — | `{ accountList: UserModel[] }` |
| `/Settings/SaveNewAccount` | POST | `{ account: UserModel, role }` | `{ message }` — `"Saved"` on success |
| `/Settings/AdminChangePassword` | POST | `{ account: long, password }` | `{ errorMessage }` |
| `/Settings/UpdateStatus` | POST | `{ account: long, password }` (acting admin's own password) | `{ errorMessage }` |

### C2. C# signatures

```csharp
// Models/UserModel.cs
namespace CoreUIDemo.Models
public class UserModel { long ID; string Username; string Password; string FirstName; string LastName; string FullName {get;} string Role; bool IsActive; }
public class ChangePasswordModel { string CurrentPassword; string NewPassword; string ConfirmPassword; }

// App_Start/Principal.cs   namespace CoreUIDemo.App_Start
public class Principal : IPrincipal { IIdentity Identity; bool IsInRole(string) => true; Principal(string name); string Username; string Password; string SessionID; }
public class PrincipalSerializedModel { string Username; string Password; string SessionID; }
interface IPrincipal : System.Security.Principal.IPrincipal { string Username; string Password; string SessionID; }

// Helpers/UniversalHelpers.cs   namespace CoreUIDemo.Helpers
public static UserModel CurrentUser { get; }

// Services/AccountService.cs   namespace CoreUIDemo.Services
public static bool LoginToSession(UserModel _userModel);
public static void LogoutFromSession(out string message);

// Services/UserService.cs   namespace CoreUIDemo.Services
private const string NamePattern = "^[a-zA-Z ]+$";
private const int PasswordMinLength = 6;
public static UserModel ValidateUserLogin(string _username, string _password, out string returnString);
public static void ChangePassword(ChangePasswordModel _pass, out string message);
public static List<UserModel> GetAllAccount();
public static bool CheckUserNameDuplicate(string _username);
public static bool SaveAccount(UserModel _account, string _role, out string message);
public static bool UpdateAccount(UserModel _account, string _role, out string message);
public static void AdminChangePassword(long _id, string _password, out string message);
public static void AdminUpdateStatus(long _id, string _password, out string message);

// Controllers/HomeController.cs
public ActionResult Login();
[HttpPost] public JsonResult Login(string username, string password);
[HttpPost] public JsonResult Logout();
[HttpPost] public JsonResult ChangePassword(ChangePasswordModel password);
public JsonResult GetCurrentUser();
public ActionResult Index();

// Controllers/SettingsController.cs
public ActionResult UserAccounts();
[HttpPost] public JsonResult GetAccounts();
[HttpPost] public JsonResult SaveNewAccount(UserModel account, string role);
[HttpPost] public JsonResult AdminChangePassword(long account, string password);
[HttpPost] public JsonResult UpdateStatus(long account, string password);
```

### C3. Server-side message strings (verbatim)

| Situation | String |
|---|---|
| bad credentials | `Invalid Username or Password!!` |
| inactive account at login | `Account is Locked. Contact MIS Department` |
| login exception | `Error | Validate User Login` + `error.Message` |
| name regex fails | `First Name and Last Name may contain letters and spaces only` |
| password too short | `Password must be at least 6 characters` |
| duplicate username (controller) | `Duplicate Username` |
| save ok / save failed | `Saved` / `Error on Saving` |
| status: wrong admin password | `Wrong Password!` |
| status: target not found | `Invalid Password!!` |
| sproc RAISERROR | passed through as `error.Message` |

### C4. Angular

| File | Module | Controller | `$scope` functions |
|---|---|---|---|
| `App/App.js` | `angular.module('app', ['angular-growl', 'login', 'useraccount'])` | `mainController` (`as main`) | `Init`, `ChangePassword(value)`, `Logout`, `OpenPasswordModal`; global `PopUpMessage(data)` |
| `App/Controller/Login.js` | `angular.module('login', ['angular-growl'])` | `loginController` (`as vm`) | `TryLogin` |
| `App/Controller/UserAccounts.js` | `angular.module('useraccount', ['app'])` | `accountsController` (`as vm`) | `Init`, `NewAccount`, `EditAccount(value)`, `Save`, `UpdatePassword(value)`, `ChangePassword`, `UpdateStatus(value)`, `SaveStatus` |

`mainController` state: `main.CurrentUser`, `main.ItemLoad`, `main.SearchBox`, `main.ChangePassword {CurrentPassword, NewPassword, ConfirmPassword}`.
`accountsController` state: `vm.AccountList`, `vm.ModalHeader ('New'|'Edit')`, `vm.Modal`, `vm.Change`, `vm.Status`, `vm.RoleList = ['user','manager','admin']`.

Modal helper used in every controller (replaces `$('#id').modal()`):
```js
function ShowModal(id) { coreui.Modal.getOrCreateInstance(document.getElementById(id)).show(); }
function HideModal(id) { coreui.Modal.getOrCreateInstance(document.getElementById(id)).hide(); }
```

Client-side validation messages (verbatim): `Please input Username`, `Please input Password`, `Password must be at least 6 characters`, `Please input First Name`, `First Name must contain letters only`, `Please input Last Name`, `Last Name must contain letters only`, `Please select Role`, `Please input New Password`, `Please input Confirm Password`, `Password Not Match!`, `Please input Password to proceed`.

### C5. Modal IDs and DOM ids

`#logoutModal`, `#PasswordModal` (in `_Layout`); `#AccountModal`, `#ChangePasswordModal`, `#UpdateStatusModal` (in `UserAccounts.cshtml`); inputs `#firstName`, `#lastName` (keypress filters); sidebar `#sidebar`.

### C6. Bundles and vendored paths

```
~/Content/css     : ~/Content/build/css/style.css
                    ~/Content/vendor/@coreui/icons/css/free.min.css
                    ~/Content/vendor/simplebar/css/simplebar.css
                    ~/Content/vendor/growl/angular-growl.min.css
                    ~/Content/Site.css
~/bundles/scripts : ~/Scripts/jquery-3.7.1.min.js
                    ~/Content/vendor/@coreui/coreui/js/coreui.bundle.min.js
                    ~/Content/vendor/simplebar/js/simplebar.min.js
                    ~/Scripts/angular.min.js
                    ~/Scripts/angular-growl.min.js
~/bundles/angular : ~/App/App.js
                    ~/App/Controller/Login.js
                    ~/App/Controller/UserAccounts.js
plain <script> in <head> (not bundled): ~/Scripts/js/color-modes.js
```

Copy map (dist -> repo):

| From `../coreui-free-bootstrap-admin-template-v5.5.0-dist/` | To |
|---|---|
| `css/style.css` (+ `.map`) | `Content/build/css/` |
| `vendors/@coreui/coreui/js/coreui.bundle.min.js` (+ `.map`) | `Content/vendor/@coreui/coreui/js/` |
| `vendors/@coreui/icons/css/free.min.css` | `Content/vendor/@coreui/icons/css/` |
| `vendors/@coreui/icons/fonts/CoreUI-Icons-Free.*` | `Content/vendor/@coreui/icons/fonts/` |
| `vendors/simplebar/css/simplebar.css`, `vendors/simplebar/js/simplebar.min.js` | `Content/vendor/simplebar/{css,js}/` |
| `js/color-modes.js` | `Scripts/js/` |
| `assets/brand/coreui.svg`, `assets/favicon/favicon-32x32.png` | `Src/Image/` |

angular-growl-v2 0.7.3: copy `Scripts/angular-growl.min.js` and `Content/vendor/growl/angular-growl.min.css` from `../../OneMasaito/OneMasaito/` (or download from https://github.com/JanStevens/angular-growl-2/tree/master/build).

`Scripts/angular.min.js` comes from the `angularjs 1.8.2` NuGet package already in `packages.config`.

### C7. Icons (CoreUI Icons Free, all verified present in `free.min.css`)

`cil-speedometer` Dashboard, `cil-settings` Settings, `cil-people` User Account, `cil-menu` header toggler, `cil-user` header avatar, `cil-lock-locked` Change Password / Reset Password, `cil-account-logout` Logout, `cil-plus` New, `cil-pencil` Edit, `cil-ban` Deactivate, `cil-check-circle` Activate, `cil-search` search.

### C8. Seed SQL

```sql
USE [loginDemo];
INSERT INTO dbo.USERS_ACCOUNTS (USERNAME, [PASSWORD], FIRST_NAME, LAST_NAME, IS_ACTIVE, [ROLE])
VALUES ('admin', 'admin123', 'System', 'Administrator', 1, 'admin');
```

### C9. Git checkpoints in BUILD_GUIDE (commit messages, in order)

1. `chore: commit database schema`
2. `refactor: remove non-OneMasaito files`
3. `feat: EDMX function imports verified`
4. `feat: models, principal, helpers`
5. `feat: services`
6. `feat: controllers`
7. `feat: vendor CoreUI v5.5.0 and bundles`
8. `feat: layout, login and dashboard views`
9. `feat: user accounts view and angular controllers`
10. `docs: end-to-end verification complete`

---

## File Structure

| File | Responsibility |
|---|---|
| `docs/BUILD_GUIDE.md` | The only place with full file listings. §0 prerequisites → §10 end-to-end checklist, Appendix A troubleshooting. |
| `docs/COREUI_GUIDE.md` | Theme only: vendoring, bundle, layout anatomy, login card, JS API rules, icons, dark mode, BS4→BS5 rename table. References BUILD_GUIDE for the full `_Layout.cshtml` rather than duplicating it. |
| `docs/ARCHITECTURE.md` | Reference: folder map, request flows, endpoint table (C1), OneMasaito mapping tables, known limitations. No step-by-step. |
| `README.md` | 30-line entry point. |

---

### Task 1: BUILD_GUIDE.md — §0 to §6 (setup, cleanup, database, EDMX, backend)

**Files:**
- Create: `docs/BUILD_GUIDE.md`

**Interfaces:**
- Consumes: Shared Contract C1–C3, C8, C9 (checkpoints 1–6).
- Produces: the section numbers `§0`–`§6` and the file listings that Task 2 continues, and that COREUI_GUIDE / ARCHITECTURE link to by heading.

- [ ] **Step 1: Write the header and §0 Prerequisites**

Content: title `# CoreUIDemo — Build Guide (OneMasaito User-Module Mirror)`; a 6-line intro stating what is mirrored, what is deliberately identical (security posture) and where the three other docs are; a "How to read this guide" note (✅ VERIFY blocks, 🔍 GIT CHECKPOINT blocks, ⚠️ DEVIATION callouts = where the CoreUIDemo code differs from OneMasaito and why).

§0 lists: Visual Studio with ASP.NET workload + .NET Framework 4.7.2 targeting pack + IIS Express; SQL Server (default instance on `localhost`, or note where to change `data source`); SSMS; Git; the dist folder location; the OneMasaito folder location (only needed for copying angular-growl). ✅ VERIFY: open SSMS and connect; open dist `index.html` in a browser.

- [ ] **Step 2: Write §1 Repository state and cleanup**

Content: a table of what currently exists in the repo and its fate (Keep / Delete / Rewrite) covering every file in spec §3.1 plus the kept files (EDMX, `Global.asax`, `Web.config`, `packages.config`, `favicon.ico`, `Views/Shared/Error.cshtml`, `Views/Web.config`, `Views/_ViewStart.cshtml`, `Database/script.sql`).

A PowerShell block that performs the deletions (run from the web project folder):

```powershell
Remove-Item -Recurse -Force Controllers\AccountController.cs, Controllers\BaseController.cs, Controllers\UsersController.cs
Remove-Item -Recurse -Force Services\IUserService.cs, Services\Userservice.cs
Remove-Item -Recurse -Force Helpers\ValidateAngularAntiForgeryTokenAttribute.cs
Remove-Item -Recurse -Force Models\AuthenticatedUserData.cs, Models\ViewModels
Remove-Item -Recurse -Force App\Controller\Shared, App\Controller\Users
Remove-Item -Recurse -Force Views\Account, Views\Users, Views\Home\About.cshtml, Views\Home\Contact.cshtml, Views\Shared\_LoginLayout.cshtml
Remove-Item -Force Content\bootstrap*.css*, Content\coreui*.css*
Remove-Item -Recurse -Force Content\css, Content\vendors, Content\icons
Remove-Item -Force Scripts\bootstrap*.js*, Scripts\coreui*.js*
Get-ChildItem Scripts\js -Exclude color-modes.js | Remove-Item -Recurse -Force
```

Then the Visual Studio step: in Solution Explorer the deleted files show as missing — select and Delete, or edit `CoreUIDemo.csproj` and remove the `<Compile Include=...>` / `<Content Include=...>` lines for them (state that VS "Show All Files" + "Exclude" is not enough; the csproj entries must go).

Edits shown in full: `Global.asax.cs` (final content — `Application_Start` only), `App_Start/FilterConfig.cs` (final content), `Web.config` `<system.web>` block without `<authentication>` and with `<httpRuntime targetFramework="4.7.2" />`, plus the `<entityFramework>` block using `SqlConnectionFactory`. `packages.config` final content (the current one minus `bootstrap`).

✅ VERIFY: build fails only on missing types referenced by files not yet written (list the expected error names: `IUserService`, `AuthenticatedUserData`, `LoginViewModel`…) — or, if VS still lists the deleted files, re-check csproj. 🔍 GIT CHECKPOINT 1 (`git add Database` first) and 2.

- [ ] **Step 3: Write §2 Database**

Content: run `Database/script.sql` in SSMS (it creates `loginDemo`), then the seed block C8. Explain each object in one line (table, view, 4 sprocs) and the two behaviours the reader must know: `sp_InsertUserAccount` raises on duplicate username **and** duplicate first+last name; `sp_DeleteUser` raises when `@UserID = @ActingUserId`. ✅ VERIFY: `SELECT * FROM dbo.vw_Users` returns the admin row with `UserRole = 'admin'`.

Connection string: show the current `Web.config` `<connectionStrings>` entry and say what to change (`data source`, and `user id`/`password` if not using `sa`); ⚠️ DEVIATION note: none — OneMasaito also keeps its connection string in `Web.config`.

- [ ] **Step 4: Write §3 EDMX verification**

Content: open `Models/LoginDemoEntities.edmx` → Model Browser → check `loginDemoEntities` contains entities `USERS_ACCOUNTS`, `vw_Users` and Function Imports `sp_InsertUserAccount`, `sp_UpdateUser`, `sp_UpdateUserPassword`, `sp_DeleteUser`, each with Returns = None. Steps to add a missing one: right-click model → Update Model from Database → Stored Procedures → tick → Finish; then Model Browser → Function Imports → right-click → Edit → Returns "None". Also "Update Model from Database" refresh if `vw_Users` columns differ.

Show the expected generated signatures (as they appear in `Models/LoginDemoEntities.Context.cs`):

```csharp
public virtual int sp_DeleteUser(Nullable<int> userID, Nullable<int> actingUserId)
public virtual int sp_InsertUserAccount(string uSERNAME, string pASSWORD, string fIRST_NAME, string lAST_NAME, Nullable<bool> iS_ACTIVE, string rOLE)
public virtual int sp_UpdateUser(Nullable<int> userid, string userName, string firstName, string lastName, string password, string role, Nullable<bool> isActive)
public virtual int sp_UpdateUserPassword(Nullable<int> userId, string currentPassword, string newPassword, string confirmPassword)
```

✅ VERIFY: build; the four methods resolve. 🔍 GIT CHECKPOINT 3.

- [ ] **Step 5: Write §4 Models, Principal, Helpers**

Full listings, each preceded by the OneMasaito file it mirrors and a ⚠️ DEVIATION list:

1. `Models/UserModel.cs` — per C2. Deviations: `Department*`, `Modified*`, `UserAccess`, access flags removed (no columns); `Status long?` → `IsActive bool`; `ConfirmPassword` added to `ChangePasswordModel` (sproc needs it).
2. `App_Start/Principal.cs` — per C2. Deviations: `LotID`, `DocIDForUpload`, `PrincipalSelectedTransactionModule`, `PrincipalDocIDForUpload` removed.
3. `Helpers/UniversalHelpers.cs` — `CurrentUser` only: read cookie `FormsAuthentication.FormsCookieName` → `FormsAuthentication.Decrypt` → `JavaScriptSerializer().Deserialize<PrincipalSerializedModel>` → `new Principal(ticket.Name)` with `Username`/`SessionID` → `HttpContext.Current.User = newUser` → `using (var db = new loginDemoEntities())` LINQ on `USERS_ACCOUNTS where USERNAME == newUser.Username` projecting `UserModel { ID, Username, FirstName, LastName, Role, IsActive }` → `FirstOrDefault()`. Returns `null` when no cookie. Deviations: no `USER_ACCESS` join, no `MODULE_PRIVILEDGE` query, `SelectedTransactionModule`/`SelectedDocumentIDForUpload` removed.

✅ VERIFY: build. 🔍 GIT CHECKPOINT 4.

- [ ] **Step 6: Write §5 Services**

Full listings:

1. `Services/AccountService.cs` — `LoginToSession` exactly as OneMasaito (`Session["session_status"]`, `PrincipalSerializedModel`, `JavaScriptSerializer`, `FormsAuthenticationTicket(1, username, DateTime.Now, DateTime.Now.AddMinutes(30), true, userData)`, `FormsAuthentication.Encrypt`, `HttpCookie`, `Response.Cookies.Add`, `try/catch → false`); `LogoutFromSession` (`FormsAuthentication.SignOut()`). Deviations: `SelectDocIDForUpload`, `SelectedTransactionToSession` removed.
2. `Services/UserService.cs` — every method in C2 with the messages in C3. Method bodies:
   - `ValidateUserLogin`: `db.USERS_ACCOUNTS.FirstOrDefault(r => r.USERNAME == _username && r.PASSWORD == _password)`; null → invalid; `!IS_ACTIVE` → locked; else project `UserModel` (with `Password = user.PASSWORD` — needed by `LoginToSession`, as OneMasaito).
   - `ChangePassword`: null/length check on `NewPassword` → message; else `db.sp_UpdateUserPassword((int)UniversalHelpers.CurrentUser.ID, _pass.CurrentPassword, _pass.NewPassword, _pass.ConfirmPassword)` inside try; catch → `message = error.Message`. Note that the sproc's own RAISERROR strings ("Current password is incorrect.", "New password and confirmation do not match.", "New password must be different from the current password.") are what the UI shows.
   - `GetAllAccount`: LINQ over `db.vw_Users orderby FIRST_NAME` → `UserModel { ID, Username, FirstName, LastName, Role = a.UserRole, IsActive = a.IS_ACTIVE }`.
   - `CheckUserNameDuplicate`: as OneMasaito (`Replace(" ", "")`).
   - `SaveAccount`: `Regex.IsMatch` on FirstName/LastName with `NamePattern` → message; `Password.Length < PasswordMinLength` → message; else `db.sp_InsertUserAccount(_account.Username, _account.Password, _account.FirstName, _account.LastName, true, _role)`; return true; catch → `message = error.Message; return false`.
   - `UpdateAccount`: name regex → message; `var currentPassword = db.USERS_ACCOUNTS.Where(r => r.ID == _account.ID).Select(r => r.PASSWORD).FirstOrDefault();` then `db.sp_UpdateUser((int)_account.ID, _account.Username, _account.FirstName, _account.LastName, currentPassword, _role, _account.IsActive)`. ⚠️ DEVIATION: OneMasaito's `VW_ACCOUNTS` returned the password to the browser and posted it back; `vw_Users` does not, so the current password is read server-side.
   - `AdminChangePassword`: length check; `var user = db.USERS_ACCOUNTS.FirstOrDefault(r => r.ID == _id)`; set `PASSWORD`; `db.Entry(user).State = EntityState.Modified; db.SaveChanges();`. Deviation: no `MODIFIED_BY/DATE` (no columns).
   - `AdminUpdateStatus`: `var currentUser = UniversalHelpers.CurrentUser.ID; var admin = db.USERS_ACCOUNTS.FirstOrDefault(r => r.ID == currentUser); var user = db.USERS_ACCOUNTS.FirstOrDefault(r => r.ID == _id);` → user null → `Invalid Password!!`; `admin.PASSWORD != _password` → `Wrong Password!`; `user.IS_ACTIVE` → `db.sp_DeleteUser((int)_id, (int)currentUser)`; else `user.IS_ACTIVE = true; SaveChanges()`. Whole body in try/catch → `message = error.Message` (this is how the sproc's "You cannot deactivate your own account." reaches the UI).
   `using System.Text.RegularExpressions;` and `using System.Data.Entity;` at top.

✅ VERIFY: build. 🔍 GIT CHECKPOINT 5.

- [ ] **Step 7: Write §6 Controllers**

Full listings of `Controllers/HomeController.cs` and `Controllers/SettingsController.cs` per C1/C2. `SaveNewAccount` body:

```csharp
bool save; string message = "";
if (account.ID == 0)
{
    if (UserService.CheckUserNameDuplicate(account.Username))
        return Json(new { message = "Duplicate Username" });
    save = UserService.SaveAccount(account, role, out message);
}
else
    save = UserService.UpdateAccount(account, role, out message);

if (save) return Json(new { message = "Saved" });
return Json(new { message = string.IsNullOrEmpty(message) ? "Error on Saving" : message });
```

`GetCurrentUser` returns `Json(new { obj = UniversalHelpers.CurrentUser }, JsonRequestBehavior.AllowGet)` (⚠️ note: OneMasaito's version has no `AllowGet` and is only ever POSTed to; `AllowGet` is added so the reader can verify it in the browser address bar — remove it if strictness matters more than convenience). `Index()` / `UserAccounts()` use `RedirectToRoute(new { controller = "Home", action = "Login", id = UrlParameter.Optional })` exactly as OneMasaito. Deviations list: dropped actions (`UpdateUserAccess`, `GetUserReportAccess`, `UpdateReportAccess`, `BuildingPermitDropdown`); `department` → `role`; `GetAccounts` returns only `accountList`.

✅ VERIFY: build succeeds with zero errors (views are still missing — that is a runtime, not compile, concern). 🔍 GIT CHECKPOINT 6.

- [ ] **Step 8: Verify Task 1 output**

Run from the web project folder:

```bash
grep -c "^## §" docs/BUILD_GUIDE.md          # expect 7 (§0–§6)
grep -n "Services/UserService.cs\|Services/AccountService.cs\|Helpers/UniversalHelpers.cs\|App_Start/Principal.cs\|Models/UserModel.cs\|Controllers/HomeController.cs\|Controllers/SettingsController.cs" docs/BUILD_GUIDE.md | wc -l   # expect >= 7
grep -c "GIT CHECKPOINT" docs/BUILD_GUIDE.md  # expect 6
grep -n "TODO\|TBD\|\.\.\. *$" docs/BUILD_GUIDE.md   # expect no hits inside code blocks
```

- [ ] **Step 9: Commit**

```bash
git add docs/BUILD_GUIDE.md
git commit -m "docs: build guide part 1 — setup, cleanup, database, EDMX, backend"
```

---

### Task 2: BUILD_GUIDE.md — §7 to §10 + Appendix (CoreUI vendoring, bundles, views, Angular, end-to-end)

**Files:**
- Modify: `docs/BUILD_GUIDE.md` (append)

**Interfaces:**
- Consumes: C1, C4–C7, C9 (checkpoints 7–10); §-numbering from Task 1.
- Produces: the full `_Layout.cshtml`, `Login.cshtml`, `UserAccounts.cshtml`, `App.js`, `Login.js`, `UserAccounts.js`, `BundleConfig.cs` listings that COREUI_GUIDE links to instead of duplicating.

- [ ] **Step 1: Write §7 Vendoring CoreUI and bundles**

Content: the copy map from C6 as a PowerShell block:

```powershell
$dist = "..\coreui-free-bootstrap-admin-template-v5.5.0-dist"
$om   = "..\..\OneMasaito\OneMasaito"
New-Item -ItemType Directory -Force Content\build\css, Content\vendor\@coreui\coreui\js, Content\vendor\@coreui\icons\css, Content\vendor\@coreui\icons\fonts, Content\vendor\simplebar\css, Content\vendor\simplebar\js, Content\vendor\growl, Scripts\js, Src\Image | Out-Null
Copy-Item "$dist\css\style.css*"                                   Content\build\css\
Copy-Item "$dist\vendors\@coreui\coreui\js\coreui.bundle.min.js*"  Content\vendor\@coreui\coreui\js\
Copy-Item "$dist\vendors\@coreui\icons\css\free.min.css*"          Content\vendor\@coreui\icons\css\
Copy-Item "$dist\vendors\@coreui\icons\fonts\CoreUI-Icons-Free.*"  Content\vendor\@coreui\icons\fonts\
Copy-Item "$dist\vendors\simplebar\css\simplebar.css"              Content\vendor\simplebar\css\
Copy-Item "$dist\vendors\simplebar\js\simplebar.min.js"            Content\vendor\simplebar\js\
Copy-Item "$dist\js\color-modes.js*"                               Scripts\js\
Copy-Item "$dist\assets\brand\coreui.svg"                          Src\Image\
Copy-Item "$dist\assets\favicon\favicon-32x32.png"                 Src\Image\
Copy-Item "$om\Scripts\angular-growl.min.js"                       Scripts\
Copy-Item "$om\Content\vendor\growl\angular-growl.min.css"         Content\vendor\growl\
```

Then: "Show All Files" → Include In Project for `Content/build`, `Content/vendor`, `Scripts/js`, `Src`, `Scripts/angular-growl.min.js`. Confirm `Scripts/angular.min.js` exists (NuGet `angularjs 1.8.2`); if not, `Update-Package -reinstall angularjs`. `Content/Site.css` full listing (the loader spinner CSS from OneMasaito's theme, ~15 lines, plus `.icon-text-white-50`).

Full `App_Start/BundleConfig.cs` listing per C6 with `#if DEBUG BundleTable.EnableOptimizations = false`. Include the ⚠️ note on why `~/Content/css` is safe now (folder deleted in §1) and why `color-modes.js` is not bundled. `App_Start/RouteConfig.cs` full listing (default `Home`/`Login`).

✅ VERIFY: build; `Content/build/css/style.css` present in Solution Explorer. 🔍 GIT CHECKPOINT 7.

- [ ] **Step 2: Write §8 Layout, Login and Dashboard views**

Full listings:

1. `Views/_ViewStart.cshtml` (unchanged, shown for completeness).
2. `Views/Shared/_Layout.cshtml` — `<html ng-app="app">`; head: meta, favicon `~/Src/Image/favicon-32x32.png`, `<title>CoreUIDemo</title>`, `@Styles.Render("~/Content/css")`, `<script src="~/Scripts/js/color-modes.js"></script>`, `@Scripts.Render("~/bundles/scripts")`, `@Scripts.Render("~/bundles/angular")` (scripts in head — as OneMasaito). Body: `<div ng-controller="mainController as main" ng-init="Init()">`, loader `ng-hide="main.ItemLoad"`, `<div ng-show="main.ItemLoad">`, `<div growl class="fading"></div>`, then the CoreUI shell: `div.sidebar.sidebar-dark.sidebar-fixed.border-end#sidebar` with `sidebar-header` (brand img `~/Src/Image/coreui.svg` + text "CoreUIDemo"), `ul.sidebar-nav[data-coreui="navigation"]`: `nav-item` Dashboard (`/Home/Index`, `cil-speedometer`), `nav-title` "Modules", `nav-group` Settings (`cil-settings`, `ng-show="main.CurrentUser.Role === 'admin'"`) containing `nav-group-items` → "User Account" (`/Settings/UserAccounts`, `cil-people`); `sidebar-footer` with `sidebar-toggler`. `div.wrapper.d-flex.flex-column.min-vh-100` → `header.header.header-sticky.p-0.mb-4` (container-fluid: `button.header-toggler` calling `coreui.Sidebar.getOrCreateInstance(document.querySelector('#sidebar')).toggle()`, search `input.form-control` `ng-model="main.SearchBox"`, `ul.header-nav.ms-auto` → dropdown with `{{main.CurrentUser.FirstName}} {{main.CurrentUser.LastName}}` + `cil-user` avatar; items: Change Password (`data-coreui-toggle="modal" data-coreui-target="#PasswordModal" ng-click="OpenPasswordModal()"`), divider, Logout (`data-coreui-target="#logoutModal"`)) → `div.body.flex-grow-1 > div.container-fluid > @RenderBody()` → `footer.footer.px-4` ("© CoreUIDemo 2026"). Then `#logoutModal` and `#PasswordModal` (Current / New / Confirm, `ng-model="main.ChangePassword.*"`, button `ng-click="ChangePassword(main.ChangePassword)"`) with BS5 markup (`btn-close`, `data-coreui-dismiss="modal"`, `mb-3`).
3. `Views/Home/Login.cshtml` — `Layout = null`; `<html ng-app="login">`; same head renders; `body.bg-body-tertiary.min-vh-100.d-flex.flex-row.align-items-center` `ng-controller="loginController as vm"`; `<div growl class="fading"></div>`; container `max-width: 32rem` → brand img → `card.p-4` → `h2.h5.text-center` "Login to your account" → username input `ng-model="vm.Username"` (placeholder "Enter Username. . . "), password input `ng-model="vm.Password"`, `button.btn.btn-primary.w-100 ng-click="TryLogin()"` "Login", `<p class="text-center text-body-secondary small">version 1.0.0</p>`. ⚠️ DEVIATION: no "forgot password", "remember me", social buttons, "sign up" — OneMasaito has none.
4. `Views/Home/Index.cshtml` — `ViewBag.Title = "Dashboard"`; one `card` with "Welcome, {{main.CurrentUser.FullName}}" and a short paragraph. (OneMasaito's `Index` is an 1830-line patch-notes page; the shell is what is mirrored.)

✅ VERIFY: run (F5) → `/Home/Login` renders the card with the theme, no console errors; `/Home/Index` without a cookie redirects to Login. 🔍 GIT CHECKPOINT 8.

- [ ] **Step 3: Write §9 User Accounts view and Angular controllers**

Full listings:

1. `App/App.js` — per C4. `Init` posts to `/Home/GetCurrentUser`, sets `main.CurrentUser = data.data.obj; main.ItemLoad = true;`. `ChangePassword(value)`: `if (!value.NewPassword || value.NewPassword.length < 6) growl.error("Password must be at least 6 characters"...)` → `else if (value.ConfirmPassword != value.NewPassword) growl.error("Password Not Match!"...)` → else POST `/Home/ChangePassword` with `{ password: value }`; on `errorMessage == ""` growl success "Password Successfully Changed" + `HideModal("PasswordModal")`, else growl error and clear fields. `Logout` POST `/Home/Logout` → `window.location.href = "/Home/Login"`. `OpenPasswordModal` clears fields. `PopUpMessage` global as OneMasaito. `ShowModal`/`HideModal` defined as globals here (C4). ⚠️ DEVIATION: the `$("#sidebarToggle")` jQuery block removed — CoreUI's sidebar has its own toggler.
2. `App/Controller/Login.js` — verbatim OneMasaito shape: keypress 13 → `TryLogin`; POST `/Home/Login` `{ username: vm.Username, password: vm.Password }`; error → growl; success → `window.location.href = "/Home/Index"`.
3. `App/Controller/UserAccounts.js` — per C4. `Init` POST `/Settings/GetAccounts` → `vm.AccountList = data.data.accountList`. `vm.RoleList = ["user", "manager", "admin"]`. `NewAccount` → `vm.ModalHeader = "New"; vm.Modal = { Role: "user" }; ShowModal("AccountModal")`. `EditAccount(value)` → `vm.ModalHeader = "Edit"; vm.Modal = angular.copy(value); ShowModal(...)` (⚠️ `angular.copy` so a cancelled edit does not mutate the row — OneMasaito assigns the reference; both work, copy avoids a visual glitch). `Save`: sequential checks in the order and with the strings of C4 (`var namePattern = /^[a-zA-Z ]+$/;`), password checks only when `vm.ModalHeader === "New"`; POST `/Settings/SaveNewAccount` `{ account: vm.Modal, role: vm.Modal.Role }`; `PopUpMessage(data.data); $scope.Init(); if (data.data.message == "Saved") HideModal("AccountModal");`. `#firstName`/`#lastName` keypress filters verbatim from OneMasaito. `UpdatePassword(value)` → `vm.Change = angular.copy(value); ShowModal("ChangePasswordModal")`. `ChangePassword`: new required → confirm required → length < 6 → mismatch → POST `/Settings/AdminChangePassword` `{ account: vm.Change.ID, password: vm.Change.NewPassword }`. `UpdateStatus(value)` → `vm.Status = angular.copy(value); ShowModal("UpdateStatusModal")`. `SaveStatus`: `ConfirmPassword` required → POST `/Settings/UpdateStatus` `{ account: vm.Status.ID, password: vm.Status.ConfirmPassword }` → success growl "Account Status Successfully Changed".
4. `Views/Settings/UserAccounts.cshtml` — `ViewBag.Title = "User Accounts"`; `<div ng-controller="accountsController as vm">`; `h1.h3.mb-2` "Accounts"; `card.mb-4 ng-init="Init()"` → `table.table.table-striped` columns: actions (`th` holds `button.btn.btn-success ng-click="NewAccount()"` `cil-plus`), UserName, Name, Role, Status; rows `ng-repeat="acc in vm.AccountList | filter: main.SearchBox"`: buttons Edit (`btn-info`, `cil-pencil`), Reset Password (`btn-warning`, `cil-lock-locked`), Deactivate (`btn-danger`, `cil-ban`, `ng-show="acc.IsActive"`), Activate (`btn-success`, `cil-check-circle`, `ng-show="!acc.IsActive"`); cells `{{acc.Username}}`, `{{acc.FullName}}`, `{{acc.Role}}`, status cell `ng-class="{'text-success': acc.IsActive, 'text-danger': !acc.IsActive}"` `{{ acc.IsActive ? 'Active' : 'Inactive' }}`. Modals: `#AccountModal` (Username `ng-disabled="vm.ModalHeader === 'Edit'"`, Password `ng-show="vm.ModalHeader === 'New'"`, First Name `#firstName`, Last Name `#lastName`, Role `<select ng-model="vm.Modal.Role" ng-options="r for r in vm.RoleList">`, Save button); `#ChangePasswordModal` ("Reset Password - {{vm.Change.FullName}}", New / Confirm); `#UpdateStatusModal` (title switches on `vm.Status.IsActive`, "Confirm Password" = the admin's own password, help text saying so). All BS5/CoreUI attributes.

✅ VERIFY: login as admin → sidebar shows Settings → User Account lists the admin row → New account with digits in First Name is rejected client-side; the same POST sent from DevTools with digits is rejected server-side with the C3 string. 🔍 GIT CHECKPOINT 9.

- [ ] **Step 4: Write §10 End-to-end checklist and Appendix A Troubleshooting**

§10: the 10 numbered checks from spec §7, each with the exact expected message string from C3 or the sproc. 🔍 GIT CHECKPOINT 10.

Appendix A (table: symptom → cause → fix), at minimum: unstyled page (bundle path vs physical folder; csproj not including `Content/build`); `coreui is not defined` (`coreui.bundle.min.js` missing from `~/bundles/scripts` or scripts rendered after the controller runs); `Unknown provider: growlProvider` (`angular-growl.min.js` not in bundle or module dependency missing); `[$injector:modulerr] useraccount` (`UserAccounts.js` missing from `~/bundles/angular` or `app` module not listing `'useraccount'`); login always "Invalid Username or Password!!" (seed not run / wrong DB in connection string); `sp_InsertUserAccount` "does not contain a definition" (function import missing — §3); `The parameterized query expects @ROLE` (function import generated before `@ROLE` existed — refresh); `Cannot deactivate your own account` (expected — sproc guard); modal opens but never closes (`HideModal` called with wrong id); dark mode flashes (color-modes.js not in `<head>`); 403/404 on `/Content/css` (physical `Content/css` folder still exists).

- [ ] **Step 5: Verify Task 2 output**

```bash
grep -c "^## §" docs/BUILD_GUIDE.md        # expect 11 (§0–§10)
grep -c "GIT CHECKPOINT" docs/BUILD_GUIDE.md # expect 10
grep -c "^## Appendix A" docs/BUILD_GUIDE.md # expect 1
grep -n "\$('#\|\$(\"#" docs/BUILD_GUIDE.md | grep -v "firstName\|lastName\|keypress"   # expect no jQuery modal calls
grep -c "data-toggle=\|data-dismiss=\|data-target=" docs/BUILD_GUIDE.md   # expect 0 (must be data-coreui-*)
grep -c "coreui.Modal.getOrCreateInstance" docs/BUILD_GUIDE.md   # expect >= 2
grep -o "cil-[a-z-]*" docs/BUILD_GUIDE.md | sort -u   # every name must be in C7
```

- [ ] **Step 6: Commit**

```bash
git add docs/BUILD_GUIDE.md
git commit -m "docs: build guide part 2 — CoreUI vendoring, views, angular, verification"
```

---

### Task 3: COREUI_GUIDE.md

**Files:**
- Create: `docs/COREUI_GUIDE.md`

**Interfaces:**
- Consumes: C5–C7; BUILD_GUIDE §7–§9 headings (link targets `BUILD_GUIDE.md#-7--vendoring-coreui-and-bundles` etc. — copy the exact heading slugs from the file).
- Produces: nothing consumed later except links from README/ARCHITECTURE.

- [ ] **Step 1: Write sections 1–4 (what CoreUI is, what to take from the dist, where it goes, bundles)**

1. "What the dist contains and what we use" — table of dist folders (`css/`, `js/`, `vendors/`, `assets/`, `authentication/`, `components/`, `forms/`, `icons/`, `error-pages/`, `widgets.html`, `charts.html`) with Use / Ignore and why (`examples.css` is demo-only; `chart.js`/`@coreui/chartjs`/`@coreui/utils`/`main.js`/`charts.js`/`widgets.js`/`config.js` are demo-page scripts; `config.js` is loaded by the dist pages but only sets demo variables — omit).
2. "Why hand-vendored, not NuGet/npm" — mirrors OneMasaito's `Content/build` + `Content/vendor` convention; the `bootstrap` NuGet package that OneMasaito lists is unused there and removed here.
3. Folder layout (C6 tree) with the OneMasaito equivalent alongside (`sb-admin-2.css` ↔ `style.css`, `vendor/fontawesome-free` ↔ `vendor/@coreui/icons`).
4. `BundleConfig` — the three bundles, order rules (`coreui.bundle.min.js` before Angular so `coreui` global exists when controllers run; `angular.min.js` before `angular-growl.min.js`; `color-modes.js` outside bundles in `<head>`), link to BUILD_GUIDE §7 for the file.

- [ ] **Step 2: Write sections 5–7 (layout anatomy, login page, JS API rules)**

5. "Layout anatomy" — annotated skeleton of `index.html`'s shell reduced to what `_Layout.cshtml` keeps (sidebar → wrapper → header → body → footer), a table mapping each dist region to the Razor/Angular construct that replaced its static content (brand SVG → `Src/Image/coreui.svg`; nav items → 3 items; header dropdown → user dropdown with modals; `@RenderBody()`); which dist blocks are dropped (breadcrumb, notification dropdowns, theme dropdown kept? — keep the theme dropdown: show its exact markup from `index.html` with `data-coreui-theme-value="light|dark|auto"` so the reader can paste it). Link to BUILD_GUIDE §8 for the full file.
6. "Login page" — `authentication/login.html` → `Login.cshtml`: what is kept (outer flex wrapper, `max-width: 32rem`, `card p-4`), what is removed (email type, tooltip eye-button, forgot/remember/social/sign-up), and why `Layout = null` + `ng-app="login"`.
7. "JavaScript API rules" — table: BS4/jQuery idiom → CoreUI v5 idiom: `$('#m').modal('show')` → `coreui.Modal.getOrCreateInstance(el).show()`; `data-toggle/target/dismiss` → `data-coreui-*`; `bootstrap.Tooltip` → `coreui.Tooltip`; sidebar toggle → `coreui.Sidebar.getOrCreateInstance(el).toggle()`; dropdowns need no JS. Explain that `coreui.bundle.min.js` includes Popper and *renames* the Bootstrap namespace, so `bootstrap.*` is undefined. Note that jQuery is still loaded (for OneMasaito's keypress filters only) but CoreUI never uses it.

- [ ] **Step 3: Write sections 8–10 (icons, dark mode, BS4→BS5 rename table)**

8. Icons — `free.min.css` + fonts; usage `<i class="cil-pencil"></i>` or `<svg class="nav-icon"><use xlink:href="..."></use></svg>` (state that the guide uses the font classes for simplicity); the C7 list; how to look up others (`icons/coreui-icons-free.html` in the dist).
9. Dark mode — `color-modes.js` reads `localStorage['coreui-free-bootstrap-admin-template-theme']`, sets `data-coreui-theme` on `<html>`; must run before first paint; the header theme dropdown markup; how to force light (`<html data-coreui-theme="light">` and drop the script).
10. BS4 → BS5 class renames encountered when porting OneMasaito markup: `form-group`→`mb-3`, `ml-/mr-`→`ms-/me-`, `pl-/pr-`→`ps-/pe-`, `float-left/right`→`float-start/end`, `text-left/right`→`text-start/end`, `close`→`btn-close`, `custom-control custom-checkbox`→`form-check`, `custom-control-input`→`form-check-input`, `custom-control-label`→`form-check-label`, `form-control` on `<select>`→`form-select`, `input-group-append`→ removed (children direct), `badge-*`→`bg-*`, `font-weight-bold`→`fw-bold`, `sr-only`→`visually-hidden`, `no-gutters`→`g-0`, `dropdown-menu-right`→`dropdown-menu-end`, `text-gray-800`→`text-body`, `bg-white`→`bg-body`.

- [ ] **Step 4: Verify**

```bash
grep -c "^## " docs/COREUI_GUIDE.md      # expect 10
grep -c "BUILD_GUIDE.md#" docs/COREUI_GUIDE.md   # expect >= 3
grep -n "data-toggle\|\$('#.*').modal" docs/COREUI_GUIDE.md   # hits only inside the "before" column of rename/API tables
```

- [ ] **Step 5: Commit**

```bash
git add docs/COREUI_GUIDE.md
git commit -m "docs: CoreUI v5.5.0 implementation guide"
```

---

### Task 4: ARCHITECTURE.md

**Files:**
- Create: `docs/ARCHITECTURE.md`

**Interfaces:**
- Consumes: C1–C6; spec §4.5 and §8.
- Produces: nothing consumed later.

- [ ] **Step 1: Write sections 1–3 (overview, folder map, request flows)**

1. Overview — 5 lines: what the app is, what it mirrors, the three docs.
2. Folder map — spec §3 tree with a one-line responsibility per file and the OneMasaito file it mirrors (third column).
3. Request flows — three sequence lists in text (no diagrams needed):
   - **Login**: browser `Login.js` → POST `/Home/Login` → `UserService.ValidateUserLogin` (LINQ) → `AccountService.LoginToSession` (ticket: `PrincipalSerializedModel` → `JavaScriptSerializer` → `FormsAuthenticationTicket` 30 min persistent → `.ASPXAUTH` cookie) → JSON `{errorMessage:""}` → `window.location = /Home/Index`.
   - **Every authenticated page**: `_Layout` → `mainController.Init` → POST `/Home/GetCurrentUser` → `UniversalHelpers.CurrentUser` (decrypt cookie → `Principal` → `HttpContext.User` → DB lookup by username) → `main.CurrentUser` → sidebar `ng-show`.
   - **Save account**: `UserAccounts.js Save` (client checks) → POST `/Settings/SaveNewAccount` → `CheckUserNameDuplicate` → `SaveAccount`/`UpdateAccount` (server checks → sproc) → `{message}` → `PopUpMessage`.

- [ ] **Step 2: Write sections 4–6 (endpoints, data model, mapping tables)**

4. Endpoint table — C1 verbatim plus an "Auth check" column (`CurrentUser == null` redirect for the two GET views; **none** for every JSON action — stated plainly).
5. Data model — `USERS_ACCOUNTS` columns, `vw_Users`, the four sprocs with their guards and RAISERROR strings, index list; which service method calls which.
6. OneMasaito → CoreUIDemo mapping — three tables: files (spec §3 tree vs OneMasaito path), DB (spec §4.5), features (kept: login, logout, self change-password, list, create, edit, admin reset password, activate/deactivate with admin re-auth, search box filter, loader, growl; dropped: department, 12 module-access flags, module privileges, report access, `BuildingPermitDropdown`, IFCA lists in `GetCurrentUser`, `LotID`/`DocIDForUpload` ticket fields, DataTables, Font Awesome, Chart.js, moment, jquery-easing, angular-file-upload; changed: `Status`→`IsActive`, `Department`→`Role`, jQuery modal API→`coreui.Modal`, sproc set).

- [ ] **Step 3: Write section 7 (Known limitations)**

Numbered list from spec §8, each with: where it lives (file + member), what OneMasaito does, why it is kept (mirror), and the one-line fix a production fork would apply (e.g. "hash with `Rfc2898DeriveBytes`/BCrypt in `ValidateUserLogin` + `SaveAccount` + both password paths; sprocs would need the hash, not the plaintext").

- [ ] **Step 4: Verify**

```bash
grep -c "^## " docs/ARCHITECTURE.md        # expect 7
grep -c "/Settings/UpdateStatus\|/Home/GetCurrentUser" docs/ARCHITECTURE.md   # expect >= 2
grep -c "Known limitations" docs/ARCHITECTURE.md   # expect >= 1
```

- [ ] **Step 5: Commit**

```bash
git add docs/ARCHITECTURE.md
git commit -m "docs: architecture reference"
```

---

### Task 5: README.md and cross-document consistency check

**Files:**
- Create: `README.md` (web project root, next to `CoreUIDemo.csproj`)
- Modify: none

**Interfaces:**
- Consumes: the three docs' top-level headings.

- [ ] **Step 1: Write README.md**

Sections: title + one-paragraph description; "Stack" bullet list; "Prerequisites" (5 bullets, same as BUILD_GUIDE §0); "Quick start" (clone → run `Database/script.sql` + seed → set connection string → build → F5 → `admin`/`admin123`); "Documentation" table linking `docs/BUILD_GUIDE.md`, `docs/COREUI_GUIDE.md`, `docs/ARCHITECTURE.md`, and the spec; "Security notice" — 3 lines: learning replica of OneMasaito's posture, see ARCHITECTURE §7, do not deploy.

- [ ] **Step 2: Cross-document consistency check**

```bash
# every C# method name in C2 appears in BUILD_GUIDE and ARCHITECTURE
for m in ValidateUserLogin ChangePassword GetAllAccount CheckUserNameDuplicate SaveAccount UpdateAccount AdminChangePassword AdminUpdateStatus LoginToSession LogoutFromSession; do
  printf "%-24s BUILD=%s ARCH=%s\n" $m $(grep -c "$m" docs/BUILD_GUIDE.md) $(grep -c "$m" docs/ARCHITECTURE.md); done
# every endpoint in C1 appears in all three
for e in /Home/Login /Home/Logout /Home/ChangePassword /Home/GetCurrentUser /Home/Index /Settings/UserAccounts /Settings/GetAccounts /Settings/SaveNewAccount /Settings/AdminChangePassword /Settings/UpdateStatus; do
  printf "%-32s %s %s\n" $e $(grep -c "$e" docs/BUILD_GUIDE.md) $(grep -c "$e" docs/ARCHITECTURE.md); done
# no leftovers from the old design
grep -n "AccountController\|UsersController\|BaseController\|IUserService\|AuthenticatedUserData\|ValidateAngularAntiForgeryToken\|LoginViewModel" docs/BUILD_GUIDE.md docs/COREUI_GUIDE.md docs/ARCHITECTURE.md README.md | grep -v "Delete\|delete\|remove\|Remove"
# all relative links resolve
grep -o "](docs/[^)]*\|](\.\./[^)]*\|]([A-Z_]*\.md[^)]*" README.md docs/*.md | sort -u
```

Expected: every count ≥ 1; the "leftovers" grep returns only lines in the §1 cleanup table; every link target exists on disk.

- [ ] **Step 3: Commit**

```bash
git add README.md
git commit -m "docs: README and cross-links"
```

---

## Self-Review

- **Spec coverage**: §2 deliverables → Tasks 1–5; §3 structure → Task 1 Step 2 (cleanup) + Task 2 Step 1 (vendoring) + ARCHITECTURE §2; §4.1–4.3 → Task 1 Steps 5–7; §4.4 EDMX → Task 1 Step 4; §4.5 DB mapping → Task 1 Step 3 + ARCHITECTURE §5–6; §5.1 Angular → Task 2 Step 3; §5.2 theme rules → Task 3 Step 2; §5.3 views → Task 2 Steps 2–3; §5.4 bundles → Task 2 Step 1 + Task 3 Step 1; §6 error handling → Task 1 Step 6 (service catch pattern) + ARCHITECTURE §3; §7 verification → Task 2 Step 4; §8 limitations → Task 4 Step 3. No gaps.
- **Placeholders**: none — every doc section lists its concrete content; code that must be typed is either in the Shared Contract or fully specified member-by-member for the doc author.
- **Type consistency**: all names taken from C1–C7; `role` (controller parameter) vs `Role` (model property) vs `RoleList` (Angular) are distinct on purpose and used consistently.
