# CoreUIDemo — Build Guide (OneMasaito User-Module Mirror)

This guide converts the **CoreUIDemo** repository into a strict, class-for-class mirror of OneMasaito's login and user-account module — `HomeController` + `SettingsController`, static `UserService` / `AccountService`, `UniversalHelpers.CurrentUser`, the hand-rolled FormsAuthentication cookie, and the one-Angular-module-per-page front end — running on the `loginDemo` schema in `Database/script.sql` and themed with **coreui-free-bootstrap-admin-template-v5.5.0-dist**.

It is deliberately faithful. OneMasaito's security posture (plaintext passwords, password inside the auth ticket, no anti-forgery tokens, no `[Authorize]`, JSON actions with no auth check) is reproduced, not fixed — this is a learning replica. Every weakness is listed in `ARCHITECTURE.md` §7 so nothing is hidden. **Do not deploy this to production.**

Companion documents:

- `docs/COREUI_GUIDE.md` — everything about the CoreUI theme (what to vendor, layout anatomy, JS API rules, icons, dark mode).
- `docs/ARCHITECTURE.md` — the reference after the build (folder map, request flows, endpoints, OneMasaito mapping tables, known limitations).
- `docs/superpowers/specs/2026-09-17-onemasaito-mirror-design.md` — the approved design this guide implements.

**How to read this guide**

- Every section ends with a ✅ **VERIFY** block. Do not continue until it passes.
- 🔍 **GIT CHECKPOINT** blocks give the exact commit to make. Ten checkpoints in total.
- ⚠️ **DEVIATION** callouts mark the places where CoreUIDemo's code differs from OneMasaito's and say why. If a file has no callout, it is a straight port.
- Paths are relative to the web project folder — the one containing `CoreUIDemo.csproj`:
  `C:\...\WORK\CoreUIDemo\CoreUIDemo\`. The CoreUI dist is one level up (`..\coreui-free-bootstrap-admin-template-v5.5.0-dist\`); the OneMasaito project is at `..\..\OneMasaito\OneMasaito\`.
- Every code block that names a file is the **complete** file. Replace the whole file with it.

---

## § 0 — Prerequisites

1. **Visual Studio** (2022 or newer) with the **ASP.NET and web development** workload, and under *Individual components*:
   - **.NET Framework 4.7.2 targeting pack**
   - **IIS Express**
2. **SQL Server** — any edition. The connection string in this repo assumes the **default instance** on `localhost`. If yours is a named instance (e.g. `localhost\SQLEXPRESS`), you will change one value in § 2.
3. **SQL Server Management Studio (SSMS)**.
4. **Git**.
5. The CoreUI dist folder at `..\coreui-free-bootstrap-admin-template-v5.5.0-dist\` (HTML/CSS/JS edition — not the React/Vue/Angular editions). No Node/npm step is needed; the dist is already compiled.
6. The OneMasaito project at `..\..\OneMasaito\OneMasaito\` — needed only in § 7 to copy the `angular-growl` files (or download them; § 7 gives the URL).

✅ **VERIFY**
- SSMS connects to `localhost` (or your instance) with no error.
- Double-click `..\coreui-free-bootstrap-admin-template-v5.5.0-dist\index.html` — the dashboard renders styled in the browser.
- `git --version` prints a version.

---

## § 1 — Repository state and cleanup

The repo currently contains code from an earlier design that added things OneMasaito does not have (`BaseController`, an `IUserService` interface, ViewModels with DataAnnotations, an anti-forgery filter, a `Global.asax` principal hookup, `<authentication mode="Forms">`). All of it goes. What stays is the project skeleton, the generated EDMX, the schema script, and the images/config files the MVC template created.

### 1.1 Fate of every existing file

| Path | Fate | Reason |
|---|---|---|
| `App/App.js` | **Rewrite** (§ 9) | Becomes OneMasaito's root module + `mainController` |
| `App/Controller/Shared/main.controller.js` | **Delete** | OneMasaito has no `Shared/` folder; `mainController` lives in `App.js` |
| `App/Controller/Users/users.list.controller.js` | **Delete** | Replaced by `App/Controller/UserAccounts.js` |
| `App_Start/BundleConfig.cs` | **Rewrite** (§ 7) | OneMasaito bundle names |
| `App_Start/FilterConfig.cs` | **Rewrite** (§ 1.3) | Remove global `AuthorizeAttribute` |
| `App_Start/RouteConfig.cs` | **Rewrite** (§ 7) | Default route `Home/Login` |
| `Content/bootstrap*.css*`, `Scripts/bootstrap*.js*`, `Content/themes/` | **Delete** | Installed by the `bootstrap` NuGet package — unused (CoreUI ships its own Bootstrap 5 build) |
| `Content/coreui*.css*`, `Scripts/coreui*.js*` | **Delete** | Stray copies of the CoreUI *library* dist, not the admin template. § 7 vendors the right files into `Content/build` / `Content/vendor` |
| `Content/css/`, `Content/vendors/`, `Content/icons/`, `Scripts/js/*` (except `color-modes.js`) | **Delete** | Earlier attempt at vendoring the template into the wrong folders; § 7 does it in OneMasaito's layout. `Content/css/` **must** go — it collides with the `~/Content/css` bundle name |
| `Content/Site.css` | **Rewrite** (§ 7) | Loader spinner CSS + growl Bootstrap-4 shim |
| `Controllers/AccountController.cs`, `BaseController.cs`, `UsersController.cs` | **Delete** | No counterpart in OneMasaito |
| `Controllers/HomeController.cs` | **Rewrite** (§ 6) | |
| `Database/script.sql` | **Keep** (untracked — commit it now) | The schema |
| `Global.asax`, `Global.asax.cs` | **Rewrite** `.cs` (§ 1.3) | Remove `Application_PostAuthenticateRequest` |
| `Helpers/ValidateAngularAntiForgeryTokenAttribute.cs` | **Delete** | OneMasaito has no anti-forgery |
| `Models/LoginDemoEntities.*` (edmx, tt, Context, Designer, `USERS_ACCOUNTS.cs`, `vw_Users.cs`) | **Keep** | Generated EF model — verified in § 3 |
| `Models/AuthenticatedUserData.cs`, `Models/ViewModels/*` | **Delete** | Replaced by `Models/UserModel.cs` |
| `Services/IUserService.cs`, `Services/Userservice.cs` | **Delete** | Replaced by static `Services/UserService.cs` + `Services/AccountService.cs` |
| `Views/Account/`, `Views/Users/`, `Views/Home/About.cshtml`, `Views/Home/Contact.cshtml`, `Views/Shared/_LoginLayout.cshtml` | **Delete** | Not OneMasaito's view set |
| `Views/Home/Index.cshtml` | **Rewrite** (§ 8) | Dashboard |
| `Views/Shared/_Layout.cshtml` | **Rewrite** (§ 8) | CoreUI shell |
| `Views/Shared/Error.cshtml`, `Views/Web.config`, `Views/_ViewStart.cshtml` | **Keep** | Template defaults, identical to OneMasaito's |
| `Web.config` | **Edit** (§ 1.3) | Remove `<authentication>`; EF factory |
| `packages.config` | **Edit** (§ 1.2) | Remove `bootstrap` |
| `.gitignore`, `favicon.ico`, `Properties/AssemblyInfo.cs`, `Web.Debug.config`, `Web.Release.config` | **Keep** | |

### 1.2 Remove the `bootstrap` NuGet package

In Visual Studio: **Tools → NuGet Package Manager → Package Manager Console**:

```powershell
Uninstall-Package bootstrap -ProjectName CoreUIDemo
```

This removes `Content/bootstrap*.css*`, `Scripts/bootstrap*.js*` and their `<Content Include>` lines from the csproj, and the `bootstrap` line from `packages.config`. If the console reports the files were modified and skipped, delete them by hand in § 1.4 — they are covered there too.

⚠️ **DEVIATION** — OneMasaito's `packages.config` still lists `bootstrap 5.3.3` and `bootstrap.less 3.4.1`, both unreferenced by any page. An unused package is not part of the structure; it is dropped here.

Final `packages.config`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="angularjs" version="1.8.2" targetFramework="net472" />
  <package id="Antlr" version="3.5.0.2" targetFramework="net472" />
  <package id="EntityFramework" version="6.5.1" targetFramework="net472" />
  <package id="jQuery" version="3.7.1" targetFramework="net472" />
  <package id="jQuery.Validation" version="1.21.0" targetFramework="net472" />
  <package id="Microsoft.AspNet.Mvc" version="5.2.7" targetFramework="net472" />
  <package id="Microsoft.AspNet.Razor" version="3.2.7" targetFramework="net472" />
  <package id="Microsoft.AspNet.Web.Optimization" version="1.1.3" targetFramework="net472" />
  <package id="Microsoft.AspNet.WebPages" version="3.2.7" targetFramework="net472" />
  <package id="Microsoft.CodeDom.Providers.DotNetCompilerPlatform" version="2.0.1" targetFramework="net472" />
  <package id="Microsoft.jQuery.Unobtrusive.Validation" version="3.2.11" targetFramework="net472" />
  <package id="Microsoft.Web.Infrastructure" version="1.0.0.0" targetFramework="net472" />
  <package id="Modernizr" version="2.8.3" targetFramework="net472" />
  <package id="Newtonsoft.Json" version="13.0.3" targetFramework="net472" />
  <package id="WebGrease" version="1.6.0" targetFramework="net472" />
</packages>
```

### 1.3 Config edits

**`Global.asax.cs`** — `Application_Start` only, exactly as OneMasaito. The cookie is decrypted by hand in `UniversalHelpers` (§ 4), never by the pipeline.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace CoreUIDemo
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
```

**`App_Start/FilterConfig.cs`** — `HandleErrorAttribute` only. No global `AuthorizeAttribute`: OneMasaito guards pages with a manual `CurrentUser == null` check inside each GET view action, and guards JSON actions not at all.

```csharp
using System.Web;
using System.Web.Mvc;

namespace CoreUIDemo
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
```

**`Web.config`** — two edits.

1. Delete the whole `<authentication>` element from `<system.web>`. OneMasaito has none: `FormsAuthentication.Encrypt/Decrypt/SignOut` work as static helpers without it, and the cookie name defaults to `.ASPXAUTH`. Without `<forms loginUrl=…>` there is no automatic redirect to a login page — which is why every GET view action does its own redirect (§ 6).
2. Replace the EF `<defaultConnectionFactory>` (LocalDb) with `SqlConnectionFactory`, matching OneMasaito.

The `<system.web>` block becomes:

```xml
  <system.web>
    <compilation debug="true" targetFramework="4.7.2" />
    <httpRuntime targetFramework="4.7.2" />
  </system.web>
```

The `<entityFramework>` block becomes:

```xml
  <entityFramework>
    <defaultConnectionFactory type="System.Data.Entity.Infrastructure.SqlConnectionFactory, EntityFramework" />
    <providers>
      <provider invariantName="System.Data.SqlClient" type="System.Data.Entity.SqlServer.SqlProviderServices, EntityFramework.SqlServer" />
    </providers>
  </entityFramework>
```

Leave `<connectionStrings>`, `<runtime>`, `<system.codedom>` and `<appSettings>` as they are (the connection string is revisited in § 2.3). In the committed file `<system.codedom>` sits *after* `<entityFramework>` rather than before `<connectionStrings>` (Visual Studio moved it on 2026-09-22); element order inside `<configuration>` is irrelevant here, so either position is fine.

### 1.4 Delete the files

From the web project folder, in PowerShell:

```powershell
Remove-Item -Force Controllers\AccountController.cs, Controllers\BaseController.cs, Controllers\UsersController.cs
Remove-Item -Force Services\IUserService.cs, Services\Userservice.cs
Remove-Item -Force Helpers\ValidateAngularAntiForgeryTokenAttribute.cs
Remove-Item -Force Models\AuthenticatedUserData.cs
Remove-Item -Recurse -Force Models\ViewModels
Remove-Item -Recurse -Force App\Controller\Shared, App\Controller\Users
Remove-Item -Recurse -Force Views\Account, Views\Users
Remove-Item -Force Views\Home\About.cshtml, Views\Home\Contact.cshtml, Views\Shared\_LoginLayout.cshtml
Remove-Item -Force -ErrorAction SilentlyContinue Content\bootstrap*.css*, Content\coreui*.css*
Remove-Item -Recurse -Force -ErrorAction SilentlyContinue Content\css, Content\vendors, Content\icons, Content\themes
Remove-Item -Force -ErrorAction SilentlyContinue Scripts\bootstrap*.js*, Scripts\coreui*.js*
Get-ChildItem Scripts\js -Exclude color-modes.js, color-modes.js.map | Remove-Item -Recurse -Force
```

### 1.5 Fix the project file

`CoreUIDemo.csproj` is an old-style project that lists every file explicitly. Deleted files now show in Solution Explorer with a yellow warning triangle. Either:

- **In Visual Studio**: select every warning-triangle item (they are grouped under their folders) → **Delete** (this removes the csproj entry). Then **Build → Rebuild**.
- **Or by hand**: open `CoreUIDemo.csproj` in a text editor and remove every `<Compile Include="…">` and `<Content Include="…">` line whose file no longer exists. The `<Compile>` lines to remove are exactly:
  `Controllers\AccountController.cs`, `Controllers\BaseController.cs`, `Controllers\UsersController.cs`, `Helpers\ValidateAngularAntiForgeryTokenAttribute.cs`, `Models\AuthenticatedUserData.cs`, the five `Models\ViewModels\*.cs`, `Services\IUserService.cs`, `Services\Userservice.cs`.
  The `<Content>` lines to remove are every line starting with `Content\bootstrap`, `Content\coreui`, `Content\css\`, `Content\vendors\`, `Content\icons\`, `Content\themes\`, `Scripts\bootstrap`, `Scripts\coreui`, `Scripts\js\` (except `Scripts\js\color-modes.js`), plus `App\Controller\Shared\main.controller.js`, `App\Controller\Users\users.list.controller.js`, `Views\Account\Login.cshtml`, `Views\Users\Index.cshtml`, `Views\Users\Profile.cshtml`, `Views\Home\About.cshtml`, `Views\Home\Contact.cshtml`, `Views\Shared\_LoginLayout.cshtml`.

One reference is worth knowing about before Visual Studio rewrites it under you: `Microsoft.CodeDom.Providers.DotNetCompilerPlatform` sits in the same `<ItemGroup>` as the other NuGet references and carries its full strong name (`Version=2.0.1.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35`) instead of a bare `Include="Microsoft.CodeDom.Providers.DotNetCompilerPlatform"` in an `<ItemGroup>` of its own. Any package operation can do this. It is harmless as long as the `<HintPath>` still points at `packages\Microsoft.CodeDom.Providers.DotNetCompilerPlatform.2.0.1\lib\net45\` and `Web.config`'s `<system.codedom>` compiler entry names the same version - if they disagree, Appendix A's first row is the symptom.

A file that is on disk but not in the csproj is **not compiled and not deployed**. A file that is in the csproj but not on disk **breaks the build**. Both directions matter for the rest of this guide: every time a section says "add file X", also make sure it is included in the project (Solution Explorer → **Show All Files** → right-click → **Include In Project**), and the ✅ VERIFY build at the end of each section will catch it if you forget.

✅ **VERIFY**
- `Build → Rebuild Solution` reports errors **only** of the kind `The type or namespace name 'X' could not be found` for `X` in: `UserModel`, `ChangePasswordModel`, `UserService`, `AccountService`, `UniversalHelpers` — these come from `Controllers/HomeController.cs`, which still has the old body and is rewritten in § 6. If you see errors mentioning `IUserService`, `AuthenticatedUserData`, `LoginViewModel`, `BaseController` or `ValidateAngularAntiForgeryToken`, a deleted file is still listed in the csproj — go back to § 1.5.
- `git status` shows the deletions and the modified `Web.config`, `packages.config`, `Global.asax.cs`, `FilterConfig.cs`, `CoreUIDemo.csproj`, plus untracked `Database/`.

🔍 **GIT CHECKPOINT 1 — schema**

```bash
git add Database/script.sql
git commit -m "chore: commit database schema"
```

🔍 **GIT CHECKPOINT 2 — cleanup**

```bash
git add -A
git commit -m "refactor: remove non-OneMasaito files"
```

---

## § 2 — Database

### 2.1 Create the database

Open `Database/script.sql` in SSMS and execute it (F5). It creates the `loginDemo` database and, inside it:

| Object | What it does | What you must know |
|---|---|---|
| `dbo.USERS_ACCOUNTS` | The one table: `ID bigint identity`, `USERNAME nvarchar(50) UNIQUE`, `PASSWORD nvarchar(255)`, `FIRST_NAME`, `LAST_NAME`, `IS_ACTIVE bit default 1`, `ROLE varchar(50) default 'user'` | `CHK_UserRole` allows only `'user'`, `'manager'`, `'admin'`. Passwords are stored as typed — the sprocs compare them as plain strings. |
| `dbo.vw_Users` | `ID, USERNAME, ROLE as UserRole, FIRST_NAME, LAST_NAME, IS_ACTIVE` | **No password column.** Includes inactive rows (the UI shows both). |
| `dbo.sp_InsertUserAccount(@USERNAME, @PASSWORD, @FIRST_NAME, @LAST_NAME, @IS_ACTIVE=1, @ROLE='user')` | Insert | `RAISERROR` on duplicate `USERNAME` **and** on duplicate `FIRST_NAME + LAST_NAME`. |
| `dbo.sp_UpdateUser(@Userid, @UserName, @FirstName, @LastName, @Password, @Role, @isActive)` | Full-row update | Overwrites **every** column including `PASSWORD` — the service reads the current password first (§ 5). Same two duplicate guards, excluding the edited row. |
| `dbo.sp_UpdateUserPassword(@UserId, @CurrentPassword, @NewPassword, @ConfirmPassword)` | Self-service change | `RAISERROR` if confirm ≠ new, if new = current, or if current is wrong. |
| `dbo.sp_DeleteUser(@UserID, @ActingUserId)` | Sets `IS_ACTIVE = 0` | `RAISERROR('You cannot deactivate your own account.')` when `@UserID = @ActingUserId`. There is no "reactivate" sproc — § 5 uses a direct EF update for that branch. |

The `RAISERROR` messages are the text the user sees in the growl notification, so the exact strings matter later (§ 10).

### 2.2 Seed the first admin

Nothing can log in until a row exists. Run in SSMS:

```sql
USE [loginDemo];
INSERT INTO dbo.USERS_ACCOUNTS (USERNAME, [PASSWORD], FIRST_NAME, LAST_NAME, IS_ACTIVE, [ROLE])
VALUES ('admin', 'admin123', 'System', 'Administrator', 1, 'admin');
```

### 2.3 Connection string

`Web.config` already contains:

```xml
  <connectionStrings>
    <add name="loginDemoEntities" connectionString="metadata=res://*/Models.LoginDemoEntities.csdl|res://*/Models.LoginDemoEntities.ssdl|res://*/Models.LoginDemoEntities.msl;provider=System.Data.SqlClient;provider connection string=&quot;data source=localhost;initial catalog=loginDemo;persist security info=True;user id=sa;password=YOUR_SA_PASSWORD;trustservercertificate=True;MultipleActiveResultSets=True;App=EntityFramework&quot;" providerName="System.Data.EntityClient" />
  </connectionStrings>
