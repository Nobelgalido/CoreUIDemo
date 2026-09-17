using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CoreUIDemo.Helpers;
using CoreUIDemo.Services;
using CoreUIDemo.Models;
using CoreUIDemo.Models.ViewModels;

namespace CoreUIDemo.Controllers
{
    public class UsersController : BaseController
    {
        // Create/Edit are Bootstrap modals on this same page (mirroring OneMasaito's
        // Settings/UserAccounts screen, but with one Create/Edit modal instead of OneMasaito's
        // five separate modals — see Appendix A) — there's no separate Create/Edit page route.
        [Authorize(Roles = "admin")]
        [HttpGet]
        public ActionResult Index() => View();

        // Any authenticated user lands here after login if they're not an admin —
        // the only self-service action available is changing your own password.
        [Authorize]
        [HttpGet]
        public new ActionResult Profile() => View(CurrentUser);

        [Authorize(Roles = "admin")]
        [HttpGet]
        public JsonResult GetUsers(int pageNumber = 1, int pageSize = 25)
        {
            var users = UserService.GetUsers(pageNumber, pageSize);
            return Json(users, JsonRequestBehavior.AllowGet);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAngularAntiForgeryToken]
        public JsonResult Create(UserCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, errors });
            }
            try
            {
                UserService.CreateUser(model);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errors = new[] { ex.Message } });
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAngularAntiForgeryToken]
        public JsonResult Edit(UserEditViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                return Json(new { success = false, errors });
            }
            try
            {
                UserService.UpdateUser(model);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errors = new[] { ex.Message } });
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAngularAntiForgeryToken]
        public JsonResult Delete(long userId, long actingUserId)
        {
            UserService.DeleteUser(userId , actingUserId);
            return Json(new { success = true });
        }

        // Self-service password change — any authenticated user, not just admins.
        // UserId is deliberately taken from the auth ticket, never from the client-submitted
        // model, so an authenticated user can only ever change their own password here.
        [Authorize]
        [HttpPost]
        [ValidateAngularAntiForgeryToken]
        public JsonResult ChangePassword(UserChangePasswordViewModel model)
        {
            model.UserId = CurrentUser.UserId;

            if (!ModelState.IsValid)
                return Json(new { success = false, errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage) });
            try
            {
                UserService.ChangePassword(model);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, errors = new[] { ex.Message } });
            }
        }
    }
}