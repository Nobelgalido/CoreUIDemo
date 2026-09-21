# Global growl configuration: Design Spec

Date: 2026-09-21
Status: implemented and verified 2026-09-21

## 1. Goal

Stop repeating `{ ttl: N }` (and an inconsistent `title`) on every `growl.error` /
`growl.success` call. Set the time-to-live once, per severity, in an Angular `config` block
that every page picks up, and reduce all 25 call sites to `growl.error("…")` /
`growl.success("…")`.

This is a **deliberate deviation** from OneMasaito, which sets `ttl` inline on all 163 of its
growl calls and has no `growlProvider` configuration. Everything else about the notification
layer (angular-growl-v2 0.7.3, the `<div growl class="fading">` container, the call sites'
messages and placement) is unchanged.

## 2. Decisions

| Decision | Choice | Why |
|---|---|---|
| TTL scheme | per severity: `success` 3000 ms, `error` 5000 ms, `warning` 5000 ms, `info` 3000 ms | Confirmations vanish quickly; validation and stored-procedure errors linger long enough to read. Closest to the existing mix (2–4 s success, 2–5 s error). `warning`/`info` are unused today but set so a future call gets a sane default. |
| Error title | none — drop every `title:` | The red/green box already conveys severity; today's titles are inconsistent (`"Error!"`, `"Error"`, absent). angular-growl only renders its `<h4 class="growl-title">` when a title is passed, so omitting it removes the header cleanly. |
| Where the config lives | a new module `growlConfig` in `App/GrowlConfig.js` | The login page bootstraps `ng-app="login"`, whose module depends only on `angular-growl` — not on `app` — and `app` already depends on `login`, so neither existing module can carry the config for both pages without duplication or a cycle. |

## 3. Design

### 3.1 New file `App/GrowlConfig.js`

```javascript
angular.module("growlConfig", ["angular-growl"])
    .config(['growlProvider', function (growlProvider) {
        growlProvider.globalTimeToLive({ success: 3000, error: 5000, warning: 5000, info: 3000 });
    }]);
```

Included in the project (`<Content Include="App\GrowlConfig.js" />`) and in
`~/bundles/angular` **before** `App.js`.

### 3.2 Module dependencies

- `App/Controller/Login.js`: `angular.module("login", ["angular-growl", "growlConfig"])`
- `App/App.js`: `angular.module('app', ["angular-growl", "growlConfig", "login", "useraccount"])`
- `App/Controller/UserAccounts.js`: unchanged (`["app"]` already transitively includes it).

### 3.3 Call sites

Every `growl.success(msg, { … })` becomes `growl.success(msg)`; every
`growl.error(msg, { … })` becomes `growl.error(msg)`. No message text changes. Files:
`App/App.js` (7 calls), `App/Controller/Login.js` (1), `App/Controller/UserAccounts.js` (17).

### 3.4 Runtime behaviour

angular-growl resolves a message's TTL as `options.ttl || globalTtl[severity]`, so with no
inline `ttl` every message uses the per-severity global. A single call may still override by
passing `{ ttl: N }` — nothing forbids it; it is simply no longer needed.

### 3.5 Restore growl's original look (added during verification)

Testing showed every growl rendering as a ~47 px box with its text spilling outside. Two
CoreUI / Bootstrap 5 collisions cause it, neither present in OneMasaito's Bootstrap 4 stack:

1. angular-growl adds an `icon` class to each notification (for its severity image), and
   CoreUI's `style.css` defines `.icon` as a 1rem inline-block for CoreUI Icons.
2. Bootstrap 5 removed `.close`; growl's × and countdown buttons still use it.

Fix: a CSS shim appended to `Content/Site.css`, scoped to `.growl-container > .growl-item`,
that (a) sets the item back to `display: block; width/height: auto; font-size: inherit;
color: var(--cui-alert-color)` and (b) re-creates Bootstrap 4's `.close` (float right,
1.5rem bold, transparent, opacity .5 / .75 on hover). Growl's own icons stay on — they are
part of the original look — so `globalDisableIcons` is **not** used. CoreUI keeps alerts
pastel in dark mode, so no dark-mode override is needed.

## 4. Out of scope

- Replacing angular-growl with CoreUI Toasts (discussed separately; not decided).
- `globalPosition`, `globalDisableCountDown`, `globalDisableIcons`, or any other provider
  setting — defaults stay.
- Any server-side change.

## 5. Documentation to update

- `docs/BUILD_GUIDE.md` § 7.3 (`Site.css` listing + shim deviation note), § 7.4 (bundle
  listing gains `GrowlConfig.js`), § 9 (deviation note, new § 9.0 for `GrowlConfig.js`;
  `App.js`, `Login.js`, `UserAccounts.js` listings updated to match), Appendix A (two rows).
- `docs/ARCHITECTURE.md` § 2 folder map (new row, `Site.css` row), § 6.2 *Changed*, § 6.3
  module topology (`growlConfig` shared by `login` and `app`).
- `README.md` — no change needed (no setup step changes).

## 6. Verification

1. `MSBuild CoreUIDemo.csproj` succeeds.
2. Browser console on `/Home/Login` and `/Settings/UserAccounts`: no `[$injector:modulerr]`.
3. From BUILD_GUIDE § 10, re-run the growl-bearing steps 2, 5, 6, 10–14, 17, 20–21: every
   expected message still appears, with **no** title line.
4. Timing: an error growl stays ≈ 5 s, a success growl ≈ 3 s (measured by timestamping the
   growl element's insertion and removal).
5. Look: a growl is the full 400 px container width, icon at left, × and countdown at the
   top-right, in both light and dark themes.

Result (2026-09-21, Playwright against a fresh build): all five pass — 11 growls checked,
errors 5001–5015 ms, successes 3004–3015 ms, no title element, item 400 × 46 px.