```

Adjust only the parts inside `provider connection string`:

- `data source=localhost` → your instance name if it is not the default instance (e.g. `localhost\SQLEXPRESS`).
- `user id=sa;password=…` → your SQL login, **or** replace both with `integrated security=True` to use Windows authentication.

Keep `name="loginDemoEntities"` — the EF context class (§ 3) is named after it. This mirrors OneMasaito, which keeps its `OneMasaito_LiveEntities` connection string in `Web.config` the same way.

✅ **VERIFY** — in SSMS:

```sql
USE [loginDemo];
SELECT * FROM dbo.vw_Users;
```

One row: `admin`, `UserRole = admin`, `IS_ACTIVE = 1`.

```sql
EXEC dbo.sp_DeleteUser @UserID = 1, @ActingUserId = 1;
```

Fails with `You cannot deactivate your own account.` — proves the guard is in place and the admin is still active.

---

## § 3 — EDMX verification

The repo already contains `Models/LoginDemoEntities.edmx`, generated Database-First from this schema. Confirm it is complete rather than regenerating it.

### 3.1 What must be in the model

Open `Models/LoginDemoEntities.edmx` (the designer opens), then **View → Other Windows → Entity Data Model Browser**. Under `LoginDemoEntities` → `Entity Types` you need `USERS_ACCOUNTS` and `vw_Users`; under `Function Imports` you need all four stored procedures, each with **Returns a Collection Of: None**.

The generated methods in `Models/LoginDemoEntities.Context.cs` must read exactly:

```csharp
public virtual DbSet<USERS_ACCOUNTS> USERS_ACCOUNTS { get; set; }
public virtual DbSet<vw_Users> vw_Users { get; set; }

public virtual int sp_DeleteUser(Nullable<int> userID, Nullable<int> actingUserId)
public virtual int sp_InsertUserAccount(string uSERNAME, string pASSWORD, string fIRST_NAME, string lAST_NAME, Nullable<bool> iS_ACTIVE, string rOLE)
public virtual int sp_UpdateUser(Nullable<int> userid, string userName, string firstName, string lastName, string password, string role, Nullable<bool> isActive)
public virtual int sp_UpdateUserPassword(Nullable<int> userId, string currentPassword, string newPassword, string confirmPassword)
```

(The odd casing `uSERNAME` is how the EF generator camel-cases an all-caps parameter. Leave it.)

Note the parameter types: the sprocs declare `INT` ids while the table's `ID` is `bigint`. The services cast `(int)` at the call site.

### 3.2 If a function import is missing or wrong

1. In the designer, right-click empty space → **Update Model from Database…**
2. **Add** tab → expand **Stored Procedures and Functions** → tick the missing procedure(s) → **Finish**. (If `vw_Users` or `USERS_ACCOUNTS` is missing, tick it under **Views** / **Tables** in the same dialog.)
3. Model Browser → `Function Imports` → right-click the new import → **Edit** → set **Returns a Collection Of** to **None** → OK. (If it was auto-set to *Scalars: Int32*, `ExecuteFunction` would try to read a result set and the sproc's `RAISERROR` would surface as a confusing reader error.)
4. Save the `.edmx` — the `.tt` templates re-run and regenerate `LoginDemoEntities.Context.cs`.

If `sp_InsertUserAccount` exists but lacks the `rOLE` parameter, the import was generated from an older version of the script: delete it from *Function Imports*, delete `sp_InsertUserAccount` under *Model Browser → LoginDemoEntities.Store → Stored Procedures*, then repeat the three steps above.

✅ **VERIFY** — `Build → Rebuild`. The only remaining errors are the five missing types from § 1's VERIFY (`UserModel`, `ChangePasswordModel`, `UserService`, `AccountService`, `UniversalHelpers`). Nothing mentions `sp_*` or `loginDemoEntities`.

🔍 **GIT CHECKPOINT 3**

```bash
git add -A
git commit -m "feat: EDMX function imports verified"
```

(If the model needed no changes, this commit is empty — skip it.)

---

## § 4 — Models, Principal, Helpers

Three files, each a port of one OneMasaito file. Create them in these exact folders and include them in the project.

### 4.1 `Models/UserModel.cs`

Mirrors `OneMasaito/Models/UserModel.cs`: one file holding both `UserModel` and `ChangePasswordModel`. `UserModel` is the DTO for everything user-shaped — the login result, the current user, the account grid rows, the create/edit payload.

⚠️ **DEVIATION (2026-09-24)** — both classes carry DataAnnotations, which OneMasaito has nowhere. They are the **server-side safety net and nothing else**: every attribute deliberately omits `ErrorMessage`, because the user-facing wording belongs to the AngularJS form in the view (§ 8, § 9). An invalid `ModelState` therefore means a client was bypassed, and the action answers with one terse `"Invalid payload"` rather than a framework message. The full contract is in `docs/superpowers/specs/2026-09-24-dataannotations-angular-validation-design.md`.

Three details worth knowing before you copy the block:

- `[StringLength]` skips `null`, so `Password` needs no conditional attribute — on edit the modal hides the field, the property arrives `null`, and the validator passes. The create-only requirement is the `IValidatableObject.Validate` override, which yields a `ValidationResult` with a `null` message, consistent with the rest.
- The lengths mirror `Database/script.sql` (`USERNAME`/`FIRST_NAME`/`LAST_NAME` `nvarchar(50)`, `PASSWORD` `nvarchar(255)`), so oversize input now fails as a validation error instead of a SQL truncation exception surfaced through `GetBaseException().Message`.
- `Role`'s pattern mirrors the `CHK_UserRole` constraint — `user`, `manager`, `admin` — the same three values as `vm.RoleList` in § 9.3.
- `Compare` is `System.ComponentModel.DataAnnotations.CompareAttribute` (.NET 4.5+), **not** the `System.Web.Mvc` one.

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace CoreUIDemo.Models
{
    // The attributes below carry no ErrorMessage on purpose. They are the server-side safety net;
    // every message a user reads is produced by the AngularJS form in the view. See
    // docs/superpowers/specs/2026-09-24-dataannotations-angular-validation-design.md.
    public class UserModel : IValidatableObject
    {
        public long ID { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        // Null on edit - the modal hides the field - and StringLength skips null values.
        // The create-only requirement is in Validate below.
        [StringLength(255, MinimumLength = 6)]
        public string Password { get; set; }

        [Required]
        [StringLength(50)]
        [RegularExpression("^[a-zA-Z ]+$")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [RegularExpression("^[a-zA-Z ]+$")]
        public string LastName { get; set; }

        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

        // Mirrors CHK_UserRole in Database/script.sql.
        [Required]
        [StringLength(50)]
        [RegularExpression("^(user|manager|admin)$")]
        public string Role { get; set; }

        public bool IsActive { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ID == 0 && string.IsNullOrWhiteSpace(Password))
                yield return new ValidationResult(null, new[] { "Password" });
        }
    }

    public class ChangePasswordModel
    {
        [Required]
        public string CurrentPassword { get; set; }

        [Required]
        [StringLength(255, MinimumLength = 6)]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }
}
```

⚠️ **DEVIATION**
- `Department`, `DepartmentDescription`, `ModifiedBy`, `ModifiedDate`, `ShowModifiedDate`, `ShowModifiedBy`, `UserAccess` and the `UserAccessModel` base class (12 module flags) are gone — none of those columns exist in `loginDemo`.
- `long? Status` (1 = active) becomes `bool IsActive` (the column is a `bit`).
- `Role` is new — it replaces `Department` as the thing the Account modal's dropdown edits.
- `ChangePasswordModel.ConfirmPassword` is new — OneMasaito checks the confirmation only in the browser; `sp_UpdateUserPassword` takes it as a parameter and checks it again.

### 4.2 `App_Start/Principal.cs`

Mirrors `OneMasaito/App_Start/Principal.cs`. `PrincipalSerializedModel` is what gets JSON-serialized into the FormsAuthentication ticket's `UserData`; `Principal` is what `UniversalHelpers` assigns to `HttpContext.Current.User`.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using secPrincipal = System.Security.Principal;
using CoreUIDemo.Models;

namespace CoreUIDemo.App_Start
{
    public class Principal : IPrincipal
    {
        public secPrincipal.IIdentity Identity { get; private set; }
        public bool IsInRole(string role)
        {
            return true;
        }

        public Principal(string name)
        {
            Identity = new secPrincipal.GenericIdentity(name);
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public string SessionID { get; set; }
    }

    public class PrincipalSerializedModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string SessionID { get; set; }
    }

    interface IPrincipal : secPrincipal.IPrincipal
    {
        string Username { get; set; }
        string Password { get; set; }
        string SessionID { get; set; }
    }
}
```

`IsInRole` returning `true` unconditionally is OneMasaito's code, kept as-is. Nothing in this app calls it (there is no `[Authorize(Roles=…)]` anywhere), but be aware that adding one later would pass for every logged-in user.

⚠️ **DEVIATION** — `LotID` and `DocIDForUpload` on `PrincipalSerializedModel`, and the `PrincipalSelectedTransactionModule` / `PrincipalDocIDForUpload` classes, are gone: they carried lot/document selections for OneMasaito's Records module, which does not exist here.

### 4.3 `Helpers/UniversalHelpers.cs`

Mirrors `OneMasaito/Helpers/UniversalHelpers.cs`. `CurrentUser` is the **only** way the app finds out who is logged in. It reads the `.ASPXAUTH` cookie, decrypts the ticket, deserializes `PrincipalSerializedModel`, sets `HttpContext.Current.User`, then **queries the database** for the user row — on every single access, exactly like OneMasaito. It returns `null` when there is no cookie.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using CoreUIDemo.Models;
using CoreUIDemo.App_Start;

namespace CoreUIDemo.Helpers
{
    public class UniversalHelpers
    {
        public static UserModel CurrentUser
        {
            get
            {
                UserModel user = null;

                HttpCookie authCookie_coreuidemo = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

                if (authCookie_coreuidemo != null)
                {
                    FormsAuthenticationTicket authTicket_coreuidemo = FormsAuthentication.Decrypt(authCookie_coreuidemo.Value);

                    JavaScriptSerializer serializer = new JavaScriptSerializer();

                    PrincipalSerializedModel serializedModel = serializer.Deserialize<PrincipalSerializedModel>(authTicket_coreuidemo.UserData);

                    Principal newUser = new Principal(authTicket_coreuidemo.Name);

                    newUser.Username = serializedModel.Username;

                    newUser.SessionID = serializedModel.SessionID;

                    HttpContext.Current.User = newUser;

                    using (var db = new loginDemoEntities())
                    {
                        var query = from a in db.USERS_ACCOUNTS
                                    where a.USERNAME == newUser.Username
                                    select new UserModel
                                    {
                                        ID = a.ID,
                                        Username = a.USERNAME,
                                        FirstName = a.FIRST_NAME,
                                        LastName = a.LAST_NAME,
                                        Role = a.ROLE,
                                        IsActive = a.IS_ACTIVE
                                    };

                        user = query.FirstOrDefault();
                    }

                    return user;
                }
                else
                {
                    return user;
                }
            }
        }
    }
}
```

Two behaviours to know, both inherited:

- `FormsAuthentication.Decrypt` throws on a tampered or expired-and-garbled cookie. OneMasaito does not catch it; neither does this. The user sees the `Error.cshtml` page and can clear cookies.
- The password is **not** projected into `CurrentUser` — so `GetCurrentUser` (§ 6) never sends it to the browser. It *is* inside the encrypted ticket (`PrincipalSerializedModel.Password`), because `AccountService.LoginToSession` puts it there — see § 5.1.

⚠️ **DEVIATION** — no `join … USER_ACCESS`, no second `MODULE_PRIVILEDGE` query; `SelectedTransactionModule` and `SelectedDocumentIDForUpload` are gone. The cookie variable is named `authCookie_coreuidemo` instead of `authCookie_onemasaito`; nothing else changes.

✅ **VERIFY** — `Build → Rebuild`. Remaining errors mention only `UserService` and `AccountService`.

🔍 **GIT CHECKPOINT 4**

```bash
git add -A
git commit -m "feat: models, principal, helpers"
```

---

## § 5 — Services

Two static classes. Neither is injected anywhere — controllers call `UserService.Method(...)` directly, as OneMasaito does. Every method opens its own `using (var db = new loginDemoEntities())`.

### 5.1 `Services/AccountService.cs`

Mirrors `OneMasaito/Services/AccountService.cs`. `LoginToSession` builds the FormsAuthentication ticket by hand:

1. `PrincipalSerializedModel { Username, Password, SessionID }` — yes, the **plaintext password** goes into the ticket; that is OneMasaito's design.
2. `JavaScriptSerializer` turns it into the ticket's `UserData` string.
3. `FormsAuthenticationTicket(1, username, now, now + 30 minutes, isPersistent: true, userData)`.
4. `FormsAuthentication.Encrypt` → `HttpCookie(FormsAuthentication.FormsCookieName, …)` → `Response.Cookies.Add`.

`isPersistent: true` means the cookie survives a browser restart (until the 30-minute expiry). `Session["session_status"] = "online"` is set for parity; nothing reads it.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CoreUIDemo.Models;
using CoreUIDemo.App_Start;
using System.Web.Script.Serialization;
using System.Web.Security;
using CoreUIDemo.Helpers;

