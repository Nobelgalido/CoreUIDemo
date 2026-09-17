using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CoreUIDemo.Models;
using CoreUIDemo.Services;
using System.Web.Mvc;
using System.Web.Script.Serialization;
using System.Web.Security;
using CoreUIDemo.Models.ViewModels;

namespace CoreUIDemo.Controllers
{
    [AllowAnonymous]
    public class AccountController : BaseController
    {
        [HttpGet]
        public ActionResult Login() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var userData = UserService.ValidateCredentials(model.Username, model.Password);
            if (userData == null)
            {
                ModelState.AddModelError("", "Invalid username or password.");
                return View(model);
            }

            // Matches OneMasaito's ticket construction closely (AccountService.LoginToSession
            // uses the same JavaScriptSerializer + FormsAuthenticationTicket pattern) — see
            // Global.asax.cs for the matching deserialize side, and Appendix B for the one
            // deliberate difference: isPersistent below is false, where OneMasaito's is true.
            string serializedData = new JavaScriptSerializer().Serialize(userData);

            var ticket = new FormsAuthenticationTicket(
                version: 1,
                name: userData.Username,
                issueDate: DateTime.Now,
                expiration: DateTime.Now.AddMinutes(30),
                isPersistent: false,
                userData: serializedData);

            string encryptedTicket = FormsAuthentication.Encrypt(ticket);
            Response.Cookies.Add(new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket) { HttpOnly = true });

            // Admins land on user management; everyone else only has self-service actions.
            return userData.Role == "admin"
                ? RedirectToAction("Index", "Users")
                : RedirectToAction("Profile", "Users");
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            Session.Clear();
            return RedirectToAction("Login");
        }

        [HttpGet]
        public JsonResult CurrentUserInfo()
        {
            return Json(CurrentUser, JsonRequestBehavior.AllowGet);
        }
    }
}