using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using System.Web.Script.Serialization;
using System.Web.Security;


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

        protected void Application_PostAuthenticateRequest(object sender, EventArgs e)
        {
            var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
            if (authCookie == null) return;

            FormsAuthenticationTicket ticket;
            try
            {
                ticket = FormsAuthentication.Decrypt(authCookie.Value);
            }
            catch
            {
                return; // malformed/tampered cookie — leave user unauthenticated
            }

            if (ticket == null || string.IsNullOrEmpty(ticket.UserData)) return;

            var userData = new JavaScriptSerializer().Deserialize<Models.AuthenticatedUserData>(ticket.UserData);

            var identity = new FormsIdentity(ticket);
            var principal = new GenericPrincipal(identity, new[] { userData.Role }); // role goes into the principal too
            HttpContext.Current.User = principal;
            HttpContext.Current.Items["CurrentUser"] = userData;
        }
    }
}