namespace CoreUIDemo.Services
{
    public class AccountService
    {
        public static bool LoginToSession(UserModel _userModel)
        {
            try
            {
                HttpContext.Current.Session["session_status"] = "online";

                PrincipalSerializedModel serializedModel = new PrincipalSerializedModel();

                serializedModel.Username = _userModel.Username;

                serializedModel.Password = _userModel.Password;

                serializedModel.SessionID = HttpContext.Current.Session.SessionID;

                JavaScriptSerializer serializer = new JavaScriptSerializer();

                string userData = serializer.Serialize(serializedModel);

                FormsAuthenticationTicket authenticationTicket = new FormsAuthenticationTicket
                    (1, _userModel.Username, DateTime.Now, DateTime.Now.AddMinutes(30), true, userData);

                string encryptedTicket = FormsAuthentication.Encrypt(authenticationTicket);

                HttpCookie authenticationCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);

                HttpResponse response = HttpContext.Current.Response;

                response.Cookies.Add(authenticationCookie);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void LogoutFromSession(out string message)
        {
            try
            {
                message = "";

                FormsAuthentication.SignOut();
            }
            catch (Exception error)
            {
                message = error.Message;
            }
        }
    }
}
```

⚠️ **DEVIATION** — `SelectDocIDForUpload` and `SelectedTransactionToSession` (which re-issued the whole cookie to stash a lot/document id) are gone with the Records module. Variable names `authenticationQuickipedia` / `authenticationCookie_quickipedia` become `authenticationTicket` / `authenticationCookie`.

### 5.2 `Services/UserService.cs`

Mirrors `OneMasaito/Services/UserService.cs`, restricted to the user-account methods. Read the table first, then the file.

| Method | Data access | Messages it can return |
|---|---|---|
| `ValidateUserLogin` | LINQ: `USERNAME == u && PASSWORD == p` (plaintext, inside the predicate — OneMasaito's exact pattern) | `Invalid Username or Password!!`, `Account is Locked. Contact MIS Department` |
| `ChangePassword` | `sp_UpdateUserPassword` | `Password must be at least 6 characters`, or the sproc's own text |
| `GetAllAccount` | LINQ over `vw_Users` | — |
| `CheckUserNameDuplicate` | LINQ (spaces stripped on both sides, as OneMasaito) | — |
| `SaveAccount` | `sp_InsertUserAccount` | name/password validation text, or the sproc's own text |
| `UpdateAccount` | read current `PASSWORD`, then `sp_UpdateUser` | name validation text, or the sproc's own text |
| `AdminChangePassword` | direct EF `SaveChanges()` (no sproc exists for an admin reset) | `Password must be at least 6 characters`, `Invalid Password` |
| `AdminUpdateStatus` | re-check the **acting admin's** password; deactivate via `sp_DeleteUser`; re-activate via direct EF | `Wrong Password!`, `Invalid Password!!`, or the sproc's own text |

Validation rules (the only thing added to OneMasaito, at the user's request): First Name and Last Name must match `^[a-zA-Z ]+$` (letters and spaces — the space matches OneMasaito's existing keypress filter on those inputs); passwords must be at least 6 characters. Username has no format rule beyond required + not duplicate, same as OneMasaito.

⚠️ **DEVIATION (2026-09-24)** — those format rules **no longer live in this file**. They are DataAnnotations on the models (§ 4.1), enforced by the model binder before the action body runs, and their messages come from the AngularJS forms (§ 8, § 9). What that removed from `UserService`: the `NamePattern` and `NameMessage` constants, the `Regex.IsMatch` blocks at the top of `SaveAccount` and `UpdateAccount`, `SaveAccount`'s password-length check, `ChangePassword`'s length check, and the `System.Text.RegularExpressions` using.

`PasswordMinLength` and `PasswordMessage` survive for **`AdminChangePassword` only**, because that action takes a plain `string _password` that no attribute can reach (§ 6.2). Its message is unreachable in practice — the reset modal checks the same rule first — so users still only ever read AngularJS's wording. Everything else in this file is a *state* rule (duplicate username, wrong admin password, account not found) and is untouched: those need the database and keep their own messages. Contract: `docs/superpowers/specs/2026-09-24-dataannotations-angular-validation-design.md`.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using CoreUIDemo.Models;
using CoreUIDemo.Helpers;

namespace CoreUIDemo.Services
{
    public class UserService
    {
        // Format rules live on the models as DataAnnotations and on the AngularJS forms; see
        // docs/superpowers/specs/2026-09-24-dataannotations-angular-validation-design.md.
        // These two remain because AdminChangePassword takes a plain string, which no attribute can reach.
        private const int PasswordMinLength = 6;
        private const string PasswordMessage = "Password must be at least 6 characters";

        public static UserModel ValidateUserLogin(string _username, string _password, out string returnString)
        {
            returnString = "";

            UserModel userModel = null;

            try
            {
                using (var db = new loginDemoEntities())
                {
                    var user = db.USERS_ACCOUNTS.FirstOrDefault(r => r.USERNAME == _username && r.PASSWORD == _password);

                    if (user != null)
                    {
                        if (user.IS_ACTIVE)
                        {
                            userModel = new UserModel
                            {
                                ID = user.ID,
                                Username = user.USERNAME,
                                Password = user.PASSWORD,
                                FirstName = user.FIRST_NAME,
                                LastName = user.LAST_NAME,
                                Role = user.ROLE,
                                IsActive = user.IS_ACTIVE
                            };
                        }
                        else
                        {
                            returnString = "Account is Locked. Contact MIS Department";
                        }
                    }
                    else
                    {
                        returnString = "Invalid Username or Password!!";
                    }
                }
            }
            catch (Exception error)
            {
                returnString = "Error | Validate User Login" + error.Message;
            }

            return userModel;
        }//end of validate login

        public static void ChangePassword(ChangePasswordModel _pass, out string message)
        {
            message = "";

            try
            {
                using (var db = new loginDemoEntities())
                {
                    var currentUser = UniversalHelpers.CurrentUser;

                    db.sp_UpdateUserPassword((int)currentUser.ID, _pass.CurrentPassword, _pass.NewPassword, _pass.ConfirmPassword);
                }
            }
            catch (Exception error)
            {
                message = error.GetBaseException().Message;
            }
        }

        public static List<UserModel> GetAllAccount()
        {
            List<UserModel> accountList = null;

            using (var db = new loginDemoEntities())
            {
                var list = from a in db.vw_Users
                           orderby a.FIRST_NAME ascending
                           select new UserModel
                           {
                               ID = a.ID,
                               Username = a.USERNAME,
                               FirstName = a.FIRST_NAME,
                               LastName = a.LAST_NAME,
                               Role = a.UserRole,
                               IsActive = a.IS_ACTIVE
                           };

                accountList = list.ToList();
            }
            return accountList;
        }

        public static bool CheckUserNameDuplicate(string _username)
        {
            using (var db = new loginDemoEntities())
            {
                var checkDuplicate = db.USERS_ACCOUNTS.Where(r => r.USERNAME.Replace(" ", "") == _username.Replace(" ", "")).FirstOrDefault();

                if (checkDuplicate == null)
                    return false;
                else
                    return true;
            }
        }

        public static bool SaveAccount(UserModel _account, string _role, out string message)
        {
            message = "";

            try
            {
                using (var db = new loginDemoEntities())
                {
                    db.sp_InsertUserAccount(_account.Username, _account.Password, _account.FirstName, _account.LastName, true, _role);

                    return true;
                }
            }
            catch (Exception error)
            {
                message = error.GetBaseException().Message;
                return false;
            }
        }

        public static bool UpdateAccount(UserModel _account, string _role, out string message)
        {
            message = "";

            try
            {
                using (var db = new loginDemoEntities())
                {
                    var currentPassword = db.USERS_ACCOUNTS
                                            .Where(r => r.ID == _account.ID)
                                            .Select(r => r.PASSWORD)
                                            .FirstOrDefault();

                    db.sp_UpdateUser((int)_account.ID, _account.Username, _account.FirstName, _account.LastName, currentPassword, _role, _account.IsActive);

                    return true;
                }
            }
            catch (Exception error)
            {
                message = error.GetBaseException().Message;
                return false;
            }
        }

        public static void AdminChangePassword(long _id, string _password, out string message)
        {
            message = "";

            try
            {
                if (_password == null || _password.Length < PasswordMinLength)
                {
                    message = PasswordMessage;
                    return;
                }

                using (var db = new loginDemoEntities())
                {
                    var user = db.USERS_ACCOUNTS.FirstOrDefault(r => r.ID == _id);

                    if (user != null)
                    {
                        user.PASSWORD = _password;

                        db.Entry(user).State = EntityState.Modified;

                        db.SaveChanges();
                    }
                    else
                    {
                        message = "Invalid Password";
                    }
                }
            }
            catch (Exception error)
            {
                message = error.GetBaseException().Message;
            }
        }

        public static void AdminUpdateStatus(long _id, string _password, out string message)
        {
            message = "";

            try
            {
                using (var db = new loginDemoEntities())
                {
                    var currentUser = UniversalHelpers.CurrentUser.ID;

                    var adminID = db.USERS_ACCOUNTS.FirstOrDefault(r => r.ID == currentUser);

                    var user = db.USERS_ACCOUNTS.FirstOrDefault(r => r.ID == _id);

                    if (user != null)
                    {
                        if (adminID.PASSWORD == _password)
                        {
                            if (user.IS_ACTIVE)
                            {
                                db.sp_DeleteUser((int)_id, (int)currentUser);
                            }
                            else
                            {
                                user.IS_ACTIVE = true;

                                db.Entry(user).State = EntityState.Modified;

                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            message = "Wrong Password!";
                        }
                    }
                    else
                    {
                        message = "Invalid Password!!";
                    }
                }
            }
            catch (Exception error)
            {
                message = error.GetBaseException().Message;
            }
        }
    }
}
```

How the pieces behave:

- **`ValidateUserLogin`** projects `Password = user.PASSWORD` on purpose — `AccountService.LoginToSession` needs it for the ticket. This `UserModel` never leaves the server (the controller returns only `errorMessage`).
- **`ChangePassword`** relies on the sproc for the three real checks: wrong current password (`Current password is incorrect.`), confirmation mismatch (`New password and confirmation do not match.`), new = current (`New password must be different from the current password.`). `UniversalHelpers.CurrentUser` is `null` for an unauthenticated caller → `NullReferenceException` → caught → its message is returned. That is the mirror; there is no separate auth check.
- **`UpdateAccount`** must read the current password because `sp_UpdateUser` overwrites every column. ⚠️ **DEVIATION** — OneMasaito's `VW_ACCOUNTS` included `PASSWORD`, so its grid held the password and posted it back for updates. `vw_Users` does not expose it; reading it server-side is the only way to keep the sproc's contract.
- **`AdminUpdateStatus`** — `_password` is the **acting admin's own password**, re-entered as a confirmation before toggling someone's status. Deactivation goes through `sp_DeleteUser`, which also refuses when the admin targets their own row (the sproc's message reaches the UI through the catch). Re-activation has no sproc, so it uses OneMasaito's direct `SaveChanges()` pattern.
- ⚠️ **DEVIATION** — catch blocks use `error.GetBaseException().Message` instead of OneMasaito's `error.Message`. EF6 wraps a stored procedure's `RAISERROR` in an `EntityCommandExecutionException` whose own message is the generic "An error occurred while executing the command definition"; `GetBaseException()` unwraps to the `SqlException` that carries the sproc's text. Same idiom, one extra call, and the growl shows the real reason.
- ⚠️ **DEVIATION** — `SaveAccount` / `UpdateAccount` gain an `out string message`. OneMasaito's versions return only `bool` and swallow the exception, so the UI could only ever say "Error on Saving". With the schema's duplicate-name guards living inside the sprocs, that text has to get out.
- `MODIFIED_BY` / `MODIFIED_DATE` writes are gone (no columns). `GetAccess`, `GetDepartmentList`, `GetUserList`, `GetReportListPerUser`, `SaveAccess`, `UpdateAccess`, `GetUserReportAccess`, `SaveReportAccess` are gone (no tables).

✅ **VERIFY** — `Build → Rebuild`. The only remaining errors are in `Controllers/HomeController.cs` (rewritten next).

🔍 **GIT CHECKPOINT 5**

```bash
git add -A
git commit -m "feat: services"
```

---

## § 6 — Controllers

Two controllers, mirroring `OneMasaito/Controllers/HomeController.cs` and `SettingsController.cs`. Rules inherited from OneMasaito:

- A **GET view action** checks `UniversalHelpers.CurrentUser == null` and redirects to `Home/Login` itself. There is no `[Authorize]` and no framework redirect.
- `Login()` (GET) is the mirror image of that rule: it redirects to `Home/Index` when `CurrentUser` is **not** null, so a signed-in user who opens the login URL gets the dashboard instead of the card. OneMasaito has no such check — see § 6.1.
- A **JSON action** does **no** auth check. Anyone who can reach the URL can call it. (Listed in `ARCHITECTURE.md` §7.)
- JSON actions take plain parameters or a model, and return anonymous objects: `{ errorMessage }` for the auth/password family, `{ message }` for saves.
- No `[ValidateAntiForgeryToken]` anywhere.

### 6.1 `Controllers/HomeController.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoreUIDemo.Models;
using CoreUIDemo.Services;
using CoreUIDemo.Helpers;
using System.Web.ModelBinding;


namespace CoreUIDemo.Controllers
{
    public class HomeController : Controller
    {
        // GET: HOME/Login
        [HttpGet]
        public ActionResult Login()
        {
            // If already logged in, redirect straight to index
            if (UniversalHelpers.CurrentUser != null)
            {
                return RedirectToAction("Index");
            }

            return View();

        }

        // POST: Home/Login
        [HttpPost]
        public JsonResult Login(string username, string password)
        {

            UserModel user = UserService.ValidateUserLogin(username, password, out string serverResponse);

            if (user != null) 
            {
                AccountService.LoginToSession(user);
            }

            return Json(new { errorMessage = serverResponse });
        }


        // POST: HOME/Logout
        [HttpPost]
        public JsonResult Logout()
        {
           

            AccountService.LogoutFromSession(out string serverResponse);

            return Json(serverResponse);
        }



        // POST: Home/ChangePassword
        [HttpPost]
        public JsonResult ChangePassword(ChangePasswordModel password)
        {
            // DataAnnotations on ChangePasswordModel are the safety net only; the AngularJS form
            // says what is wrong, so anything that reaches here bypassed the client.
            if (password == null || !ModelState.IsValid)
            {
                return Json(new { errorMessage = "Invalid payload" });
            }

            string serverResponse = "";

            UserService.ChangePassword(password, out serverResponse);

            return Json(new { errorMessage = serverResponse });
        }

        public JsonResult GetCurrentUser()
        {
            var currentUser = UniversalHelpers.CurrentUser;

            JsonResult result = Json(new { obj = currentUser }, JsonRequestBehavior.AllowGet);

            return result;
        }

        public ActionResult Index()
        {
            var user = UniversalHelpers.CurrentUser;

            if (user == null)
            {
                return RedirectToRoute(new { controller = "Home", action = "Login", id = UrlParameter.Optional });
            }
            else
            {
                return View();
            }
                
        }
    }
}
```

How model binding works for the JSON actions (this is why the Angular payload shapes in § 9 are what they are): AngularJS posts `application/json`; MVC 5's `JsonValueProviderFactory` flattens the body so that `{ username: "a", password: "b" }` binds the two string parameters of `Login`, and `{ password: { CurrentPassword: …, NewPassword: …, ConfirmPassword: … } }` binds the `ChangePasswordModel password` parameter by prefix. `FullName` on `UserModel` is read-only, so the binder ignores it when the browser sends it back.

⚠️ **DEVIATION**
- `GetCurrentUser` returns only `{ obj }`. OneMasaito also stuffs two unrelated dropdown lists from `IFCAService` into this response.
- `JsonRequestBehavior.AllowGet` is added so you can hit `/Home/GetCurrentUser` in the address bar while verifying § 8. OneMasaito's version has no `AllowGet` and is only ever POSTed to by `App.js`; the Angular code here also POSTs. Remove `AllowGet` if you prefer the stricter mirror — nothing else depends on it.
- `Login()` carries an explicit `[HttpGet]` (the GET/POST pair was already unambiguous without it) and returns `RedirectToAction("Index")` when `UniversalHelpers.CurrentUser` is already set. OneMasaito's `Login()` has neither, and always renders the card. Consequence: to see the login form again you have to log out first — the cookie, not the URL, decides (2026-09-22).
- `Login` and `Logout` declare their `out string serverResponse` inline (C# 7 out-variables); OneMasaito writes `string serverResponse = "";` on its own line first. Identical behaviour. `ChangePassword` keeps the separate declaration, because it returns that empty string unchanged when `password` is null.

### 6.2 `Controllers/SettingsController.cs`

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoreUIDemo.Services;
using CoreUIDemo.Helpers;
using CoreUIDemo.Models;

namespace CoreUIDemo.Controllers
{
    public class SettingsController : Controller
    {
        // GET: Settings/UserAccounts
        public ActionResult UserAccounts()
        {
            var user = UniversalHelpers.CurrentUser;

            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            //    //return RedirectToRoute(new { controller = "Home", action = "Login", id = UrlParameter.Optional });
            //else
            return View();
        }

        // POST: Settings/GetAccounts
        [HttpPost]
        public JsonResult GetAccounts()
        {
            var accountList = UserService.GetAllAccount();

            return Json(new {  accountList });
        }


        // POST: Settings/SaveNewAccount
        [HttpPost]
        public JsonResult SaveNewAccount(UserModel account, string role)
        {
            if (account == null || !ModelState.IsValid)
            {
                return Json(new { message = "Invalid payload" });
            }

            bool save;

            string message = "";

            if (account.ID == 0)
            {
                if (UserService.CheckUserNameDuplicate(account.Username))
                {
                    return Json(new { message = "Duplicate Username" });
                }
                else
                {
                    save = UserService.SaveAccount(account, role, out message);
                }
            }
            else
                save = UserService.UpdateAccount(account, role, out message);

            if (save)
            {
                return Json(new { message = "Saved" });
            }
            return Json(new { message = string.IsNullOrEmpty(message) ? "Error on Saving" : message });
        }


        // POST: Settings/AdminChangePassword
        [HttpPost]
        public JsonResult AdminChangePassword(long account, string password)
        {
            var serverResponse = "";

            if (password != null)
            {
                UserService.AdminChangePassword(account, password, out serverResponse);
            }
            return Json(new { errorMessage = serverResponse });
        }


        // POST:  Settings/UpdateStatus
        [HttpPost]
        public JsonResult UpdateStatus(long account, string password)
        {
            var serverResponse = "";

            if (password != null)
            {
                UserService.AdminUpdateStatus(account, password, out serverResponse);
            }
            return Json(new { errorMessage = serverResponse });
        }
    }
}
```

`SaveNewAccount` keeps OneMasaito's branching: `ID == 0` means create (duplicate-username check in C#, then the sproc, which checks again), otherwise update. A null `account` — an empty or malformed body — short-circuits to `{ message: "Invalid payload" }` before any service call; without that guard the model binder's `null` would reach `account.ID` and throw a `NullReferenceException` into the JSON response (added 2026-09-22; OneMasaito has no such guard). The `role` parameter arrives beside the model — `{ account: {...}, role: "admin" }` — exactly where OneMasaito's `long department` used to be.

`UpdateStatus`'s `password` is the acting admin's password, not the target user's — see § 5.2.

⚠️ **DEVIATION**
- Dropped actions: `UpdateUserAccess`, `GetUserReportAccess`, `UpdateReportAccess` (no access/report tables) and `BuildingPermitDropdown` (an AppSuite endpoint that sat in OneMasaito's Settings controller by accident).
- `GetAccounts` returns only `accountList` (OneMasaito adds `departmentList` and `reportList`).
- `long department` → `string role`.
- When a save fails, the sproc/validation message is returned instead of the fixed "Error on Saving" (which is still the fallback when there is no message).
- `UserAccounts()` redirects with `RedirectToAction("Login", "Home")`; `HomeController.Index()` still uses `RedirectToRoute(new { controller = "Home", action = "Login", … })`. Both emit the same 302 to `/Home/Login`. The old `RedirectToRoute` line is left commented out above it (2026-09-22).
- `GetAccounts` returns `Json(new { accountList })` — the C# 7 inferred member name, byte-identical on the wire to `new { accountList = accountList }`.

✅ **VERIFY** — `Build → Rebuild Solution` reports **0 errors**. (Views are still the old ones or missing; that is a runtime concern handled in § 8–§ 9. Do not run the app yet.)

🔍 **GIT CHECKPOINT 6**

```bash
git add -A
git commit -m "feat: controllers"
```

---

## § 7 — Vendoring CoreUI and bundles

OneMasaito keeps its compiled theme in `Content/build/` and every third-party library in `Content/vendor/<name>/`, and registers three bundles: `~/Content/css`, `~/bundles/scripts`, `~/bundles/angular`. This section does the same with CoreUI. `docs/COREUI_GUIDE.md` explains *why* each file is chosen; this section just puts them in place.

### 7.1 Copy the files

From the web project folder, in PowerShell:

```powershell
$dist = "..\coreui-free-bootstrap-admin-template-v5.5.0-dist"
$om   = "..\..\OneMasaito\OneMasaito"
$logo = "..\MasaitoLogo"

New-Item -ItemType Directory -Force Content\build\css, Content\vendor\@coreui\coreui\js, Content\vendor\@coreui\icons\css, Content\vendor\@coreui\icons\fonts, Content\vendor\simplebar\css, Content\vendor\simplebar\js, Content\vendor\growl, Scripts\js, Src\Image | Out-Null

Copy-Item "$dist\css\style.css*"                                   Content\build\css\
Copy-Item "$dist\vendors\@coreui\coreui\js\coreui.bundle.min.js*"  Content\vendor\@coreui\coreui\js\
Copy-Item "$dist\vendors\@coreui\icons\css\free.min.css*"          Content\vendor\@coreui\icons\css\
Copy-Item "$dist\vendors\@coreui\icons\fonts\CoreUI-Icons-Free.*"  Content\vendor\@coreui\icons\fonts\
Copy-Item "$dist\vendors\simplebar\css\simplebar.css"              Content\vendor\simplebar\css\
Copy-Item "$dist\vendors\simplebar\js\simplebar.min.js"            Content\vendor\simplebar\js\
Copy-Item "$dist\js\color-modes.js*"                               Scripts\js\
Copy-Item "$logo\masaito-logo-light-gradient.svg"                  Src\Image\
Copy-Item "$logo\masaito-logo-dark-gradient.svg"                   Src\Image\
Copy-Item "$logo\masaito-mark-light-gradient.svg"                  Src\Image\
Copy-Item "$logo\masaito-mark-dark-gradient.svg"                   Src\Image\
Copy-Item "$om\Scripts\angular-growl.min.js"                       Scripts\
Copy-Item "$om\Content\vendor\growl\angular-growl.min.css"         Content\vendor\growl\
```

If you do not have the OneMasaito folder, `angular-growl-v2 0.7.3` is on GitHub — download `build/angular-growl.min.js` and `build/angular-growl.min.css` from https://github.com/JanStevens/angular-growl-2 into the same two locations.

`Scripts/angular.min.js` comes from the `angularjs 1.8.2` NuGet package that is already in `packages.config`. If it is missing from `Scripts/`, run `Update-Package -reinstall angularjs -ProjectName CoreUIDemo` in the Package Manager Console.

### 7.2 Include the new files in the project

Solution Explorer → **Show All Files** (toolbar icon) → select `Content\build`, `Content\vendor`, `Scripts\js`, `Src`, and `Scripts\angular-growl.min.js` → right-click → **Include In Project**. Turn *Show All Files* off again. Un-included files are not deployed and, for CSS/JS, produce a silent 404.

### 7.3 `Content/Site.css`

Replace the MVC template's `Site.css` with the loader spinner that OneMasaito's theme ships (its `sb-admin-2.css` defines `.loader`; CoreUI's `style.css` does not), and a small shim that gives angular-growl back the Bootstrap 4 look it has in OneMasaito.

⚠️ **DEVIATION** — the growl shim is CoreUI-specific. angular-growl puts an `icon` class on every notification for its severity image, and CoreUI's `style.css` defines `.icon` as a 1rem inline-block (for CoreUI Icons), which collapses the notification to a 47-pixel box. Bootstrap 5 also dropped the `.close` class that growl's × and countdown buttons use. Without the shim the growls render, but unreadably.

```css
/* Full-page loader shown by _Layout until mainController.Init() has loaded the current user
   (ported from OneMasaito's Content/build/css/sb-admin-2.css). */
.loader {
    border: 16px solid #f3f3f3;
    border-top: 16px solid #3498db;
    border-radius: 50%;
    width: 120px;
    height: 120px;
    animation: spin 2s linear infinite;
}

@keyframes spin {
    0% {
        transform: rotate(0deg);
    }

    100% {
        transform: rotate(360deg);
    }
}

/* angular-growl on Bootstrap 5 / CoreUI — restore the Bootstrap 4 look OneMasaito had.
   1. CoreUI's own `.icon` rule (style.css) collapses any element carrying that class to a
      1rem inline-block; angular-growl puts `icon` on every growl item for its severity image,
      so undo that here.
   2. Bootstrap 5 dropped `.close` (now `.btn-close`); growl's × and countdown buttons still
      use it, so re-create Bootstrap 4's `.close`, scoped to growl. */
.growl-container > .growl-item.icon {
    display: block;
    width: auto;
    height: auto;
    font-size: inherit;
    color: var(--cui-alert-color);
    text-align: left;
    vertical-align: baseline;
}

.growl-container > .growl-item > button.close {
    float: right;
    padding: 0;
    margin-left: 4px;
    font-size: 1.5rem;
    font-weight: 700;
    line-height: 1;
    color: #000;
    text-shadow: 0 1px 0 #fff;
    background-color: transparent;
    border: 0;
    opacity: .5;
}

.growl-container > .growl-item > button.close:hover {
    opacity: .75;
}

/* AngularJS validation state, per the AngularJS 1.8 Forms guide: ngModel puts ng-invalid /
   ng-touched on each control and the form directive puts ng-submitted on the <form>, so the
   invalid border needs no ng-class on any input. Same reveal rule as the messages themselves
   ($submitted || $touched), and --cui-form-invalid-border-color follows CoreUI's dark mode.
   Contract: docs/superpowers/specs/2026-09-24-dataannotations-angular-validation-design.md */
form.ng-submitted .form-control.ng-invalid,
form.ng-submitted .form-select.ng-invalid,
.form-control.ng-invalid.ng-touched,
.form-select.ng-invalid.ng-touched {
    border-color: var(--cui-form-invalid-border-color);
}

/* The validation message row keeps its space while hidden. Without this, revealing a message
   when a field is blurred pushes the Save button out from under the pointer between mousedown
   and mouseup, so the browser never fires a click and the first Save press after typing is
   silently swallowed. Site.css is bundled last, so this wins over AngularJS's own .ng-hide. */
.field-feedback.ng-hide {
    display: block !important;
    visibility: hidden;
}
```

### 7.4 `App_Start/BundleConfig.cs`

Same three bundle names as OneMasaito. Order inside `~/bundles/scripts` matters: jQuery first (the `#firstName` / `#lastName` keypress filters in § 9.3 were its only consumer and are commented out since 2026-09-22, so nothing in the app calls jQuery today — the order matters again the moment you uncomment them), then CoreUI (so the `coreui` global exists before any Angular controller runs), then Angular, then angular-growl (which registers a module on `angular`).

jQuery is referenced as `~/Scripts/jquery-{version}.js` rather than by an exact file name: the repo currently ships `jquery-3.7.0.js` even though `packages.config` says 3.7.1, and a bundle entry naming a file that is not on disk is skipped **silently** (no build error, no 404 in the console — just `$ is not defined` later). The `{version}` wildcard matches whichever version is installed and picks the `.min` file automatically when `EnableOptimizations` is true.

```csharp
using System.Web;
using System.Web.Optimization;

namespace CoreUIDemo
{
    public class BundleConfig
    {
        // For more information on bundling, visit https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at https://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/build/css/style.css",
                      "~/Content/vendor/@coreui/icons/css/free.min.css",
                      "~/Content/vendor/simplebar/css/simplebar.css",
                      "~/Content/vendor/growl/angular-growl.min.css",
                      "~/Content/Site.css"
                      ));

            // coreui.bundle.min.js is built by a modern toolchain whose output the ~2013-era
            // Microsoft.Ajax.Utilities minifier (System.Web.Optimization's default JS transform)
            // cannot parse — it throws a NullReferenceException instead of falling back, the way
            // the CSS minifier does for unparseable CSS. Hitting the bundle URL runs the minify
            // transform regardless of BundleTable.EnableOptimizations (that flag only controls
            // whether Scripts.Render links the bundle vs. individual files) — so Transforms.Clear()
            // is required here, not just #if DEBUG below. Every file in this bundle is already
            // minified/production-ready, so skipping the transform costs nothing.
            var scriptsBundle = new ScriptBundle("~/bundles/scripts").Include(
                "~/Scripts/jquery-{version}.js",
                "~/Content/vendor/@coreui/coreui/js/coreui.bundle.min.js",
                "~/Content/vendor/simplebar/js/simplebar.min.js",
                "~/Scripts/angular.min.js",
                "~/Scripts/angular-growl.min.js"
                );
            scriptsBundle.Transforms.Clear();
            bundles.Add(scriptsBundle);

            bundles.Add(new ScriptBundle("~/bundles/angular").Include(
                "~/App/GrowlConfig.js",
                "~/App/App.js",
                "~/App/Controller/Login.js",
                "~/App/Controller/UserAccounts.js"
                ));

#if DEBUG
            BundleTable.EnableOptimizations = false;
#else
            BundleTable.EnableOptimizations = true;
#endif
        }
    }
}
```

Four things to know:

- **`~/Content/css` is a bundle *name*, and the physical folder `Content/css/` was deleted in § 1.** If that folder exists, IIS serves the folder (403/404) instead of the bundle and the whole site renders unstyled with no error anywhere. This is the reason § 1 deletes it. Keep it deleted.
- **`Scripts/js/color-modes.js` is not in any bundle.** It has to run in `<head>` before first paint (it sets `data-coreui-theme` on `<html>` from `localStorage`); `_Layout.cshtml` loads it with a plain `<script>` tag (§ 8).
- `free.min.css` references its fonts as `../fonts/CoreUI-Icons-Free.woff`. Bundling rewrites relative URLs to the bundle's virtual path, so the fonts must sit next to the css exactly as copied (`Content/vendor/@coreui/icons/fonts/`). Do not move them.
- **`~/bundles/scripts`'s `Transforms.Clear()` is not optional.** Requesting a bundle URL directly always runs its transforms (minification) — `BundleTable.EnableOptimizations` only decides whether `@Scripts.Render(...)` *links to* the bundle URL or to each file individually, not whether the transform runs when that URL is hit. Without `Transforms.Clear()`, the first request to `~/bundles/scripts` throws an unhandled `NullReferenceException` from deep inside `Microsoft.Ajax.Utilities.JSParser` — confirmed by isolating `coreui.bundle.min.js` into its own bundle and hitting it directly; the old minifier cannot parse the modern syntax CoreUI's build outputs. Appendix A has the symptom if you ever remove this line while debugging something else.

⚠️ **DEVIATION** — `~/bundles/bootstrap` is gone (OneMasaito registers it and never renders it — CoreUI's bundle contains Bootstrap). DataTables, Chart.js, jquery-easing, moment, respond, angular-file-upload are gone (used by modules that do not exist here). The `#if DEBUG` optimisation switch is added so you get readable, unminified files while debugging.

### 7.5 `App_Start/RouteConfig.cs`

The application root is the **login page**, as in OneMasaito. There is no anonymous landing page.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace CoreUIDemo
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Login", id = UrlParameter.Optional }
            );
        }
    }
}
```

✅ **VERIFY**
- `Build → Rebuild` — 0 errors.
- In Solution Explorer, `Content/build/css/style.css`, `Content/vendor/@coreui/coreui/js/coreui.bundle.min.js`, `Scripts/angular.min.js`, `Scripts/angular-growl.min.js`, `Scripts/js/color-modes.js`, `Src/Image/masaito-logo-dark-gradient.svg` all appear **without** the "not included" dotted icon.
- `Content/css` does **not** exist on disk.
- `Scripts/` contains exactly one `jquery-<version>.js` (delete any second version, or the `{version}` wildcard would include both).

🔍 **GIT CHECKPOINT 7**

```bash
git add -A
git commit -m "feat: vendor CoreUI v5.5.0 and bundles"
```

---

## § 8 — Layout, Login and Dashboard views

The views mirror OneMasaito's `Views/Shared/_Layout.cshtml`, `Views/Home/Login.cshtml` and `Views/Home/Index.cshtml`, with CoreUI's shell markup (from the dist's `index.html` and `authentication/login.html`) in place of SB Admin 2's. Everything Angular-related — `ng-app`, `ng-controller`, `ng-init="Init()"`, the loader, the growl container, the two global modals — is OneMasaito's, verbatim where the markup allows.

Two CoreUI facts drive the markup differences from OneMasaito (details in `COREUI_GUIDE.md` §7):

- Data attributes are `data-coreui-toggle` / `data-coreui-target` / `data-coreui-dismiss` (not `data-toggle` …).
- The JavaScript global is `coreui`, not `bootstrap`, and there is **no jQuery plugin API** — `$('#x').modal('show')` does not exist. § 9's controllers use `coreui.Modal.getOrCreateInstance(...)` instead.

### 8.1 `Views/_ViewStart.cshtml`

Unchanged from the template (and identical to OneMasaito's):

```cshtml
@{
    Layout = "~/Views/Shared/_Layout.cshtml";
}
```

### 8.2 `Views/Shared/_Layout.cshtml`

Structure (top to bottom): head → `mainController` wrapper with loader → growl → CoreUI sidebar → CoreUI wrapper (header / body / footer) → Logout modal → Change Password modal.

```cshtml
<!DOCTYPE html>
<html ng-app="app">
<head>
    <meta http-equiv="content-type" content="text/html; charset=UTF-8" />
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0, shrink-to-fit=no" />
    <link href="~/Src/Image/masaito-mark-light-gradient.svg" rel="icon" type="image/svg+xml" />
    <title>CoreUIDemo</title>
    @Styles.Render("~/Content/css")
    <script src="~/Scripts/js/color-modes.js"></script>
    @Scripts.Render("~/bundles/scripts")
    @Scripts.Render("~/bundles/angular")
</head>
<body>
    <div ng-controller="mainController as main" ng-init="Init()">
        <div class="d-flex justify-content-center align-items-center vh-100" ng-hide="main.ItemLoad">
            <div class="loader"></div>
        </div>
        <div ng-show="main.ItemLoad">
            <div growl class="fading"></div>

