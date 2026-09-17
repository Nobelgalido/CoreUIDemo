using System.Web.Helpers;
using System.Web.Mvc;

namespace CoreUIDemo.Helpers
{
    // [ValidateAntiForgeryToken] only ever checks Request.Form, which stays empty for
    // Angular's JSON-bodied $http POSTs — so it silently rejects every real request.
    // AngularJS's $http has built-in CSRF support: it reads a readable "XSRF-TOKEN"
    // cookie (issued in BaseController.OnActionExecuting) and echoes it back as the
    // "X-XSRF-TOKEN" header on every same-origin call automatically, no client code
    // needed. This validates that header against the standard antiforgery cookie.
    public class ValidateAngularAntiForgeryTokenAttribute : FilterAttribute, IAuthorizationFilter
    {
        public void OnAuthorization(AuthorizationContext filterContext)
        {
            var request = filterContext.HttpContext.Request;
            var cookie = request.Cookies[AntiForgeryConfig.CookieName];
            AntiForgery.Validate(cookie?.Value, request.Headers["X-XSRF-TOKEN"]);
        }
    }
}