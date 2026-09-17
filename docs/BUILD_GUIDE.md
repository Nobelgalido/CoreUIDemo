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
| `Content/Site.css` | **Rewrite** (§ 7) | Loader spinner CSS |
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

Leave `<connectionStrings>`, `<runtime>`, `<system.codedom>` and `<appSettings>` as they are (the connection string is revisited in § 2.3).

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

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CoreUIDemo.Models
{
    public class UserModel
    {
        public long ID { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public string FullName
        {
            get
            {
                return FirstName + " " + LastName;
            }
        }

        public string Role { get; set; }
        public bool IsActive { get; set; }
    }

    public class ChangePasswordModel
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
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

Validation rules (the only thing added to OneMasaito, at the user's request): First Name and Last Name must match `^[a-zA-Z ]+$` (letters and spaces — the space matches OneMasaito's existing keypress filter on those inputs); passwords must be at least 6 characters. Username has no format rule beyond required + not duplicate, same as OneMasaito. The same checks run in the browser (§ 9) so the user gets immediate feedback; these server-side copies are the ones that actually hold.

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Text.RegularExpressions;
using CoreUIDemo.Models;
using CoreUIDemo.Helpers;

namespace CoreUIDemo.Services
{
    public class UserService
    {
        private const string NamePattern = "^[a-zA-Z ]+$";
        private const int PasswordMinLength = 6;
        private const string NameMessage = "First Name and Last Name may contain letters and spaces only";
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
                if (_pass.NewPassword == null || _pass.NewPassword.Length < PasswordMinLength)
                {
                    message = PasswordMessage;
                    return;
                }

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
                if (!Regex.IsMatch(_account.FirstName ?? "", NamePattern) || !Regex.IsMatch(_account.LastName ?? "", NamePattern))
                {
                    message = NameMessage;
                    return false;
                }

                if (_account.Password == null || _account.Password.Length < PasswordMinLength)
                {
                    message = PasswordMessage;
                    return false;
                }

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
                if (!Regex.IsMatch(_account.FirstName ?? "", NamePattern) || !Regex.IsMatch(_account.LastName ?? "", NamePattern))
                {
                    message = NameMessage;
                    return false;
                }

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

namespace CoreUIDemo.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public JsonResult Login(string username, string password)
        {
            string serverResponse = "";

            UserModel user = UserService.ValidateUserLogin(username, password, out serverResponse);

            if (user != null)
            {
                AccountService.LoginToSession(user);
            }

            return Json(new { errorMessage = serverResponse });
        }

        [HttpPost]
        public JsonResult Logout()
        {
            string serverResponse = "";

            AccountService.LogoutFromSession(out serverResponse);

            return Json(serverResponse);
        }

        [HttpPost]
        public JsonResult ChangePassword(ChangePasswordModel password)
        {
            var serverResponse = "";

            if (password != null)
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
                return RedirectToRoute(new { controller = "Home", action = "Login", id = UrlParameter.Optional });
            else
                return View();
        }
    }
}
```

How model binding works for the JSON actions (this is why the Angular payload shapes in § 9 are what they are): AngularJS posts `application/json`; MVC 5's `JsonValueProviderFactory` flattens the body so that `{ username: "a", password: "b" }` binds the two string parameters of `Login`, and `{ password: { CurrentPassword: …, NewPassword: …, ConfirmPassword: … } }` binds the `ChangePasswordModel password` parameter by prefix. `FullName` on `UserModel` is read-only, so the binder ignores it when the browser sends it back.

⚠️ **DEVIATION**
- `GetCurrentUser` returns only `{ obj }`. OneMasaito also stuffs two unrelated dropdown lists from `IFCAService` into this response.
- `JsonRequestBehavior.AllowGet` is added so you can hit `/Home/GetCurrentUser` in the address bar while verifying § 8. OneMasaito's version has no `AllowGet` and is only ever POSTed to by `App.js`; the Angular code here also POSTs. Remove `AllowGet` if you prefer the stricter mirror — nothing else depends on it.

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
        // GET: Settings
        public ActionResult UserAccounts()
        {
            var user = UniversalHelpers.CurrentUser;

            if (user == null)
                return RedirectToRoute(new { controller = "Home", action = "Login", id = UrlParameter.Optional });
            else
                return View();
        }

        [HttpPost]
        public JsonResult GetAccounts()
        {
            var accountList = UserService.GetAllAccount();

            return Json(new { accountList = accountList });
        }

        [HttpPost]
        public JsonResult SaveNewAccount(UserModel account, string role)
        {
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
                return Json(new { message = "Saved" });
            else
                return Json(new { message = string.IsNullOrEmpty(message) ? "Error on Saving" : message });
        }

        [HttpPost]
        public JsonResult AdminChangePassword(long account, string password)
        {
            var serverResponse = "";

            if (password != null)
                UserService.AdminChangePassword(account, password, out serverResponse);

            return Json(new { errorMessage = serverResponse });
        }

        [HttpPost]
        public JsonResult UpdateStatus(long account, string password)
        {
            var serverResponse = "";

            if (password != null)
                UserService.AdminUpdateStatus(account, password, out serverResponse);

            return Json(new { errorMessage = serverResponse });
        }
    }
}
```

`SaveNewAccount` keeps OneMasaito's branching: `ID == 0` means create (duplicate-username check in C#, then the sproc, which checks again), otherwise update. The `role` parameter arrives beside the model — `{ account: {...}, role: "admin" }` — exactly where OneMasaito's `long department` used to be.

`UpdateStatus`'s `password` is the acting admin's password, not the target user's — see § 5.2.

⚠️ **DEVIATION**
- Dropped actions: `UpdateUserAccess`, `GetUserReportAccess`, `UpdateReportAccess` (no access/report tables) and `BuildingPermitDropdown` (an AppSuite endpoint that sat in OneMasaito's Settings controller by accident).
- `GetAccounts` returns only `accountList` (OneMasaito adds `departmentList` and `reportList`).
- `long department` → `string role`.
- When a save fails, the sproc/validation message is returned instead of the fixed "Error on Saving" (which is still the fallback when there is no message).

✅ **VERIFY** — `Build → Rebuild Solution` reports **0 errors**. (Views are still the old ones or missing; that is a runtime concern handled in § 8–§ 9. Do not run the app yet.)

🔍 **GIT CHECKPOINT 6**

```bash
git add -A
git commit -m "feat: controllers"
```

---
