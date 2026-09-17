using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CoreUIDemo.Models;
using System.Web.Mvc;
using CoreUIDemo.Models.ViewModels;
using System.Data.Entity;


namespace CoreUIDemo.Services
{
    public class Userservice : IUserService
    {
        public List<UserListItemViewModel> GetUsers(int pageNumber, int pageSize)
        {
            using (var db = new loginDemoEntities())
            {
                // Reads go through the VIEW via LINQ — no stored procedure needed here,
                // since this is a simple filtered/paged SELECT, not a write with business rules.
                int skipCount = (pageNumber - 1) * pageSize; // local var avoids C# overload-resolution ambiguity on Skip()
                return db.vw_Users
                          .OrderBy(u => u.FIRST_NAME).ThenBy(u => u.LAST_NAME)
                          .Skip(skipCount)
                          .Take(pageSize)
                          .Select(u => new UserListItemViewModel
                          {
                              Id = u.ID,
                              Username = u.USERNAME,
                              FirstName = u.FIRST_NAME,
                              LastName = u.LAST_NAME,
                              Role = u.UserRole,
                              IsActive = u.IS_ACTIVE
                          })
                          .ToList();
            }
        }
        public UserListItemViewModel GetUserById(long userId)
        {
            using (var db = new loginDemoEntities())
            {
                return db.vw_Users
                          .Where(u => u.ID == userId)
                          .Select(u => new UserListItemViewModel
                          {
                              Id = u.ID,
                              Username = u.USERNAME,
                              FirstName = u.FIRST_NAME,
                              LastName = u.LAST_NAME,
                              Role = u.UserRole,
                              IsActive = u.IS_ACTIVE
                          })
                          .FirstOrDefault();
            }
        }

        public void CreateUser(UserCreateViewModel model)
        {
            using (var db = new loginDemoEntities())
            {
                // Maps to the sp_InsertUserAccount Function Import — the § 3.1 version, with
                // @ROLE as the sixth parameter. This call is POSITIONAL: @IS_ACTIVE (true)
                // comes before the role, matching the procedure's declaration order.
                // Duplicate-username / duplicate-name checks happen INSIDE the procedure —
                // a SqlException with the RAISERROR message bubbles up here if a check fails
                // (which only works because the function import returns None, not a scalar — § 4).
                db.sp_InsertUserAccount(model.Username, model.Password, model.FirstName, model.LastName, true, model.Role);
            }
        }

        public void UpdateUser(UserEditViewModel model)
        {
            using (var db = new loginDemoEntities())
            {
                // Landmine: PASSWORD is NOT NULL and sp_UpdateUser overwrites it unconditionally.
                // This screen never collects a new password (that's ChangePassword's job), so we
                // must fetch the current password and pass it back unchanged here.
                long editId = model.Id; // USERS_ACCOUNTS.ID is bigint; sp_UpdateUser takes INT — explicit cast below at the call site
                string currentPassword = db.USERS_ACCOUNTS
                                            .Where(u => u.ID == editId)
                                            .Select(u => u.PASSWORD)
                                            .FirstOrDefault();

                db.sp_UpdateUser((int)model.Id, model.Username, model.FirstName, model.LastName,
                                  currentPassword, model.Role, model.IsActive);
            }
        }

        public void DeleteUser(long userId)
        {
            using (var db = new loginDemoEntities())
            {
                db.sp_DeleteUser((int)userId);
            }
        }

        public AuthenticatedUserData ValidateCredentials(string username, string password)
        {
            using (var db = new loginDemoEntities())
            {
                // Query USERS_ACCOUNTS directly (not the view) — need PASSWORD, which vw_Users excludes.
                // Password verification is intentionally NOT part of this LINQ predicate (see VerifyPassword) —
                // that's what lets it become a real hash comparison later without touching this query.
                // OneMasaito's ValidateUserLogin does the plaintext comparison INSIDE the LINQ predicate
                // itself (`r.PASSWORD == password`), which is what forces its verification logic to be
                // un-swappable later — this split is the deliberate fix.
                var account = db.USERS_ACCOUNTS
                                 .FirstOrDefault(u => u.USERNAME == username && u.IS_ACTIVE == true);

                if (account == null || !VerifyPassword(password, account.PASSWORD)) return null;

                return new AuthenticatedUserData
                {
                    UserId = account.ID,
                    Username = account.USERNAME,
                    FirstName = account.FIRST_NAME,
                    LastName = account.LAST_NAME,
                    Role = account.ROLE
                };
            }
        }

        public void ChangePassword(UserChangePasswordViewModel model)
        {
            using (var db = new loginDemoEntities())
            {
                // Maps to sp_UpdateUserPassword — throws (RAISERROR) if CurrentPassword doesn't match.
                db.sp_UpdateUserPassword((int)model.UserId, model.CurrentPassword, model.NewPassword);
            }
        }

        // Passwords stay plaintext for now, deliberately mirroring the reference project — see
        // Appendix D "Future work" for the BCrypt plan. This is the single place that changes
        // when hashing is introduced.
        private static bool VerifyPassword(string suppliedPassword, string storedPassword)
        {
            return suppliedPassword == storedPassword;
        }
    }
}