            <!-- Sidebar -->
            <div class="sidebar sidebar-dark sidebar-fixed border-end" id="sidebar">
                <div class="sidebar-header border-bottom">
                    <div class="sidebar-brand me-auto">
                        <img class="sidebar-brand-full" style="height:32px;" src="~/Src/Image/masaito-logo-dark-gradient.svg" alt="Masaito Development Corporation" />
                        <img class="sidebar-brand-narrow" style="height:32px;" src="~/Src/Image/masaito-mark-dark-gradient.svg" alt="Masaito" />
                    </div>
                    <button class="btn-close d-lg-none" type="button" data-coreui-theme="dark" aria-label="Close"
                            onclick="coreui.Sidebar.getOrCreateInstance(document.querySelector('#sidebar')).toggle()">
                    </button>
                </div>
                <ul class="sidebar-nav" data-coreui="navigation" data-simplebar>
                    <!-- Nav Item - Dashboard -->
                    <li class="nav-item">
                        <a class="nav-link" href="/Home/Index">
                            <i class="nav-icon cil-speedometer"></i>
                            Dashboard
                        </a>
                    </li>

                    <!-- Heading -->
                    <li class="nav-title">Modules</li>

                    <!-- Nav Group - Settings (admin only) -->
                    <li class="nav-group" ng-show="main.CurrentUser.Role === 'admin'">
                        <a class="nav-link nav-group-toggle" href="#">
                            <i class="nav-icon cil-settings"></i>
                            Settings
                        </a>
                        <ul class="nav-group-items compact">
                            <li class="nav-item">
                                <a class="nav-link" href="/Settings/UserAccounts">
                                    <i class="nav-icon cil-people"></i>
                                    User Account
                                </a>
                            </li>
                        </ul>
                    </li>
                </ul>
            </div>
            <!-- End of Sidebar -->

