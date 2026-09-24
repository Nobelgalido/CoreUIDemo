# DataAnnotations as a safety net, AngularJS as the voice — design

**Date:** 2026-09-24
**Status:** approved, implemented the same day
**Supersedes:** the validation arrangement described in `2026-09-17-onemasaito-mirror-design.md` § "Validation added in OneMasaito's style"

## Problem

Every validation rule in the app is written twice, and both copies are in the wrong place.

The browser copy is a sequential `if` chain in `App/Controller/UserAccounts.js` and `App/App.js` that growls one message at a time; the field that failed gets no marking, and the user has to map the message back to an input. The server copy is `Regex.IsMatch` plus `const NameMessage` buried inside static service methods, where it sits between the business logic and the database call.

The rules themselves are also uneven. Four forms validate; the login form does not validate at all. The self-service Change Password modal merges "required" and "too short" into one message, while the admin reset modal splits them. Nothing anywhere stops a 51-character name from reaching `nvarchar(50)` and coming back as a SQL truncation exception rendered through `GetBaseException().Message`.

## The contract

Three tiers, each with exactly one job.

| Tier | Job | Speaks to the user? |
|---|---|---|
| **AngularJS form validation** (views) | presence and format, per field | **Yes** — every message a user can provoke by typing |
| **DataAnnotations** (models) | the same presence/format rules, `ErrorMessage` deliberately omitted | **No** — an invalid `ModelState` means a client was bypassed |
| **Service + stored procedures** | state rules that need the database | **Yes** — own messages, unchanged |

The middle tier never produces user-facing text. When it catches something, the action returns `"Invalid payload"` in whichever envelope that endpoint already uses — `{ message }` for the save family, `{ errorMessage }` for the auth/password family. That string already existed (added 2026-09-22 in `SaveNewAccount`); no second one is invented.

**Why annotations are the faithful choice.** OneMasaito has no DataAnnotations and no server-side format validation at all; `BUILD_GUIDE.md` § 5.2 records the name regex and the password-length rule as "the only thing added to OneMasaito, at the user's request". The rules are therefore *already* a deviation. This design does not add one — it moves an existing deviation from imperative code inside a static service onto metadata on a class OneMasaito already has, which is the smaller footprint of the two.

## Rule inventory

Every rule in the application, and who owns each.

### Account modal (`#AccountModal`)

| Rule | View | Model | Message |
|---|---|---|---|
| Username present | `required` | `[Required]` | *Please input Username* |
| Username ≤ 50 | `ng-maxlength="50"` | `[StringLength(50)]` | *Username is too long (maximum 50)* |
| Password present, create only | `ng-required="vm.ModalHeader === 'New'"` | `IValidatableObject` (`ID == 0`) | *Please input Password* |
| Password ≥ 6 | `ng-minlength="6"` | `[StringLength(255, MinimumLength = 6)]` | *Password must be at least 6 characters* |
| First / Last present | `required` | `[Required]` | *Please input First Name* / *Last Name* |
| First / Last letters and spaces | `ng-pattern="/^[a-zA-Z ]+$/"` | `[RegularExpression("^[a-zA-Z ]+$")]` | *… must contain letters only* |
| First / Last ≤ 50 | `ng-maxlength="50"` | `[StringLength(50)]` | *… is too long (maximum 50)* |
| Role chosen | `required` | `[Required]`, `[RegularExpression("^(user\|manager\|admin)$")]` | *Please select Role* |

The role pattern mirrors `CHK_UserRole` in `Database/script.sql`, which allows exactly `user`, `manager`, `admin` — the same three values as `vm.RoleList`.

### Admin reset modal (`#ChangePasswordModal`)

| Rule | View | Model | Message |
|---|---|---|---|
| New Password present | `required` | — (primitive parameter) | *Please input New Password* |
| New ≥ 6 | `ng-minlength="6"` | — | *Password must be at least 6 characters* |
| Confirm present | `required` | — | *Please input Confirm Password* |
| Confirm equals New | inline expression | — | *Password Not Match!* |

This endpoint takes primitives, so its net stays the existing length check in `UserService.AdminChangePassword`. See *Deviations*.

### Activate / deactivate modal (`#UpdateStatusModal`)

