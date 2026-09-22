using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data.Entity;
using System.Text.RegularExpressions;
using CoreUIDemo.Models;
using CoreUIDemo.Helpers;

namespace CoreUIDemo.Services
{
    public class UserService
    {
        private const string NamePattern = "^[a-zA-Z ]+$";
        private const int PasswordMinLength = 6;
        private const string NameMessage = "First Name and Last Name may contain letters and spaces only";
        private const string PasswordMessage = "Password must be at least 6 characters";

        public static UserModel ValidateUserLogin(string _username, string _password, out string returnString)
        {
            returnString = "";

            UserModel userModel = null;

            try
            {
                using (var db = new loginDemoEntities())
                {
                    var user = db.USERS_ACCOUNTS.FirstOrDefault(r => r.USERNAME == _username && r.PASSWORD == _password);

                    if (user != null)
                    {
                        if (user.IS_ACTIVE)
                        {
                            userModel = new UserModel
                            {
                                ID = user.ID,
                                Username = user.USERNAME,
                                Password = user.PASSWORD,
                                FirstName = user.FIRST_NAME,
                                LastName = user.LAST_NAME,
                                Role = user.ROLE,
                                IsActive = user.IS_ACTIVE
                            };
                        }
                        else
                        {
                            returnString = "Account is Locked. Contact MIS Department";
                        }
                    }
                    else
                    {
                        returnString = "Invalid Username or Password!!";
                    }
                }
            }
            catch (Exception error)
            {
                returnString = "Error | Validate User Login" + error.Message;
            }

            return userModel;
        }//end of validate login

        public static void ChangePassword(ChangePasswordModel _pass, out string message)
        {
            message = "";

            try
            {
                if (_pass.NewPassword == null || _pass.NewPassword.Length < PasswordMinLength)
                {
                    message = PasswordMessage;
                    return;
                }

                using (var db = new loginDemoEntities())
                {
                    var currentUser = UniversalHelpers.CurrentUser;

                    db.sp_UpdateUserPassword((int)currentUser.ID, _pass.CurrentPassword, _pass.NewPassword, _pass.ConfirmPassword);
                }
            }
            catch (Exception error)
            {
                message = error.GetBaseException().Message;
            }
        }

        public static List<UserModel> GetAllAccount()
        {
            List<UserModel> accountList = null;

            using (var db = new loginDemoEntities())
            {
                var list = from a in db.vw_Users
                           orderby a.FIRST_NAME ascending
                           select new UserModel
                           {
                               ID = a.ID,
                               Username = a.USERNAME,
                               FirstName = a.FIRST_NAME,
                               LastName = a.LAST_NAME,
                               Role = a.UserRole,
                               IsActive = a.IS_ACTIVE
                           };

                accountList = list.ToList();
            }
            return accountList;
        }

        public static bool CheckUserNameDuplicate(string _username)
        {
            using (var db = new loginDemoEntities())
            {
                var checkDuplicate = db.USERS_ACCOUNTS.Where(r => r.USERNAME.Replace(" ", "") == _username.Replace(" ", "")).FirstOrDefault();

                if (checkDuplicate == null)
                    return false;
                else
                    return true;
            }
        }

        public static bool SaveAccount(UserModel _account, string _role, out string message)
        {
            message = "";

            try
            {
                
                if (!Regex.IsMatch(_account.FirstName ?? "", NamePattern) || !Regex.IsMatch(_account.LastName ?? "", NamePattern))
                {
                    message = NameMessage;
                    return false;
                }

                if (_account.Password == null || _account.Password.Length < PasswordMinLength)
                {
                    message = PasswordMessage;
                    return false;
                }

                using (var db = new loginDemoEntities())
                {
                    db.sp_InsertUserAccount(_account.Username, _account.Password, _account.FirstName, _account.LastName, true, _role);

                    return true;
                }
            }
            catch (Exception error)
            {
                message = error.GetBaseException().Message;
                return false;
            }
        }

        public static bool UpdateAccount(UserModel _account, string _role, out string message)
        {
            message = "";

            try
            {
                if (!Regex.IsMatch(_account.FirstName ?? "", NamePattern) || !Regex.IsMatch(_account.LastName ?? "", NamePattern))
                {
                    message = NameMessage;
                    return false;
                }

                using (var db = new loginDemoEntities())
                {
                    var currentPassword = db.USERS_ACCOUNTS
                                            .Where(r => r.ID == _account.ID)
                                            .Select(r => r.PASSWORD)
                                            .FirstOrDefault();

                    db.sp_UpdateUser((int)_account.ID, _account.Username, _account.FirstName, _account.LastName, currentPassword, _role, _account.IsActive);

                    return true;
                }
            }
            catch (Exception error)
            {
                message = error.GetBaseException().Message;
                return false;
            }
        }

        public static void AdminChangePassword(long _id, string _password, out string message)
        {
            message = "";

            try
            {
                if (_password == null || _password.Length < PasswordMinLength)
                {
                    message = PasswordMessage;
                    return;
                }

                using (var db = new loginDemoEntities())
                {
                    var user = db.USERS_ACCOUNTS.FirstOrDefault(r => r.ID == _id);

                    if (user != null)
                    {
                        user.PASSWORD = _password;

                        db.Entry(user).State = EntityState.Modified;

                        db.SaveChanges();
                    }
                    else
                    {
                        message = "Invalid Password";
                    }
                }
            }
            catch (Exception error)
            {
                message = error.GetBaseException().Message;
            }
        }

        public static void AdminUpdateStatus(long _id, string _password, out string message)
        {
            message = "";

            try
            {
                using (var db = new loginDemoEntities())
                {
                    var currentUser = UniversalHelpers.CurrentUser.ID;

                    var adminID = db.USERS_ACCOUNTS.FirstOrDefault(r => r.ID == currentUser);

                    var user = db.USERS_ACCOUNTS.FirstOrDefault(r => r.ID == _id);

                    if (user != null)
                    {
                        if (adminID.PASSWORD == _password)
                        {
                            if (user.IS_ACTIVE)
                            {
                                db.sp_DeleteUser((int)_id, (int)currentUser);
                            }
                            else
                            {
                                user.IS_ACTIVE = true;

                                db.Entry(user).State = EntityState.Modified;

                                db.SaveChanges();
                            }
                        }
                        else
                        {
                            message = "Wrong Password!";
                        }
                    }
                    else
                    {
                        message = "Invalid Password!!";
                    }
                }
            }
            catch (Exception error)
            {
                message = error.GetBaseException().Message;
            }
        }
    }
}
