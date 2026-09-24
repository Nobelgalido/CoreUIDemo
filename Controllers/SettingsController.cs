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
        // GET: Settings/UserAccounts
        public ActionResult UserAccounts()
        {
            var user = UniversalHelpers.CurrentUser;

            if (user == null)
            {
                return RedirectToAction("Login", "Home");
            }
            //    //return RedirectToRoute(new { controller = "Home", action = "Login", id = UrlParameter.Optional });
            //else
            return View();
        }

        // POST: Settings/GetAccounts
        [HttpPost]
        public JsonResult GetAccounts()
        {
            var accountList = UserService.GetAllAccount();

            return Json(new {  accountList });
        }


        // POST: Settings/SaveNewAccount
        [HttpPost]
        public JsonResult SaveNewAccount(UserModel account, string role)
        {
            if (account == null || !ModelState.IsValid)
            {
                return Json(new { message = "Invalid payload" });
            }

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
            {
                return Json(new { message = "Saved" });
            }
            return Json(new { message = string.IsNullOrEmpty(message) ? "Error on Saving" : message });
        }


        // POST: Settings/AdminChangePassword
        [HttpPost]
        public JsonResult AdminChangePassword(long account, string password)
        {
            var serverResponse = "";

            if (password != null)
            {
                UserService.AdminChangePassword(account, password, out serverResponse);
            }
            return Json(new { errorMessage = serverResponse });
        }


        // POST:  Settings/UpdateStatus
        [HttpPost]
        public JsonResult UpdateStatus(long account, string password)
        {
            var serverResponse = "";

            if (password != null)
            {
                UserService.AdminUpdateStatus(account, password, out serverResponse);
            }
            return Json(new { errorMessage = serverResponse });
        }
    }
}