| Rule | View | Model | Message |
|---|---|---|---|
| Admin's own password present | `required` | — (primitive parameter) | *Please input Password to proceed* |

### Self-service modal (`#PasswordModal`, in `_Layout.cshtml`)

| Rule | View | Model | Message |
|---|---|---|---|
| Current Password present | `required` | `[Required]` | *Please input Current Password* — **new** |
| New present | `required` | `[Required]` | *Password must be at least 6 characters* |
| New ≥ 6 | `ng-minlength="6"` | `[StringLength(255, MinimumLength = 6)]` | *Password must be at least 6 characters* |
| Confirm present | `required` | `[Required]` | *Password Not Match!* |
| Confirm equals New | inline expression | `[Compare("NewPassword")]` | *Password Not Match!* |

This modal keeps its merged wording on purpose: `$error.required` and `$error.minlength` both render the same sentence, because that is what it says today. The admin reset modal keeps its split wording for the same reason.

### Login page (`Login.cshtml`)

| Rule | View | Model | Message |
|---|---|---|---|
| Username present | `required` | — (primitive parameters) | *Please input Username* — **new** |
| Password present | `required` | — | *Please input Password* — **new** |

### State rules — server only, messages unchanged

`Duplicate Username` · `Username already exists.` · `An account for this First Name and Last Name already exists.` · `Cannot Update User, ID# does not exist!` · `Wrong Password!` · `Invalid Password` / `Invalid Password!!` (not-found branches) · `You cannot deactivate your own account.` · `New password and confirmation do not match.` · `New password must be different from the current password.` · `Current password is incorrect.` · `Invalid Username or Password!!` · `Account is Locked. Contact MIS Department`

## Model layer

`UserModel` implements `IValidatableObject`; both models gain unmessaged attributes. No new types, no changed signatures, no folder-map change.

```csharp
[Required, StringLength(50)]
public string Username { get; set; }

[StringLength(255, MinimumLength = 6)]      // null on edit → validator skips it
public string Password { get; set; }
```

`[StringLength]` ignores `null`, so the edit path — where the modal hides the password field — passes untouched, and `IValidatableObject` carries the one genuinely conditional rule:

```csharp
public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
{
    if (ID == 0 && string.IsNullOrWhiteSpace(Password))
        yield return new ValidationResult(null, new[] { "Password" });
}
```

The `null` message is deliberate and consistent with the tier contract. `FullName` is computed and carries no rules; the binder already ignores it.

`ChangePasswordModel` uses `System.ComponentModel.DataAnnotations.CompareAttribute` (.NET 4.5+), not the `System.Web.Mvc` one.

## View layer

The markup follows the AngularJS Forms guide for 1.8.x verbatim: a named form, `name` on each input, and errors revealed by `form.$submitted || field.$touched`.

```html
<div ng-show="AccountForm.$submitted || AccountForm.Username.$touched">
  <div class="invalid-feedback d-block" ng-show="AccountForm.Username.$error.required">Please input Username</div>
</div>
```

`d-block` is required because Bootstrap 5 only reveals `.invalid-feedback` beside an `.is-invalid` sibling, and visibility here comes from `ng-show`.

Five forms are named: `AccountForm`, `ChangePasswordForm`, `StatusForm`, `PasswordForm`, `LoginForm`. A named form publishes its `FormController` on the scope, so handlers reach it as `$scope.<Name>`.

**Invalid styling is CSS only** — no `ng-class` on any input. Angular puts `ng-invalid` / `ng-touched` on controls and `ng-submitted` on the form, so one rule in `Site.css` covers every field, using CoreUI's own `--cui-form-invalid-border-color` token so dark mode follows for free.

**`ng-maxlength`, not `maxlength`.** The plain attribute blocks typing and truncates a paste silently, which would make the 50-character rule invisible and impossible to mirror.

**Modals are long-lived DOM and must be re-armed.** `$setPristine()` clears `$submitted` and propagates to every control; `$setUntouched()` clears the touched flags. Both are called when opening `#AccountModal`, `#ChangePasswordModal`, `#UpdateStatusModal` and `#PasswordModal`, and again when a server rejection clears the password fields. Without this, reopening a modal shows the previous attempt's red fields.