            <!-- Content Wrapper -->
            <div class="wrapper d-flex flex-column min-vh-100">

                <!-- Topbar -->
                <header class="header header-sticky p-0 mb-4">
                    <div class="container-fluid border-bottom px-4">
                        <button class="header-toggler" type="button" style="margin-inline-start: -14px"
                                onclick="coreui.Sidebar.getOrCreateInstance(document.querySelector('#sidebar')).toggle()">
                            <i class="icon icon-lg cil-menu"></i>
                        </button>

                        <div class="input-group ms-3" style="max-width:500px;">
                            <input type="text" class="form-control" placeholder="Search for..."
                                   aria-label="Search" ng-model="main.SearchBox">
                            <span class="input-group-text"><i class="cil-search"></i></span>
                        </div>

                        <!-- Topbar Navbar -->
                        <ul class="header-nav ms-auto">
                            <!-- Theme switcher (required by color-modes.js — see COREUI_GUIDE §9) -->
                            <li class="nav-item dropdown">
                                <button class="btn btn-link nav-link py-2 px-2 d-flex align-items-center" type="button"
                                        aria-expanded="false" data-coreui-toggle="dropdown">
                                    <span class="theme-icon-active">
                                        <svg class="icon icon-lg theme-icon-active" xmlns="http://www.w3.org/2000/svg" viewBox="0 0 512 512">
                                            <path fill="var(--ci-primary-color, currentcolor)" d="M256 16C123.452 16 16 123.452 16 256s107.452 240 240 240 240-107.452 240-240S388.548 16 256 16m-22 446.849a208.35 208.35 0 0 1-169.667-125.9c-.364-.859-.706-1.724-1.057-2.587L234 429.939Zm0-69.582L50.889 290.76A210 210 0 0 1 48 256q0-9.912.922-19.67L234 339.939Zm0-90L54.819 202.96a206 206 0 0 1 9.514-27.913Q67.1 168.5 70.3 162.191L234 253.934Zm0-86.015L86.914 134.819a209.4 209.4 0 0 1 22.008-25.9q3.72-3.72 7.6-7.228L234 166.027Zm0-87.708-89.648-49.093A206.95 206.95 0 0 1 234 49.151ZM464 256a207.775 207.775 0 0 1-198 207.761V48.239A207.79 207.79 0 0 1 464 256" class="ci-primary" />
                                        </svg>
                                    </span>
                                </button>
                                <ul class="dropdown-menu dropdown-menu-end" style="--cui-dropdown-min-width: 8rem">
                                    <li><button class="dropdown-item" type="button" data-coreui-theme-value="light">Light</button></li>
                                    <li><button class="dropdown-item" type="button" data-coreui-theme-value="dark">Dark</button></li>
                                    <li><button class="dropdown-item active" type="button" data-coreui-theme-value="auto">Auto</button></li>
                                </ul>
                            </li>

                            <li class="nav-item py-1">
                                <div class="vr h-100 mx-2 text-body text-opacity-75"></div>
                            </li>

                            <!-- Nav Item - User Information -->
                            <li class="nav-item dropdown">
                                <a class="nav-link py-0 pe-0 d-flex align-items-center" data-coreui-toggle="dropdown" href="#" role="button"
                                   aria-haspopup="true" aria-expanded="false">
                                    <span class="me-2 d-none d-lg-inline text-body-secondary small">{{main.CurrentUser.FirstName}} {{main.CurrentUser.LastName}}</span>
                                    <i class="icon icon-lg cil-user"></i>
                                </a>
                                <!-- Dropdown - User Information -->
                                <div class="dropdown-menu dropdown-menu-end pt-0">
                                    <div class="dropdown-header bg-body-tertiary text-body-secondary fw-semibold rounded-top mb-2">Account</div>
                                    <a class="dropdown-item" href="#" data-coreui-toggle="modal" data-coreui-target="#PasswordModal" ng-click="OpenPasswordModal()">
                                        <i class="cil-lock-locked me-2"></i>
                                        Change Password
                                    </a>
                                    <div class="dropdown-divider"></div>
                                    <a class="dropdown-item" href="#" data-coreui-toggle="modal" data-coreui-target="#logoutModal">
                                        <i class="cil-account-logout me-2"></i>
                                        Logout
                                    </a>
                                </div>
                            </li>
                        </ul>
                    </div>
                </header>
                <!-- End of Topbar -->

                <!-- Begin Page Content -->
                <div class="body flex-grow-1">
                    <div class="container-fluid px-4">
                        @RenderBody()
                    </div>
                </div>
                <!-- End of Page Content -->

                <!-- Footer -->
                <footer class="footer px-4">
                    <div><span class="text-primary"><b>&copy;2026 Management Information System.</b></span></div>
                </footer>
                <!-- End of Footer -->

            </div>
            <!-- End of Content Wrapper -->

            <!-- Logout Modal-->
            <div class="modal fade" id="logoutModal" tabindex="-1" role="dialog" aria-labelledby="logoutModalLabel" aria-hidden="true">
                <div class="modal-dialog" role="document">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="logoutModalLabel">Ready to Leave?</h5>
                            <button class="btn-close" type="button" data-coreui-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">Select "Logout" below if you are ready to end your current session.</div>
                        <div class="modal-footer">
                            <button class="btn btn-secondary" type="button" data-coreui-dismiss="modal">Cancel</button>
                            <a class="btn btn-primary" ng-click="Logout()">Logout</a>
                        </div>
                    </div>
                </div>
            </div>

