# 07 · Error pages

The dist's `error-pages/` folder holds two pages, `404.html` and `500.html`, that share one structure: a big number, a one-line heading, a grey sentence and a search box, centred on the same tinted full-height wrapper as the authentication pages. This chapter shows that markup once, then maps it onto what CoreUIDemo actually has for errors today — a global `HandleErrorAttribute` and the MVC template's plain `Views/Shared/Error.cshtml` — and describes the **optional** 404 route OneMasaito does not have. Nothing in this chapter is added to the repo by the handbook; the views and `Web.config` edits below are what you would write if the user asks for them.

What exists in CoreUIDemo today, verified against the source: `App_Start/FilterConfig.cs` registers `new HandleErrorAttribute()` globally (wired from `Global.asax.cs`); `Views/Shared/Error.cshtml` is the unmodified MVC template page (`<hgroup>` with "Error." / "An error occurred while processing your request.", no CoreUI, kept as-is per [`../BUILD_GUIDE.md` § 1.1](../BUILD_GUIDE.md#11-fate-of-every-existing-file)); `Web.config` has **no** `<customErrors>` element and **no** `<system.webServer>` section; `Controllers/HomeController.cs` has no `NotFound` action.

- [404 and 500 in the dist](#404-and-500-in-the-dist)
- [500 → Views/Shared/Error.cshtml](#500--viewssharederrorcshtml)
- [404 → Views/Home/NotFound.cshtml (optional)](#404--viewshomenotfoundcshtml-optional)
- [Gotchas](#gotchas)

## 404 and 500 in the dist

**1. What the dist page shows.** A `404` (or `500`) in display type on the left, a heading and a muted sentence beside it, and a search `input-group` under them. No sidebar, no header, no card. Dist: `error-pages/404.html` and `error-pages/500.html` — the two files differ only in the number and the two lines of text (`Oops! You're lost.` / `The page you are looking for was not found.` versus `Houston, we have a problem!` / `The page you are looking for is temporarily unavailable.`).

**2. Essential markup.** The outer wrapper is the one every `Layout = null` page in this project uses — see [02 · The shared frame](./02-authentication.md#the-shared-frame); the inside is a plain grid column, not a card:

**`error-pages/404.html`** — body, search box elided
```html
<div class="bg-body-tertiary min-vh-100 d-flex flex-row align-items-center">   <!-- same wrapper as authentication/ -->
  <div class="container">
    <div class="row justify-content-center">
      <div class="col-md-6">                                                     <!-- half width from md up, full below -->
        <div class="clearfix">                                                   <!-- contains the floated number -->
          <h1 class="float-start display-3 me-4">404</h1>                        <!-- big number, text wraps beside it -->
          <h4 class="pt-3">Oops! You're lost.</h4>                               <!-- pt-3 aligns the heading with the number's cap height -->
          <p class="text-body-secondary">The page you are looking for was not found.</p>
        </div>
        <div class="input-group">…inline SVG magnifier + form-control + btn btn-info "Search"…</div>
      </div>
    </div>
  </div>
</div>
```

The `<head>` of both pages is the auth-page head: `css/style.css`, simplebar CSS, `js/config.js`, `js/color-modes.js`; the body ends with `coreui.bundle.min.js`, `simplebar.min.js` and the scroll snippet from `index.html` that toggles `shadow-sm` on the header — all inert here, since the page has no `header.header`, no `[data-coreui-theme-value]` button (so `color-modes.js` throws, exactly as on the auth pages) and no component that needs JavaScript.

**3. Behaviour.** CSS only.

**4. In CoreUIDemo.** Drop the search `input-group`: it is decorative in the dist (a `type="button"` that submits nothing), and an error page runs with `Layout = null`, so there is no `mainController`, no `$http`, and nothing to bind a search box to. What remains — wrapper, `clearfix`, `h1.float-start.display-3.me-4`, `h4.pt-3`, `p.text-body-secondary` — is used verbatim in the two views below, plus a `btn btn-primary` link back to the dashboard so the reader has somewhere to go.

**5. Gotchas.** `float-start` + `clearfix` is the dist's choice, not a flex row; if you replace it with `d-flex`, drop the `pt-3` on the `h4` or the heading sits too low. Nothing else.

## 500 → Views/Shared/Error.cshtml

**What `HandleErrorAttribute` does today.** `FilterConfig.RegisterGlobalFilters` adds `HandleErrorAttribute` to every action. When an action throws, the filter renders the view named `Error` (found at `Views/Shared/Error.cshtml`) with a `HandleErrorInfo` model, sets the status to 500 and sets `Response.TrySkipIisCustomErrors = true` so IIS does not swap in its own page — **but only when custom errors are enabled for the request**. That is governed by `<customErrors>` in `Web.config`, which CoreUIDemo does not declare, so ASP.NET's default `RemoteOnly` applies: pressing F5 on the dev machine still shows the yellow diagnostic screen; a browser on another machine gets `Error.cshtml`. Two things the filter never sees: exceptions raised outside an action (routing, view compilation of the layout, `Application_Start`), and 404s, because a URL that matches no route throws nothing inside MVC. The `FormsAuthentication.Decrypt` case noted in [`../BUILD_GUIDE.md` § 4.3](../BUILD_GUIDE.md#43-helpersuniversalhelperscs) is one exception this page does catch.

**The current view.** `Views/Shared/Error.cshtml` is the file the MVC 5 project template generates — a bare `<!DOCTYPE html>` page with a viewport meta, `<title>Error</title>` and an `<hgroup>`. It references no stylesheet at all, so it renders as unstyled browser default text, and it is listed as **Keep** in [`../BUILD_GUIDE.md` § 1.1](../BUILD_GUIDE.md#11-fate-of-every-existing-file) because OneMasaito's is identical.

**Rebuilt with the dist's 500 markup.** Same file, same name (the filter looks for `Error` by convention), same `Layout = null` head as `Views/Home/Login.cshtml` minus the script bundles — an error page has no AngularJS module, so only `~/Content/css` is rendered (the bundle is described in [`../COREUI_GUIDE.md` § 4](../COREUI_GUIDE.md#4-bundles)). The `<html>` is pinned to `data-coreui-theme="light"` for the same reason the login page is; to follow the user's saved theme instead, paste the one-liner from [`../COREUI_GUIDE.md` § 9](../COREUI_GUIDE.md#9-dark-mode) ("Login page follows the saved theme") as the first thing in `<head>` and remove the attribute.

**`Views/Shared/Error.cshtml`** — complete replacement
```html
@model System.Web.Mvc.HandleErrorInfo
@{
    Layout = null;
}

<!DOCTYPE html>

<html data-coreui-theme="light">
<head>
    <meta http-equiv="content-type" content="text/html; charset=UTF-8" />
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0, shrink-to-fit=no" />
    <link href="~/Src/Image/masaito-mark-light-gradient.svg" rel="icon" type="image/svg+xml" />

    @Styles.Render("~/Content/css")

    <title>Error</title>
</head>
<body class="bg-body-tertiary min-vh-100 d-flex flex-row align-items-center">
    <div class="container">
        <div class="row justify-content-center">
            <div class="col-md-6">
                <div class="clearfix">
                    <h1 class="float-start display-3 me-4">500</h1>
                    <h4 class="pt-3">Houston, we have a problem!</h4>
                    <p class="text-body-secondary">The page you are looking for is temporarily unavailable.</p>
                </div>
                <a class="btn btn-primary mt-3" href="~/Home/Index">Back to Dashboard</a>
            </div>
        </div>
    </div>
</body>
</html>
```

The `@model` line is there because that is what the filter passes; the view deliberately does not print `Model.Exception.Message` — the exception text belongs in the yellow screen and the server log, not in a page a remote user sees. `Model` is `null` when the view is reached any other way (for example a `defaultRedirect` to it), so leave it unused rather than dereferencing it. The "Back to Dashboard" link goes to `/Home/Index`, whose action redirects to `Login` when there is no session, so the link is safe on a page that may have been reached after the session died.

To see it locally, add `<customErrors mode="On" />` inside `<system.web>` (that is the whole change; no `defaultRedirect` is needed because the filter renders the view directly and keeps the 500 status), or leave the default and test from another machine. Editing `Web.config` or a view is a change to a file the repo's docs-sync rule covers (`CLAUDE.md`): `../BUILD_GUIDE.md` § 1.3 and § 1.1 must be updated in the same turn.

## 404 → Views/Home/NotFound.cshtml (optional)

**OneMasaito has no 404 page and neither does CoreUIDemo** — an unknown URL today produces the IIS/ASP.NET default 404 response. Everything in this section is an optional addition, written so it can be dropped in if the user asks for it; it is not part of the mirror.

**What does not work.** `<customErrors>` can only redirect to a URL, and `.cshtml` files are never served directly (`Views/Web.config` blocks them), so `<error statusCode="404" redirect="~/Views/Shared/NotFound.cshtml" />` yields a second 404. The redirect target has to be a routed action.

**The route-based option** is three pieces: a `Web.config` element, one action, one view.

**`Web.config`** — inside `<system.web>`, which currently has only `<compilation>` and `<httpRuntime>`
```xml
<customErrors mode="On">
  <error statusCode="404" redirect="~/Home/NotFound" />
</customErrors>
```

`mode="On"` also turns on the `Error.cshtml` rendering from the previous section for local requests; use `mode="RemoteOnly"` to keep the yellow screen on the dev machine. ASP.NET answers the bad URL with a `302` to `/Home/NotFound?aspxerrorpath=/the/bad/url`, so the address bar changes — that is the documented behaviour of the default `redirectMode="ResponseRedirect"`, and the alternative (`ResponseRewrite`) transfers to a physical file, which an MVC route is not.

**`Controllers/HomeController.cs`** — the only C# in this chapter
```csharp
public ActionResult NotFound() { Response.StatusCode = 404; return View(); }
```

Setting `Response.StatusCode` is what turns the redirected-to page back into a real 404 for the browser and for crawlers; without it the page returns 200. This is a GET view action with no `UniversalHelpers.CurrentUser` check on purpose — the page must render whether or not there is a session. `return View()` resolves to `Views/Home/NotFound.cshtml` (or `Views/Shared/NotFound.cshtml` if you would rather keep both error views together; MVC checks `Views/Home/` first, then `Views/Shared/`).

**`Views/Home/NotFound.cshtml`** — complete; `<head>` identical to `Error.cshtml` above except the title
```html
@{
    Layout = null;
}

<!DOCTYPE html>

<html data-coreui-theme="light">
<head>
    <meta http-equiv="content-type" content="text/html; charset=UTF-8" />
    <meta charset="utf-8" />
    <meta http-equiv="X-UA-Compatible" content="IE=edge" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0, shrink-to-fit=no" />
    <link href="~/Src/Image/masaito-mark-light-gradient.svg" rel="icon" type="image/svg+xml" />

    @Styles.Render("~/Content/css")

    <title>Page not found</title>
</head>
<body class="bg-body-tertiary min-vh-100 d-flex flex-row align-items-center">
    <div class="container">
        <div class="row justify-content-center">
            <div class="col-md-6">
                <div class="clearfix">
                    <h1 class="float-start display-3 me-4">404</h1>
                    <h4 class="pt-3">Oops! You're lost.</h4>
                    <p class="text-body-secondary">The page you are looking for was not found.</p>
                </div>
                <a class="btn btn-primary mt-3" href="~/Home/Index">Back to Dashboard</a>
            </div>
        </div>
    </div>
</body>
</html>
```

Adding the action changes `Controllers/HomeController.cs`, whose complete listing lives in [`../BUILD_GUIDE.md` § 6.1](../BUILD_GUIDE.md#61-controllershomecontrollercs), and the endpoint table in `../ARCHITECTURE.md` — both must be updated in the same turn under the repo's docs-sync rule (`CLAUDE.md`).

## Gotchas

- **`<customErrors>` sees only the ASP.NET pipeline.** A request for a file that does not exist under `Content/`, `Scripts/` or `Src/` — a typo in an `<img src>`, a stale bundle name — is answered by IIS's static-file handler and never reaches ASP.NET, so it gets IIS's own 404 page. To route those through the same view, add `<httpErrors>` under `<system.webServer>` (a section `Web.config` does not have today; create it as a sibling of `<system.web>`):

  **`Web.config`** — new section, sibling of `<system.web>`
  ```xml
  <system.webServer>
    <httpErrors errorMode="Custom">
      <remove statusCode="404" />
      <error statusCode="404" path="/Home/NotFound" responseMode="ExecuteURL" />
    </httpErrors>
  </system.webServer>
  ```

  `errorMode="Custom"` applies it locally too (the IIS default, `DetailedLocalOnly`, keeps the detailed page on the dev machine); `ExecuteURL` runs the MVC action inside the same request, so unlike the `<customErrors>` redirect the address bar does not change. Leave `existingResponse` at its default (`Auto`): `HandleErrorAttribute` sets `TrySkipIisCustomErrors`, so the 500 page from `Error.cshtml` passes through untouched, while the `NotFound()` action's 404 (which does not set the flag) is handed to the same `/Home/NotFound` URL either way. Do not set `existingResponse="Replace"` — that ignores the flag and would overwrite the 500 page with IIS's.
- **`Layout = null` pages style themselves.** `_Layout.cshtml` is not involved, so a view that forgets `@Styles.Render("~/Content/css")` renders as unstyled text — which is exactly what the template's original `Error.cshtml` does. The bundle order and why `~/Content/css` must not also be a physical folder are in [`../COREUI_GUIDE.md` § 4](../COREUI_GUIDE.md#4-bundles).
- **No script bundles on these pages.** Neither view above renders `~/bundles/scripts` or `~/bundles/angular`. Rendering `~/bundles/angular` without an `ng-app` is harmless but pointless; rendering it *with* `ng-app="app"` would make `mainController` call `/Home/GetCurrentUser` from an error page, which is the situation the login page avoids for the same reason ([02 · The shared frame](./02-authentication.md#the-shared-frame)).
- **`HandleErrorAttribute` and `customErrors` are separate switches.** The filter is always registered; it simply declines to act when custom errors are off for the request. If `Error.cshtml` never appears, check `<customErrors>` (or the lack of it) before checking `FilterConfig.cs`.
- **Theme on error pages.** Both views pin `data-coreui-theme="light"`. If the shell uses the theme dropdown from [08 · Colour modes](./08-customizing.md#colour-modes), a dark-mode user gets a light error page unless you add the [`../COREUI_GUIDE.md` § 9](../COREUI_GUIDE.md#9-dark-mode) one-liner to both views.
