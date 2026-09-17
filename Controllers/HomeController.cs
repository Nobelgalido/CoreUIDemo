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