            <!--Change Password Modal-->
            <div class="modal fade" id="PasswordModal" tabindex="-1" role="dialog" aria-hidden="true">
                <div class="modal-dialog">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title">Change Password</h5>
                            <button class="btn-close" type="button" data-coreui-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <form name="PasswordForm" ng-submit="ChangePassword(main.ChangePassword)" autocomplete="off" novalidate>
                        <div class="modal-body">
                            <div class="mb-3">
                                <label class="form-label">Current Password</label>
                                <input type="password" class="form-control" name="CurrentPassword" ng-model="main.ChangePassword.CurrentPassword" required />
                                <div class="field-feedback" ng-show="PasswordForm.$submitted || PasswordForm.CurrentPassword.$touched">
                                    <div class="invalid-feedback d-block" ng-show="PasswordForm.CurrentPassword.$error.required">Please input Current Password</div>
                                </div>
                            </div>
                            <div class="mb-3">
                                <label class="form-label">New Password</label>
                                <input type="password" class="form-control" name="NewPassword" ng-model="main.ChangePassword.NewPassword" required ng-minlength="6" ng-maxlength="255" />
                                <div class="field-feedback" ng-show="PasswordForm.$submitted || PasswordForm.NewPassword.$touched">
                                    <div class="invalid-feedback d-block" ng-show="PasswordForm.NewPassword.$error.required || PasswordForm.NewPassword.$error.minlength">Password must be at least 6 characters</div>
                                </div>
                            </div>
                            <div class="mb-3">
                                <label class="form-label">Confirm Password</label>
                                <input type="password" class="form-control" name="ConfirmPassword" ng-model="main.ChangePassword.ConfirmPassword" required />
                                <div class="field-feedback" ng-show="PasswordForm.$submitted || PasswordForm.ConfirmPassword.$touched">
                                    <div class="invalid-feedback d-block" ng-show="PasswordForm.ConfirmPassword.$error.required">Password Not Match!</div>
                                    <div class="invalid-feedback d-block" ng-show="main.ChangePassword.ConfirmPassword && main.ChangePassword.ConfirmPassword !== main.ChangePassword.NewPassword">Password Not Match!</div>
                                </div>
                            </div>
                        </div>
                        <div class="modal-footer">
                            <button class="btn btn-primary" type="submit">Change Password</button>
                            <button class="btn btn-secondary" type="button" data-coreui-dismiss="modal">Cancel</button>
                        </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    </div>
</body>
</html>
```

What is OneMasaito's and what is CoreUI's:

- **OneMasaito**: scripts rendered in `<head>`; `ng-controller="mainController as main" ng-init="Init()"` on the outermost div; the `.loader` shown until `main.ItemLoad`; `<div growl class="fading">`; the header search box bound to `main.SearchBox` (the accounts grid filters on it); the user dropdown with *Change Password* (opens `#PasswordModal` **and** calls `OpenPasswordModal()` to clear the fields — OneMasaito wires both) and *Logout* (opens `#logoutModal`, whose confirm button calls `Logout()`); both modals' contents.
- **CoreUI**: `div.sidebar` / `ul.sidebar-nav[data-coreui="navigation"]` / `div.wrapper` / `header.header` / `div.body` / `footer.footer`; `data-coreui-*` attributes; `btn-close`; `mb-3` instead of `form-group`; the theme dropdown.
- **Change Password modal**: the body and footer sit inside `<form ng-submit="ChangePassword(main.ChangePassword)" autocomplete="off" novalidate>` and the *Change Password* button is `type="submit"`, so Enter in any of the three fields submits; *Cancel* is `type="button"` so it only dismisses. OneMasaito uses `ng-click` on the button and has no form element; the fields, `ng-model` names and controller call are the same.
- **The theme dropdown is not optional.** `color-modes.js` runs `showActiveTheme()` on `DOMContentLoaded` and dereferences the button matching `[data-coreui-theme-value="…"]`; with no such buttons it throws a `TypeError` in the console on every page. If you do not want a theme switcher, remove **both** the dropdown and the `<script src="~/Scripts/js/color-modes.js">` tag and hard-code `<html ng-app="app" data-coreui-theme="light">`.
- Sidebar gating: OneMasaito shows its *Settings* group when `Department === 1 || Settings === 2 || Settings === 3`. With neither column here, the equivalent is `Role === 'admin'`. This is a UI convenience only — `/Settings/UserAccounts` itself checks only that *someone* is logged in (§ 6), exactly like OneMasaito.

⚠️ **DEVIATION** — Google Fonts `<link>` (Nunito) removed; CoreUI uses the system font stack. The `#sidebarToggle` button and the `$("#sidebarToggle").click(...)` jQuery block in `App.js` are gone — CoreUI's `header-toggler` handles it. The "Patch Notes" nav item becomes "Dashboard". The mobile search dropdown (`d-sm-none`) is dropped; the one search input is enough.

⚠️ **DEVIATION (2026-09-22)** — three further departures from the dist shell, all visible in the block above:

- **No sidebar footer.** The dist's `div.sidebar-footer` holding `button.sidebar-toggler[data-coreui-toggle="unfoldable"]` is deleted. The sidebar is always full width on desktop and the header's `header-toggler` (hide / show) is the only sidebar control left. Narrow mode is still reachable — put `sidebar-narrow` on the `div.sidebar` permanently (`COREUI_GUIDE.md` § 5, `docs/coreui/08-customizing.md`).
- **The theme toggle is an icon, not the word *Theme*.** `<span class="theme-icon-active">` now wraps an inline `<svg class="icon icon-lg theme-icon-active">` (CoreUI's contrast glyph). The icon is **static**: `color-modes.js` copies an icon into that span only when the matching dropdown *item* contains an `<svg>`, and the three items here are text labels — so the contrast glyph shows whatever the active theme is. `COREUI_GUIDE.md` § 9 has the rule and what to add if you want the icon to track the theme.
- **Footer line** reads `© 2026 Management Information System.`

### 8.3 `Views/Home/Login.cshtml`

`Layout = null` and its own `ng-app="login"` — a completely separate Angular application from the layout's `app`, exactly as OneMasaito. The card is CoreUI's `authentication/login.html` reduced to what OneMasaito's login has: username, password, one button. (The `<hr />` + `version 1.0.0` line under the form was removed on 2026-09-22, and the placeholders shortened to `Enter Username` / `Enter Password`.)

```cshtml
@{
    Layout = null;
}

<!DOCTYPE html>

<html ng-app="login" data-coreui-theme="light">
<head>
    <meta http-equiv="content-type" content="text/html; charset=UTF-8" />
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0, shrink-to-fit=no" />
    <link href="~/Src/Image/masaito-mark-light-gradient.svg" rel="icon" type="image/svg+xml" />

    @Styles.Render("~/Content/css")
    @Scripts.Render("~/bundles/scripts")
    @Scripts.Render("~/bundles/angular")

    <title>Login</title>
</head>
<body class="bg-body-tertiary min-vh-100 d-flex flex-row align-items-center" ng-controller="loginController as vm">
    <div growl class="fading"></div>

    <div class="container" style="max-width: 32rem">
        <div class="d-flex flex-column gap-4">
            <div class="text-center">
                <img style="height:48px;" src="~/Src/Image/masaito-logo-light-gradient.svg" alt="Masaito Development Corporation" />
            </div>

            <div class="card p-4">
                <div class="card-body d-flex flex-column gap-4">
                    <h2 class="h5 text-center">Login to your account</h2>
                    <form class="row gap-3" name="LoginForm" autocomplete="off" novalidate ng-submit="TryLogin()">
                        <div>
                            <label class="form-label" for="username">Username</label>
                            <input class="form-control" id="username" name="Username" type="text" placeholder="Enter Username"
                                   ng-model="vm.Username" required ng-maxlength="50" />
                            <div class="field-feedback" ng-show="LoginForm.$submitted || LoginForm.Username.$touched">
                                <div class="invalid-feedback d-block" ng-show="LoginForm.Username.$error.required">Please input Username</div>
                                <div class="invalid-feedback d-block" ng-show="LoginForm.Username.$error.maxlength">Username is too long (maximum 50)</div>
                            </div>
                        </div>
                        <div>
                            <label class="form-label" for="password">Password</label>
                            <input class="form-control" id="password" name="Password" type="password" placeholder="Enter Password"
                                   ng-model="vm.Password" required />
                            <div class="field-feedback" ng-show="LoginForm.$submitted || LoginForm.Password.$touched">
                                <div class="invalid-feedback d-block" ng-show="LoginForm.Password.$error.required">Please input Password</div>
                            </div>
                        </div>
                        <div>
                            <button class="btn btn-primary w-100" type="submit">Login</button>
                        </div>
                    </form>
                </div>
            </div>
        </div>
    </div>
</body>
</html>
```

The `<form>` is named `LoginForm`, has no `action`, carries `ng-submit="TryLogin()"`, and its Login button is `type="submit"`. ⚠️ **DEVIATION (2026-09-24)** — both inputs are `required` and render their message under the field, and `TryLogin()` opens with an `$invalid` guard; until then this page had no client-side validation at all and an empty post came back as *Invalid Username or Password!!* from the server. AngularJS's `ngSubmit` calls `preventDefault()` on a form with no `action`, so clicking the button or pressing Enter in either field runs `TryLogin()` and nothing reloads. (OneMasaito uses `type="button"` + `ng-click` and binds Enter with a jQuery `keypress` handler in `Login.js`; the form-submit route is one attribute and covers both.)

`data-coreui-theme="light"` on `<html>` pins the login page to the light theme because this page does not load `color-modes.js` (it has no theme dropdown for that script to bind to — see § 8.2). `COREUI_GUIDE.md` §9 shows how to make it follow the saved theme if you want that.

⚠️ **DEVIATION** — CoreUI's login card also has "I forgot password", "Remember me", "Login with Google/Apple" and "Sign up". OneMasaito has none of these, so neither does this page. OneMasaito's two-column card with the 50th-anniversary image on the left is replaced by CoreUI's single-column card.

### 8.4 `Views/Home/Index.cshtml`

OneMasaito's `Index` is an 1,830-line "Patch Notes" page. The shell — a page rendered inside `_Layout` that redirects to Login when there is no user — is what is mirrored; the content is a placeholder card.

```cshtml
@{
    ViewBag.Title = "Dashboard";
}

<h1 class="h3 mb-4">Dashboard</h1>

<div class="card mb-4">
    <div class="card-header">Welcome</div>
    <div class="card-body">
        <p class="mb-1">Signed in as <b>{{main.CurrentUser.FullName}}</b> ({{main.CurrentUser.Username}}, role: {{main.CurrentUser.Role}}).</p>
        <p class="text-body-secondary mb-0" ng-show="main.CurrentUser.Role === 'admin'">Use <b>Settings &rarr; User Account</b> in the sidebar to manage accounts.</p>
    </div>
</div>
```

✅ **VERIFY** — press **F5**.
- The browser opens at `/Home/Login` (the default route) and the card renders with CoreUI styling. Open DevTools → Console: no errors. (`Unknown provider: growlProvider` or `[$injector:modulerr]` means a bundle is wrong — see Appendix A.)
- Navigate to `/Home/Index` by hand — you are redirected back to `/Home/Login` (no cookie yet).
- Navigate to `/Home/GetCurrentUser` — the response is `{"obj":null}`.
- Login itself does not work yet: `Login.js` is written in § 9.

🔍 **GIT CHECKPOINT 8**

```bash
git add -A
git commit -m "feat: layout, login and dashboard views"
```

---

## § 9 — User Accounts view and Angular controllers

Three Angular files mirror OneMasaito's `App/App.js`, `App/Controller/Login.js` and `App/Controller/UserAccounts.js`: **one Angular module per page**. `app` (the layout) lists the page modules as dependencies; `useraccount` depends on `app` back (Angular tolerates the cycle; OneMasaito relies on it); `login` stands alone because the login page has no layout. A fourth, tiny file — `App/GrowlConfig.js` — holds the growl settings both `app` and `login` share. All four files are in `~/bundles/angular` and load on every page.

Validation in the browser is done the OneMasaito way: a chain of `if / else if` checks that each `growl.error(...)` a fixed string, with the actual `$http` call in the final `else`. The two user-requested rules (letters-only names, 6-character passwords) are added to those chains with the same shape. Modals are shown/hidden through two tiny global helpers because CoreUI has no jQuery plugin API.

⚠️ **DEVIATION** — OneMasaito passes `{ ttl: N }` (and sometimes `title: "Error!"`) on every growl call, with N varying from 2000 to 5000 across the files. Here the time-to-live is set **once**, per severity, in `GrowlConfig.js` (success 3 s, error 5 s), and every call is just `growl.error("…")` / `growl.success("…")` with no title — the red/green box already says which it is. A single call can still pass `{ ttl: N }` to override. See `docs/superpowers/specs/2026-09-21-global-growl-ttl-design.md`.

### 9.0 `App/GrowlConfig.js`

New file. Add it to the project (`<Content Include="App\GrowlConfig.js" />` next to `App\App.js` in the `.csproj`, or *Include In Project* from Solution Explorer); § 7.4 already lists it first in `~/bundles/angular`.

```js
angular.module("growlConfig", ["angular-growl"])
    .config(['growlProvider', function (growlProvider) {
        growlProvider.globalTimeToLive({ success: 3000, error: 5000, warning: 5000, info: 3000 });
    }]);
```

angular-growl resolves a message's TTL as `options.ttl || globalTtl[severity]`, so with no inline `ttl` every message uses these values. `login` and `app` both list `growlConfig` as a dependency (below); `useraccount` gets it through `app`.

### 9.1 `App/App.js`

```js
var app = angular.module('app', ["angular-growl", "growlConfig", "login", "useraccount"])
    .controller("mainController", ['$scope', '$location', '$http', 'growl', function ($scope, $location, $http, growl) {
        var main = this;

        main.ChangePassword = {};

        main.ChangePassword.CurrentPassword = "";

        main.ChangePassword.NewPassword = "";

        main.ChangePassword.ConfirmPassword = "";

        main.SearchBox = "";

        main.ItemLoad = true;

        PopUpMessage = function (data) {
            if (data.message == "Saved" || data.message == "Updated" || data.message == "Deleted") {
                growl.success("Successfully " + data.message);
            }
            else {
                growl.error(data.message);
            }
        };

        ShowModal = function (id) {
            coreui.Modal.getOrCreateInstance(document.getElementById(id)).show();
        };

        HideModal = function (id) {
            coreui.Modal.getOrCreateInstance(document.getElementById(id)).hide();
        };

        // Re-arms the modal's form: $setPristine also clears $submitted, $setUntouched clears the
        // per-field touched flags, so a reopened modal shows no errors from the previous attempt.
        var ResetForm = function (form) {
            if (form) {
                form.$setPristine();

                form.$setUntouched();
            }
        };

        $scope.Init = function () {
            main.ItemLoad = false;
            $http({
                method: "POST",
                url: "/Home/GetCurrentUser",
                arguments: { "Content-Type": "application/json" }
            }).then(function (data) {
                main.CurrentUser = data.data.obj;

                main.ItemLoad = true;
            });
        };

        $scope.ChangePassword = function (value) {

            // PasswordForm renders each message under its field. The match is the one cross-field
            // rule and is not part of the form's own validity, so it is re-checked here.
            if ($scope.PasswordForm.$invalid || value.ConfirmPassword !== value.NewPassword) {
                return;
            }

            $http({
                method: "POST",
                url: "/Home/ChangePassword",
                data: { password: value }
            }).then(function (data) {
                if (data.data.errorMessage == "") {
                    growl.success("Password Successfully Changed");

                    HideModal("PasswordModal");
                }
                else {
                    growl.error(data.data.errorMessage);

                    value.CurrentPassword = "";

                    value.NewPassword = "";

                    value.ConfirmPassword = "";

                    ResetForm($scope.PasswordForm);
                }
            });
        };

        $scope.Logout = function () {
            $http({
                method: "POST",
                url: "/Home/Logout",
                arguments: { "Content-Type": "application/json" }
            }).then(function (data) {
                if (data.data != "") {
                    growl.error(data.data);
                }
                else {
                    HideModal("logoutModal");

                    window.location.href = "/Home/Login";
                }
            });
        };

        $scope.OpenPasswordModal = function () {
            main.ChangePassword.CurrentPassword = "";

            main.ChangePassword.NewPassword = "";

            main.ChangePassword.ConfirmPassword = "";

            ResetForm($scope.PasswordForm);
        };

    }]);
```

- `PopUpMessage`, `ShowModal`, `HideModal` are deliberately **global** (no `var`), the way OneMasaito declares `PopUpMessage`, so `UserAccounts.js` can call them.
- `main.ItemLoad` starts `true`, `Init()` flips it to `false` while `/Home/GetCurrentUser` is in flight, then back to `true` — that is what shows and hides the `.loader` in `_Layout`. (OneMasaito's exact sequence.)
- `arguments: { "Content-Type": ... }` is not an `$http` option — it is a harmless typo carried over from OneMasaito; `$http` sends JSON by default.

⚠️ **DEVIATION** — the 35-module dependency list becomes 4 (`angular-growl`, `growlConfig`, `login`, `useraccount` — the last two are the page modules); `main.EntityList` / `main.ProjectList` / `main.SelectedModule` / `$scope.SelectModule` are gone; the `$("#sidebarToggle")` block is gone; the password-length check is new; `HideModal("logoutModal")` before redirecting is new (CoreUI leaves the modal backdrop on the page otherwise).

### 9.2 `App/Controller/Login.js`

```js
angular.module("login", ["angular-growl", "growlConfig"])
    .controller("loginController", ['$scope', '$location', '$http', 'growl', function ($scope, $location, $http, growl) {
        var vm = this;

        $scope.TryLogin = function () {

            // LoginForm carries the two required rules and renders their messages under the fields.
            if ($scope.LoginForm.$invalid) {
                return;
            }

            $http({
                method: "POST",
                url: "/Home/Login",
                data: {
                    username: vm.Username,
                    password: vm.Password
                }
            }).then(function (data) {
                if (data.data.errorMessage != "") {
                    growl.error(data.data.errorMessage);
                }
                else {
                    window.location.href = "/Home/Index";
                }
            });
        };
    }]);
```

Plus, since 2026-09-24, an `$invalid` guard at the top of `TryLogin()` — `LoginForm` carries the two `required` rules and renders their messages (§ 8.3).

A copy of OneMasaito's file plus the `growlConfig` dependency and the ttl-less growl call, minus OneMasaito's `$(document).on('keypress', …)` block — Enter is handled by the view's `ng-submit` (§ 8.3), so the controller only exposes `TryLogin()`. The response's `errorMessage` decides between a growl and a redirect. There is no client-side validation here because OneMasaito has none: an empty username/password simply comes back as `Invalid Username or Password!!`.

### 9.3 `App/Controller/UserAccounts.js`

```js
angular.module("useraccount", ["app"])

    .controller("accountsController", function ($scope, $location, $http, growl) {
        var vm = this;

        vm.RoleList = ["user", "manager", "admin"];

        vm.ChangePassword = {};

        vm.StatusFilter = "active";

        $scope.StatusMatch = function (acc) {
            if (vm.StatusFilter === "all") {
                return true;
            }

            if (vm.StatusFilter === "active") {
                return acc.IsActive;
            } else {
                return !acc.IsActive;
            }
        };


        // Re-arms a modal's form so a reopened modal never shows the previous attempt's errors:
        // $setPristine also clears $submitted, $setUntouched clears the per-field touched flags.
        var ResetForm = function (form) {
            if (form) {
                form.$setPristine();

                form.$setUntouched();
            }
        };

        $scope.Init = function () {
            $http({
                method: "POST",
                url: "/Settings/GetAccounts",
                arguments: { "Content-Type": "application/json" }
            }).then(function (data) {
                vm.AccountList = data.data.accountList;
            });

        };

        $scope.NewAccount = function () {
            vm.ModalHeader = "New";

            // Every bound field is initialised explicitly rather than left absent. A control whose
            // validator failed parks $modelValue at undefined, so a model without the property is
            // not a change, ngModel's watch never fires, and the rejected text stays in the input.
            vm.Modal = { Username: "", Password: null, FirstName: "", LastName: "", Role: "user" };

            ResetForm($scope.AccountForm);

            ShowModal("AccountModal");
        };

        $scope.EditAccount = function (value) {
            vm.ModalHeader = "Edit";

            vm.Modal = angular.copy(value);

            // The grid row carries no password; null (not "") both clears any stale view value and
            // is skipped by [StringLength] server-side, which an empty string would fail.
            vm.Modal.Password = null;

            ResetForm($scope.AccountForm);

            ShowModal("AccountModal");
        };

        $scope.Save = function () {

            // AccountForm carries every presence/format rule (see the modal markup); the messages
            // are rendered under each field, so there is nothing to growl here.
            if ($scope.AccountForm.$invalid) {
                return;
            }

            $http({
                method: "POST",
                url: "/Settings/SaveNewAccount",
                data: {
                    account: vm.Modal,
                    role: vm.Modal.Role
                }
            }).then(function (response) {
                PopUpMessage(response.data);

                $scope.Init();

                if (response.data.message == "Saved") {
                    HideModal("AccountModal");
                }
            });
        };

        /*$("#firstName").keypress(function (event) {
            var inputValue = event.which;

            if (!(inputValue >= 65 && inputValue <= 90) && !(inputValue >= 97 && inputValue <= 122) && inputValue != 32) {
                event.preventDefault();
            }
        });


        $('#lastName').keypress(function (event) {
            var inputValue = event.which;

            if (!(inputValue >= 65 && inputValue <= 90) && !(inputValue >= 97 && inputValue <= 122) && inputValue != 32) {
                event.preventDefault();
            }
        });*/

        $scope.UpdatePassword = function (value) {

            vm.Change = angular.copy(value);

            vm.Change.NewPassword = "";

            vm.Change.ConfirmPassword = "";

            ResetForm($scope.ChangePasswordForm);

            ShowModal("ChangePasswordModal");
        };

        $scope.ChangePassword = function () {

            // The match is the one cross-field rule, shown inline by the modal and re-checked here
            // because it is not part of the form's own validity.
            if ($scope.ChangePasswordForm.$invalid || vm.Change.NewPassword !== vm.Change.ConfirmPassword) {
                return;
            }

            $http({
                method: "POST",
                url: "/Settings/AdminChangePassword",
                data: {
                    account: vm.Change.ID,
                    password: vm.Change.NewPassword
                }

            }).then(function (response) {
                if (response.data.errorMessage == "") {
                    growl.success("Password Successfully Changed");

                    $scope.Init();

                    HideModal("ChangePasswordModal");
                }
                else {
                    growl.error(response.data.errorMessage)

                    vm.Change.NewPassword = "";

                    vm.Change.ConfirmPassword = "";

                    ResetForm($scope.ChangePasswordForm);
                }
            });
        };

        $scope.UpdateStatus = function (value) {

            vm.Status = angular.copy(value);

            vm.Status.ConfirmPassword = "";

            ResetForm($scope.StatusForm);

            ShowModal("UpdateStatusModal");
        };

        $scope.SaveStatus = function () {

            if ($scope.StatusForm.$invalid) {
                return;
            }

            $http({
                method: "POST",
                url: "/Settings/UpdateStatus",
                data: {
                    account: vm.Status.ID,
                    password: vm.Status.ConfirmPassword
                }
            }).then(function (response) {
                if (response.data.errorMessage == "") {
                    growl.success("Account Status Successfully Changed");

                    $scope.Init();

                    HideModal("UpdateStatusModal");
                }
                else {
                    growl.error(response.data.errorMessage)

                    vm.Status.ConfirmPassword = "";

                    ResetForm($scope.StatusForm);
                }
            });
        }
    });
```

How each piece maps to OneMasaito:

- `Init` → `/Settings/GetAccounts`; only `accountList` comes back now.
- `NewAccount` / `EditAccount` / `Save` → the Account modal. `vm.Modal.Role` is posted twice — inside `account` (bound to `UserModel.Role`) and as the separate `role` parameter — because the controller signature keeps OneMasaito's `(UserModel account, <second param>)` shape.
- `vm.StatusFilter` (`"active"` default, `"inactive"`, `"all"`) and `$scope.StatusMatch(acc)` drive the status filter in the grid header — a second `filter:` on the `ng-repeat`, client-side like the search box. Not in OneMasaito.
- ⚠️ **DEVIATION (2026-09-24)** — **there is no validation chain in this file any more.** Each handler opens with one guard — `if ($scope.AccountForm.$invalid) { return; }` — and every presence/format rule, with its message, lives on the form in § 9.4. OneMasaito's sequential `if`/`growl.error` ladder (and the `!x` shorthand that replaced its `== "" || == null`) is gone, along with the `namePattern` constant and the two `.trim()` locals: `ngTrim` already trims text inputs, and `[Required]` trims before testing on the server.
- `NewAccount` initialises **every** bound field (`Username`, `Password`, `FirstName`, `LastName`, `Role`) instead of just `Role`, and `EditAccount` sets `Password` to `null` after its `angular.copy`. This is not tidiness — it is required. A control whose validator failed parks `$modelValue` at `undefined`, so assigning a model that simply *omits* the property is not a change, `ngModel`'s watch never fires, `$render()` never runs, and the rejected text stays in the input while the model is empty. Found by testing on 2026-09-24: type a 51-character first name, close the modal, reopen it — the old text was still there. `null` rather than `""` for `Password` because `[StringLength(255, MinimumLength = 6)]` skips `null` but *fails* an empty string, which would break every Edit save.
- `ResetForm(form)` calls `$setPristine()` (which also clears `$submitted`) and `$setUntouched()`. Every handler that opens a modal calls it, because these modals are server-rendered once and reused — without it, reopening one shows the previous attempt's red fields. The handlers that blank a password after a *server* rejection call it too, so the cleared fields do not immediately read as errors.
- The Confirm-matches-New rule is the one cross-field check. It has no built-in AngularJS validator, so the modal shows it with a plain expression (`vm.Change.ConfirmPassword && vm.Change.ConfirmPassword !== vm.Change.NewPassword`) and the handler re-tests it beside `$invalid`, since the form's own validity does not cover it. `[Compare]` and `sp_UpdateUserPassword` are the nets behind it.
- Where OneMasaito's `Save` chain went: Username → Password → First Name → Last Name → Department is now the field order in the modal markup, each rule an attribute on its input. The create-only password rules are `ng-required="vm.ModalHeader === 'New'"` (AngularJS's `minlength` passes on empty values, so the hidden field on Edit never blocks a save), and Department is Role.
- The `#firstName` / `#lastName` keypress filters are OneMasaito's own — they block anything that is not a letter or a space as it is typed. **They have been commented out since 2026-09-22** and are left in the file as a `/* … */` block; `ng-pattern` on the two inputs now states the same rule and says so under the field. Note **`ngTrim` defaults to `true` on `input[type=text]`**, so AngularJS trims what it writes into `vm.Modal` — a field holding only spaces arrives as `""` and fails `required` — and `[Required]` trims before testing on the server, which is why no `.trim()` call is needed on either side. (`input[type=password]` never trims, by design.) Uncommenting the block restores the typing filter; nothing depends on it, and while it is inactive nothing in the app calls jQuery at all (§ 7.4).
- The `$http` success argument is named `response`, so the body is `response.data`. OneMasaito names it `data` and writes `data.data`; the rename is cosmetic and was applied throughout this file on 2026-09-22 (`App.js` still uses OneMasaito's name).
- `UpdatePassword` / `ChangePassword` → admin reset. `UpdatePassword` also blanks the two password fields it copied from the grid row and re-arms the form before showing the modal.
- `UpdateStatus` / `SaveStatus` → the activate/deactivate modal; `vm.Status.ConfirmPassword` is the **admin's own** password (see § 5.2).
- `angular.copy(value)` instead of OneMasaito's `vm.Modal = value` — a cancelled edit no longer leaves half-typed values in the grid row.

⚠️ **DEVIATION** — `vm.DepartmentList` / `vm.ReportList`, `UpdateAccess` / `SaveAccess`, `UpdateReport` / `SaveReportAccess` are gone (no tables). The modal is closed only when the save succeeded (OneMasaito closes it either way, so a duplicate-username error would hide the form the user still needs).

### 9.4 `Views/Settings/UserAccounts.cshtml`

One page: the grid, and three modals. OneMasaito's page has five modals; *User Access* and *Report Access* have no tables here. Each modal's body + footer is wrapped in a **named** `<form name="…" ng-submit="…" autocomplete="off" novalidate>` (`AccountForm`/`Save()`, `ChangePasswordForm`/`ChangePassword()`, `StatusForm`/`SaveStatus()`) and its Save button is `type="submit"`, so Enter in any field saves — OneMasaito uses `type="button"` + `ng-click` and no form element.

⚠️ **DEVIATION (2026-09-24)** — the forms validate. The markup is the AngularJS 1.8 Forms guide's own pattern: `name` on every input, the rule as an attribute (`required`, `ng-required`, `ng-pattern`, `ng-minlength`, `ng-maxlength`), and one `<div class="invalid-feedback d-block">` per error key, all wrapped in a reveal gate of `Form.$submitted || Form.Field.$touched`. Naming a form publishes its `FormController` on the scope, which is how § 9.3's handlers reach `$scope.AccountForm`. Two mechanics to keep in mind:

- `d-block` is needed because Bootstrap 5 only reveals `.invalid-feedback` beside an `.is-invalid` sibling, and visibility here comes from `ng-show`.
- **Each message wrapper carries `class="field-feedback"`, and `Site.css` keeps that row in the layout while it is hidden** (`display: block !important; visibility: hidden`). Without it the form has a genuine input bug, found by testing on 2026-09-24: type into the last field, press **Save** once, and nothing happens. Blurring the field sets `$touched`, which reveals that field's message, which pushes the Save button ~25px down *between mousedown and mouseup* — and a browser only fires `click` when both land on the same element. The press is swallowed and the user has to click twice. Reserving the row removes the shift.
- `ng-maxlength`, not plain `maxlength` — the HTML attribute blocks typing and truncates a paste silently, which would make the 50-character rule invisible and impossible to mirror against `[StringLength(50)]`.

The red border needs no `ng-class` on any input: AngularJS puts `ng-invalid`/`ng-touched` on the controls and `ng-submitted` on the form, and one rule in `Site.css` (§ 7.3) keys off those classes. The header cell holds the "+" button and an *Active | Inactive | All* `btn-group-sm` (selected segment carries `active`, bound to `vm.StatusFilter`); the row loop is `filter: main.SearchBox | filter: StatusMatch`.

```cshtml
@{
    ViewBag.Title = "User Accounts";
}

<div ng-controller="accountsController as vm">

    <h1 class="h3 mb-2">Accounts</h1>

    <div class="card mb-4" ng-init="Init()">
        <div class="card-body">
            <div class="table-responsive">
                <table class="table table-striped" id="dataTable" width="100%" cellspacing="0">
                    <thead>
                        <tr>
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
                            <th>UserName</th>
                            <th>Name</th>
                            <th>Role</th>
                            <th>Status</th>
                        </tr>
                    </thead>
                    <tbody>
                        <tr ng-repeat="acc in vm.AccountList | filter: main.SearchBox | filter: StatusMatch">
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
                            <td>{{acc.Username}}</td>
                            <td>{{acc.FullName}}</td>
                            <td>{{acc.Role}}</td>
                            <td ng-class="{'text-success': acc.IsActive, 'text-danger': !acc.IsActive}">{{ acc.IsActive ? 'Active' : 'Inactive' }}</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <!--CRUD MODAL-->
    <div class="modal fade" id="AccountModal" tabindex="-1" role="dialog" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">{{vm.ModalHeader}} Account</h4>
                    <button type="button" class="btn-close" data-coreui-dismiss="modal" aria-label="Close"></button>
                </div>

                <form name="AccountForm" ng-submit="Save()" autocomplete="off" novalidate>
                <div class="modal-body">
                    <div class="mb-3">
                        <label class="form-label">Username</label>
                        <input type="text" class="form-control" name="Username" ng-model="vm.Modal.Username" ng-disabled="vm.ModalHeader === 'Edit'" required ng-maxlength="50" />
                        <div class="field-feedback" ng-show="AccountForm.$submitted || AccountForm.Username.$touched">
                            <div class="invalid-feedback d-block" ng-show="AccountForm.Username.$error.required">Please input Username</div>
                            <div class="invalid-feedback d-block" ng-show="AccountForm.Username.$error.maxlength">Username is too long (maximum 50)</div>
                        </div>
                    </div>

                    <div class="mb-3" ng-show="vm.ModalHeader === 'New'">
                        <label class="form-label">Password</label>
                        <input type="password" class="form-control" name="Password" ng-model="vm.Modal.Password" ng-required="vm.ModalHeader === 'New'" ng-minlength="6" ng-maxlength="255" />
                        <div class="field-feedback" ng-show="AccountForm.$submitted || AccountForm.Password.$touched">
                            <div class="invalid-feedback d-block" ng-show="AccountForm.Password.$error.required">Please input Password</div>
                            <div class="invalid-feedback d-block" ng-show="AccountForm.Password.$error.minlength">Password must be at least 6 characters</div>
                        </div>
                    </div>

                    <div class="mb-3">
                        <label class="form-label">First Name</label>
                        <input type="text" class="form-control" id="firstName" name="FirstName" ng-model="vm.Modal.FirstName" required ng-pattern="/^[a-zA-Z ]+$/" ng-maxlength="50" />
                        <div class="field-feedback" ng-show="AccountForm.$submitted || AccountForm.FirstName.$touched">
                            <div class="invalid-feedback d-block" ng-show="AccountForm.FirstName.$error.required">Please input First Name</div>
                            <div class="invalid-feedback d-block" ng-show="AccountForm.FirstName.$error.pattern">First Name must contain letters only</div>
                            <div class="invalid-feedback d-block" ng-show="AccountForm.FirstName.$error.maxlength">First Name is too long (maximum 50)</div>
                        </div>
                    </div>

                    <div class="mb-3">
                        <label class="form-label">Last Name</label>
                        <input type="text" class="form-control" id="lastName" name="LastName" ng-model="vm.Modal.LastName" required ng-pattern="/^[a-zA-Z ]+$/" ng-maxlength="50" />
                        <div class="field-feedback" ng-show="AccountForm.$submitted || AccountForm.LastName.$touched">
                            <div class="invalid-feedback d-block" ng-show="AccountForm.LastName.$error.required">Please input Last Name</div>
                            <div class="invalid-feedback d-block" ng-show="AccountForm.LastName.$error.pattern">Last Name must contain letters only</div>
                            <div class="invalid-feedback d-block" ng-show="AccountForm.LastName.$error.maxlength">Last Name is too long (maximum 50)</div>
                        </div>
                    </div>

                    <div class="mb-3">
                        <label class="form-label">Role</label>
                        <select class="form-select" name="Role" ng-model="vm.Modal.Role" ng-options="r for r in vm.RoleList" required></select>
                        <div class="field-feedback" ng-show="AccountForm.$submitted || AccountForm.Role.$touched">
                            <div class="invalid-feedback d-block" ng-show="AccountForm.Role.$error.required">Please select Role</div>
                        </div>
                    </div>
                </div>

                <div class="modal-footer">
                    <button type="submit" class="btn btn-primary">Save</button>
                </div>
                </form>
            </div>
        </div>
    </div>
    <!--END OF CRUD MODAL-->

    <!--CHANGE PASSWORD MODAL-->
    <div class="modal fade" id="ChangePasswordModal" tabindex="-1" role="dialog" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title">Reset Password - {{vm.Change.FullName}}</h4>
                    <button type="button" class="btn-close" data-coreui-dismiss="modal" aria-label="Close"></button>
                </div>

                <form name="ChangePasswordForm" ng-submit="ChangePassword()" autocomplete="off" novalidate>
                <div class="modal-body">
                    <div class="mb-3">
                        <label class="form-label">New Password</label>
                        <input type="password" class="form-control" name="NewPassword" ng-model="vm.Change.NewPassword" required ng-minlength="6" ng-maxlength="255" />
                        <div class="field-feedback" ng-show="ChangePasswordForm.$submitted || ChangePasswordForm.NewPassword.$touched">
                            <div class="invalid-feedback d-block" ng-show="ChangePasswordForm.NewPassword.$error.required">Please input New Password</div>
                            <div class="invalid-feedback d-block" ng-show="ChangePasswordForm.NewPassword.$error.minlength">Password must be at least 6 characters</div>
                        </div>
                    </div>

                    <div class="mb-3">
                        <label class="form-label">Confirm Password</label>
                        <input type="password" class="form-control" name="ConfirmPassword" ng-model="vm.Change.ConfirmPassword" required />
                        <div class="field-feedback" ng-show="ChangePasswordForm.$submitted || ChangePasswordForm.ConfirmPassword.$touched">
                            <div class="invalid-feedback d-block" ng-show="ChangePasswordForm.ConfirmPassword.$error.required">Please input Confirm Password</div>
                            <div class="invalid-feedback d-block" ng-show="vm.Change.ConfirmPassword && vm.Change.ConfirmPassword !== vm.Change.NewPassword">Password Not Match!</div>
                        </div>
                    </div>
                </div>

                <div class="modal-footer">
                    <button type="submit" class="btn btn-primary">Save</button>
                </div>
                </form>
            </div>
        </div>
    </div>
    <!--END OF CHANGE PASSWORD MODAL-->

    <!--UPDATE STATUS MODAL-->
    <div class="modal fade" id="UpdateStatusModal" tabindex="-1" role="dialog" aria-hidden="true">
        <div class="modal-dialog">
            <div class="modal-content">
                <div class="modal-header">
                    <h4 class="modal-title" ng-show="vm.Status.IsActive">Deactivate Account - {{vm.Status.FullName}}</h4>
                    <h4 class="modal-title" ng-show="!vm.Status.IsActive">Activate Account - {{vm.Status.FullName}}</h4>
                    <button type="button" class="btn-close" data-coreui-dismiss="modal" aria-label="Close"></button>
                </div>

                <form name="StatusForm" ng-submit="SaveStatus()" autocomplete="off" novalidate>
                <div class="modal-body">
                    <div class="mb-3">
                        <label class="form-label">Confirm Password</label>
                        <input type="password" class="form-control" name="StatusPassword" ng-model="vm.Status.ConfirmPassword" required />
                        <div class="form-text">Enter <b>your own</b> password to confirm this change.</div>
                        <div class="field-feedback" ng-show="StatusForm.$submitted || StatusForm.StatusPassword.$touched">
                            <div class="invalid-feedback d-block" ng-show="StatusForm.StatusPassword.$error.required">Please input Password to proceed</div>
                        </div>
                    </div>
                </div>

                <div class="modal-footer">
                    <button type="submit" class="btn btn-primary">Save</button>
                </div>
                </form>
            </div>
        </div>
    </div>
    <!--END OF UPDATE STATUS MODAL-->

</div>
```

OneMasaito parity: `ng-controller="accountsController as vm"`, `ng-init="Init()"` on the card, `ng-repeat="acc in vm.AccountList | filter: main.SearchBox"` (the header search box filters this grid), the "+" button in the header cell, the same row-action buttons with the same icons and `ng-click`s, the same three modals with the same ids and the same `ng-model` names (each wrapped in an `ng-submit` form here), the `#firstName` / `#lastName` ids the keypress filters attach to, the Username field disabled and the Password field hidden in Edit mode.

⚠️ **DEVIATION** — status filter (*Active | Inactive | All*, default *Active*) beside the "+" button, absent in OneMasaito; each modal's body + footer wrapped in `<form ng-submit>` with a `type="submit"` Save button so Enter saves (OneMasaito: `type="button"` + `ng-click`, no form element), and required checks written `!x` instead of `== "" || == null`; the "+" / Edit / Reset Password buttons are `btn-ghost-secondary` and Deactivate / Activate are `btn-ghost-danger` / `btn-ghost-success` instead of OneMasaito's solid `btn-success` / `btn-info` / `btn-warning` / `btn-danger`, with the `icon-text-white-50` span (and its `Site.css` rule) removed because a half-white icon is invisible on a transparent button; Department column and dropdown → Role column and `<select>` of the three constraint values; Modified By / Modified Date columns gone; User Access and Report Access buttons and modals gone; Font Awesome `fas fa-*` → CoreUI `cil-*`; `data-dismiss="modal"` → `data-coreui-dismiss="modal"`; `close` → `btn-close`; `form-group` → `mb-3`; `form-control` on `<select>` → `form-select`; the hidden `ID` inputs OneMasaito keeps in two modals are dropped (the id travels in `vm.Change.ID` / `vm.Status.ID` anyway); a help line under the status modal's password field says whose password it is.

✅ **VERIFY** — F5, then:
1. Log in as `admin` / `admin123` → you land on `/Home/Index` with the sidebar showing **Dashboard** and **Settings → User Account**.
2. Open **User Account** → the header cell shows a plain "+" icon then **Active | Inactive | All** with *Active* filled; the grid shows the `admin` row, Role `admin`, Status **Active** in green; the row's Edit / Reset / Deactivate icons have no background. Click **Inactive** → no rows; **All** → the `admin` row again; back to **Active**.
3. Click **+** → enter Username `jdoe`, Password `12345` → *Password must be at least 6 characters*. Password `123456`, First Name `John2` → the `2` cannot be typed (keypress filter); paste `John2` → *First Name must contain letters only*. First Name `John`, Last Name `Doe`, Role `user`, press **Enter** in the Last Name field → *Successfully Saved*, modal closes, grid shows the new row (Enter submits the modal's form, same as clicking Save).
4. DevTools → Network → right-click the `SaveNewAccount` request → *Copy as fetch* → paste in Console, change `"FirstName":"John"` to `"FirstName":"J0hn"` and `"Username"` to something new → run → response `{"message":"First Name and Last Name may contain letters and spaces only"}`. That is the server-side rule holding.
5. Type `doe` in the header search box → the grid filters to John Doe.

🔍 **GIT CHECKPOINT 9**

```bash
git add -A
git commit -m "feat: user accounts view and angular controllers"
```

---

## § 10 — End-to-end verification

Run every step in order on a fresh browser session. The **expected** column is the exact text you should see.

| # | Action | Expected |
|---|---|---|
| 1 | Open the site root. | `/Home/Login` renders the login card. |
| 2 | Log in with a wrong password, pressing **Enter** in the password field. | Growl: `Invalid Username or Password!!` (Enter submits the form via `ng-submit`; the Login button does the same). |
| 3 | Log in as `admin` / `admin123`. | Redirect to `/Home/Index`; header shows *System Administrator*; sidebar shows **Settings**. |
| 4 | Settings → User Account. | Grid lists `admin` (Active) and, if § 9 was verified, `jdoe`. |
| 5 | **+** → Username `jdoe` (again), Password `123456`, First `Jane`, Last `Doe`, Role `user`. | Growl: `Duplicate Username` (C# check in `SaveNewAccount`). |
| 6 | **+** → Username `jdoe2`, Password `123456`, First `John`, Last `Doe`, Role `user`. | Growl: `An account for this First Name and Last Name already exists.` (from `sp_InsertUserAccount`). |
| 7 | **+** → Username `msmith`, Password `123456`, First `Mary Ann`, Last `Smith`, Role `manager`. | `Successfully Saved`; new row, Role `manager`. |
| 8 | Edit `msmith` → change Role to `user`, Save. | `Successfully Saved`; Role column shows `user`. |
| 9 | Edit `msmith` → change First Name to `John`, Last Name to `Doe`, Save. | Growl: `An account for this First Name and Last Name already exists.` (from `sp_UpdateUser`). |
| 10 | Reset Password on `msmith` → New `abc`, Confirm `abc`. | **Under the New Password field**: `Password must be at least 6 characters`, and the input turns red. No growl — since 2026-09-24 format messages are inline and a growl always means the server spoke. |
| 11 | Reset Password on `msmith` → New `secret1`, Confirm `secret2`. | Under the Confirm field: `Password Not Match!` (inline, as above). |
| 12 | Reset Password on `msmith` → New `secret1`, Confirm `secret1`. | `Password Successfully Changed`. |
| 13 | Deactivate `msmith` → Confirm Password `wrong`. | Growl: `Wrong Password!` |
| 14 | Deactivate **`admin`** (your own row) → Confirm Password `admin123`. | Growl: `You cannot deactivate your own account.` (from `sp_DeleteUser`); admin stays Active. |
| 15 | Deactivate `msmith` → Confirm Password `admin123`. | `Account Status Successfully Changed`; `msmith` disappears from the *Active* view. Click **Inactive** → only `msmith`, **Inactive** in red, green Activate icon. Click **All** → every row. Type `smith` in the header search with *All* selected → `jsmith` and `msmith`; switch to *Inactive* → `msmith` only (search and filter combine). |
| 16 | Logout (header → Logout → confirm). | Redirect to `/Home/Login`. |
| 17 | Log in as `msmith` / `secret1`. | Growl: `Account is Locked. Contact MIS Department` |
| 18 | Log in as `admin`, click **Inactive** (or **All**) — the grid opens on *Active* and hides `msmith` — activate `msmith` (Confirm Password `admin123`), log out, log in as `msmith` / `secret1`. | Redirect to `/Home/Index`; header shows *Mary Ann Smith*; sidebar shows **no** Settings group. |
| 19 | As `msmith`, type `/Settings/UserAccounts` in the address bar. | The page renders (OneMasaito checks only that a user is logged in — recorded in `ARCHITECTURE.md` §7). |
| 20 | As `msmith`, header → Change Password → Current `wrong`, New `secret2`, Confirm `secret2`. | Growl: `Current password is incorrect.` (from `sp_UpdateUserPassword`). |
| 21 | Change Password → Current `secret1`, New `secret1`, Confirm `secret1`. | Growl: `New password must be different from the current password.` |
| 22 | Change Password → Current `secret1`, New `secret2`, Confirm `secret2`. | `Password Successfully Changed`; log out; `msmith` / `secret2` logs in. |
| 23 | Log out. Open `/Settings/UserAccounts` directly. | Redirect to `/Home/Login`. |
| 24 | Header → the contrast icon (the theme dropdown) → Dark. Reload. | Page stays dark (persisted in `localStorage`). The header icon does **not** change with the choice — see `COREUI_GUIDE.md` § 9. |
| 25 | While logged in, type `/Home/Login` in the address bar. | Redirect to `/Home/Index` — the GET `Login` action's `CurrentUser != null` check (§ 6.1). |
| 26 | **+** → First Name `  John  ` (leading and trailing spaces), everything else valid. | `Successfully Saved`; the row stores `John`. AngularJS trims it before `Save()` ever runs — `ngTrim` defaults to `true` on `input[type=text]` — so the padding never reaches `vm.Modal` (§ 9.3). A name of *only* spaces arrives as `""` and is rejected with `Please input First Name`. |
| 27 | **+** → press **Save** with every field empty. | No growl. Red borders, and under each field: `Please input Username`, `Please input Password`, `Please input First Name`, `Please input Last Name`. Nothing is posted (§ 9.3's `$invalid` guard). |
| 28 | Dismiss that modal, then press **+** again. | A clean form: no red borders, no messages. `ResetForm` calls `$setPristine()` (which clears `$submitted`) and `$setUntouched()`. |
| 29 | **+** → First Name of 51 letters. | Under the field: `First Name is too long (maximum 50)` — the mirror of `[StringLength(50)]` and of `nvarchar(50)` in the schema. |
| 30 | Header → Change Password → leave **Current Password** empty, fill the other two correctly, Save. | Under the field: `Please input Current Password`. New on 2026-09-24; this modal had no check on that field before. |
| 31 | Log out. On the login card press **Enter** with both fields empty. | `Please input Username` and `Please input Password` under the inputs; no request is sent. Before 2026-09-24 this posted and returned *Invalid Username or Password!!* |
| 32 | In DevTools: `fetch('/Settings/SaveNewAccount', { method: 'POST', headers: { 'Content-Type': 'application/json' }, body: '{"account":{"Username":""}}' }).then(r => r.json()).then(console.log)` | `{ message: "Invalid payload" }` — the DataAnnotations net answering a client that bypassed the form. It never explains what was wrong, by design. |
| 33 | **+** → First Name `Aaaa…` (51 letters) → **Save** → dismiss the modal → **+** again. | The field is empty. Before the 2026-09-24 fix the rejected text was still sitting there while `vm.Modal` was empty (§ 9.3). |
| 34 | **+** → type into First Name → click **Save** exactly **once**. | The form submits on that one click and the messages appear. Before the 2026-09-24 fix the first press was swallowed: blurring the field revealed its message and moved the button between mousedown and mouseup (§ 9.4). |

🔍 **GIT CHECKPOINT 10**

```bash
git add -A
git commit -m "docs: end-to-end verification complete"
```

The conversion is complete. `docs/ARCHITECTURE.md` is the reference from here on.

---

## Appendix A — Troubleshooting

| Symptom | Cause | Fix |
|---|---|---|
| Build error `Could not load file or assembly 'System.Web.Mvc, Version=X.Y.Z.0...'. The located assembly's manifest definition does not match the assembly reference.` (or the same for `EntityFramework`, `Microsoft.Web.Infrastructure`, `System.Web.Razor`/`WebPages`). | A pre-existing landmine, not something this guide's edits cause: `CoreUIDemo.csproj`'s `<Reference Include="…, Version=X.Y.Z.0…">` and `<HintPath>` can drift from what `packages.config` actually restores (this repo shipped with `System.Web.Mvc` declared at `5.2.9.0` and `Microsoft.Web.Infrastructure` at `2.0.0.0` while the restored packages were `5.2.7`/`1.0.0.0`) — MSBuild's reference resolver requires an *exact* version match and gives no build error for it, only a runtime one. | In `CoreUIDemo.csproj`, make every `<Reference Include="Name, Version=…">` match the assembly version actually in `packages\<PackageId>.<version>\lib\...\Name.dll` (open the dll's properties, or trust that the folder version usually *is* the assembly version for these packages), and make every `<HintPath>` point at that same folder. Also check `Views/Web.config` — its `<pages>`/`<host>` section for `System.Web.Mvc` has its own independent version string. |
| Runtime `FileLoadException` / mismatched-version binding error at first page hit, after the build error above is fixed. | `Web.config`'s `<runtime><assemblyBinding>` `bindingRedirect` still points `newVersion` at the old declared version. | Update the matching `<bindingRedirect oldVersion="…" newVersion="…">` in `Web.config` to the real assembly version too. |
| First hit of any page that renders `_Layout.cshtml` or `Login.cshtml` returns a 500 with `[NullReferenceException] Microsoft.Ajax.Utilities.JSParser.Parse… → System.Web.Optimization.JsMinify.Process → Bundle.GetBundleResponse`. | `~/bundles/scripts`'s `Transforms.Clear()` (§ 7.4) is missing, so `System.Web.Optimization` tries to minify `coreui.bundle.min.js` and the ~2013-era minifier cannot parse its modern JS syntax. | Add `scriptsBundle.Transforms.Clear();` before `bundles.Add(scriptsBundle);` for `~/bundles/scripts`, exactly as § 7.4 shows. |
| Page renders completely unstyled; no error anywhere. | A physical `Content/css/` folder exists, so IIS serves the folder instead of the `~/Content/css` bundle. | Delete `Content/css/` (and its csproj entries). § 1.4 / § 7.4. |
| Unstyled page, and DevTools shows 404 on `/Content/css`. | A CSS file listed in the bundle is not on disk or not included in the project — `StyleBundle` silently skips missing files, and if *all* are missing the bundle URL 404s. | Re-run § 7.1, then § 7.2 *Include In Project*. |
| Icons show as empty squares. | Fonts not next to `free.min.css`, or fonts not included in the project. | `Content/vendor/@coreui/icons/fonts/CoreUI-Icons-Free.*` must exist and be included. |
| Console: `coreui is not defined`. | `coreui.bundle.min.js` missing from `~/bundles/scripts`, or listed after `App.js`. | § 7.4 order: CoreUI before Angular; both in `<head>`. |
| Console: `Unknown provider: growlProvider` / `Module 'angular-growl' is not available`. | `angular-growl.min.js` not in `~/bundles/scripts`, or listed before `angular.min.js`. | § 7.1 copy, § 7.4 order. |
| Console: `[$injector:modulerr] Failed to instantiate module app … Module 'useraccount' is not available` (or `'growlConfig'`). | `UserAccounts.js`, `Login.js` or `GrowlConfig.js` missing from `~/bundles/angular` or not included in the project. | § 7.4, § 9. |
| Growls appear as a ~47 px box with the text spilling out beside it. | The growl shim in `Site.css` is missing — CoreUI's `.icon` rule is collapsing the notification. | § 7.3. |
| Console on every page: `TypeError: Cannot read properties of null (reading 'querySelector')` from `color-modes.js`. | The theme dropdown (`[data-coreui-theme-value]` buttons) was removed from `_Layout`. | Put it back, or remove `color-modes.js` too. § 8.2. |
| Login page flashes, then nothing; Network shows `POST /Home/Login` → 200 with `errorMessage: ""`, but `/Home/Index` redirects back to Login. | Cookie not set — usually the site runs on `http://localhost:PORT` but you are browsing a different host name, or third-party cookie blocking. | Browse exactly the URL IIS Express opened. |
| Every login says `Invalid Username or Password!!`. | Seed not run, or the connection string points at a different server/database. | § 2.2 / § 2.3; confirm with `SELECT * FROM loginDemo.dbo.vw_Users` on the same instance. |
| `Login failed for user 'sa'` / `Cannot open database "loginDemo"`. | Connection string credentials or instance name. | § 2.3. |
| Build: `'loginDemoEntities' does not contain a definition for 'sp_InsertUserAccount'` (or another sproc). | Function import missing from the EDMX. | § 3.2. |
| Runtime: `Procedure or function 'sp_InsertUserAccount' expects parameter '@ROLE'` or `has too many arguments specified`. | Function import generated from an older script version. | § 3.2 — delete and re-import the sproc. |
| Runtime: `The data reader is incompatible` / `A member of the type … does not have a corresponding column` on a sproc call. | Function import's *Returns* is set to a scalar/entity instead of **None**. | § 3.2 step 3. |
| Growl shows `An error occurred while executing the command definition. See the inner exception for details.` | A service catch uses `error.Message` instead of `error.GetBaseException().Message`. | § 5.2. |
| Growl: `You cannot deactivate your own account.` | Expected — `sp_DeleteUser` guard. | Use a different admin account to deactivate this one. |
| Modal opens but never closes after a successful save. | `HideModal` called with a wrong id, or the modal element is outside the `ng-show="main.ItemLoad"` wrapper. | Ids must match § 9.4 exactly. |
| Modal closes but a dark backdrop stays on the page. | The page navigated (`window.location`) while a modal was open. | Call `HideModal(...)` before redirecting (as `Logout()` does). |
| Sidebar group *Settings* never appears for admin. | `main.CurrentUser.Role` is not `'admin'` — check `/Home/GetCurrentUser` output; the DB value is case-sensitive in the Angular comparison. | Store roles in lowercase (the `CHK_UserRole` constraint only allows lowercase anyway). |
| `Server Error in '/' Application` with `Padding is invalid and cannot be removed` after changing `Web.config`. | The `.ASPXAUTH` cookie was encrypted under a different machine key (config change, different project). | Delete the site's cookies in the browser. |
| The keypress filter on First/Last Name does not work, but the regex message appears on Save. | Expected since 2026-09-22 — both `keypress` blocks in `UserAccounts.js` are commented out and `Save()` validates instead. If you uncommented them and they still do nothing: jQuery not loaded before `UserAccounts.js`, or the inputs' `id` attributes changed. | § 9.3; then § 7.4 order and the ids `firstName` / `lastName`. |