**`ng-submit` fires even when the form is invalid** — Angular does not block it — so every handler opens with an `$invalid` guard and the `if`/growl chain below it is deleted.

## Controller and service layer

Only two actions bind a complex type, so only two change:

```csharp
if (account == null || !ModelState.IsValid)
    return Json(new { message = "Invalid payload" });
```

The explicit null check stays alongside `ModelState`: an absent body binds to `null` outright rather than to an object with null properties, so the two conditions are not redundant. `Login`, `Logout`, `AdminChangePassword` and `UpdateStatus` keep their signatures and their existing `password != null` guards.

`UserService` loses `NamePattern`, `NameMessage`, the regex blocks in `SaveAccount` and `UpdateAccount`, `SaveAccount`'s password-length check and `ChangePassword`'s length check. It keeps `PasswordMinLength` / `PasswordMessage` for `AdminChangePassword`, and every state check unchanged. Error handling is untouched: `GetBaseException().Message` → growl.

## Deviations, recorded

1. **DataAnnotations at all.** OneMasaito has none. Justified above: the rules were already a deviation, and this is the smaller footprint.
2. **No request models.** `Login`, `AdminChangePassword` and `UpdateStatus` take primitives and therefore have no annotation net. This was chosen over adding `LoginRequest` / `AdminPasswordRequest` / `StatusRequest`, which would have added three types with no OneMasaito counterpart and rewritten three controller signatures. The loss is near zero: those endpoints enforce state rules, which annotations cannot express anyway, and the one real format rule among them keeps its service-side check.
3. **`role` is validated twice, unevenly.** `SaveNewAccount(UserModel account, string role)` receives Role inside the model *and* as a loose parameter, and the service passes the loose one to the sproc. The annotation covers `account.Role` only; `CHK_UserRole` is the net for the parameter. Folding `role` into the model would close the seam but move the signature away from OneMasaito's shape.
4. **Client-side rejection no longer clears the password fields.** Today a mismatch growls and blanks all three inputs. The inline message refers to what is on screen, so blanking it would be incoherent. Server-side rejections still clear them, as before.
5. **Three new messages** where there were none: Current Password required, and the two login fields.

## Verification

No test project exists; `BUILD_GUIDE.md` § 10 is the verification pass. It gains rows for: a message appearing under a field instead of as a growl, a reopened modal showing no stale errors, a 51-character name, an empty Current Password, and an empty login.

## Found in browser testing (same day)

Two defects in this design surfaced when the implementation was driven in a browser. Both are fixed; both are worth knowing before writing a form like this again.

1. **A rejected value survives a modal close/reopen.** When a validator fails, AngularJS parks `$modelValue` at `undefined`. Assigning a fresh model that simply *omits* the property is therefore not a change: the watch never fires, `$render()` never runs, and the rejected text stays in the input while the model is empty. Fix: the open handlers initialise every bound field explicitly. `Password` uses `null`, not `""`, because `[StringLength(255, MinimumLength = 6)]` skips `null` but fails an empty string — `""` would have broken every Edit save through the safety net.

2. **The first Save press after typing was swallowed.** Blurring the last field sets `$touched`, which reveals that field's message, which pushes the submit button about 25px down *between mousedown and mouseup*. A browser only fires `click` when both land on the same element, so the press did nothing and the user had to click twice. This is a direct consequence of the guide's `$submitted || $touched` gate combined with messages that take layout space. Fix: every message wrapper carries `class="field-feedback"` and `Site.css` keeps that row in the layout while hidden (`display: block !important; visibility: hidden`), so revealing a message shifts nothing.

The general lesson for this codebase: **a validation message that occupies layout space must reserve that space**, or it moves the control the user is aiming at.

## Deferred

- **Phase 2 — guard rails.** `ARCHITECTURE.md` § 7 items 4 and 5 (unguarded JSON actions, no anti-forgery). This is also where request models for the three primitive endpoints would earn their place.
- **Phase 3 — async/await.** Blocked on a language rule, not on effort: **C# forbids `out` parameters on async methods**, and `out string message` is the service idiom throughout. Going async means rewriting every service signature to return a result type, on top of the EDMX function imports being sync-only and `UniversalHelpers.CurrentUser` being a static property with a database hit behind every access. Worth doing only under real concurrency.
