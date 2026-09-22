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
            string serverResponse = "";
            
            if (password != null)
            {
                UserService.ChangePassword(password, out serverResponse);
            }
             
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
