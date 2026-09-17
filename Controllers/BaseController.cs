using System;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using CoreUIDemo.Models;
using CoreUIDemo.Services;

namespace CoreUIDemo.Controllers
{
    public abstract class BaseController : Controller
    {
        // OneMasaito has no DI container and instantiates services directly — mirrored here,
        // just interface-typed so it stays swappable/testable.
        protected readonly IUserService UserService = new Userservice();

        // Populated by Global.asax's Application_PostAuthenticateRequest from the decrypted
        // FormsAuth ticket. Null when the request is unauthenticated.
        protected AuthenticatedUserData CurrentUser => HttpContext.Items["CurrentUser"] as AuthenticatedUserData;

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);
            IssueAngularAntiForgeryCookie();
        }

        // Pairs with ValidateAngularAntiForgeryTokenAttribute — see that file for why this
        // exists. Reuses the existing antiforgery cookie token when there is one instead of
        // rotating it on every page load (rotating unconditionally would break a POST made
        // from a page the user has had open across more than one navigation).
        private void IssueAngularAntiForgeryCookie()
        {
            var existingCookie = Request.Cookies[AntiForgeryConfig.CookieName];
            AntiForgery.GetTokens(existingCookie?.Value, out string cookieToken, out string formToken);

            if (cookieToken != null)
            {
                Response.Cookies.Add(new HttpCookie(AntiForgeryConfig.CookieName, cookieToken) { HttpOnly = true });
            }
            Response.Cookies.Add(new HttpCookie("XSRF-TOKEN", formToken) { HttpOnly = false });
        }
    }
}