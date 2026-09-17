using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using CoreUIDemo.Models;
using CoreUIDemo.App_Start;
using System.Web.Script.Serialization;
using System.Web.Security;
using CoreUIDemo.Helpers;

namespace CoreUIDemo.Services
{
    public class AccountService
    {
        public static bool LoginToSession(UserModel _userModel)
        {
            try
            {
                HttpContext.Current.Session["session_status"] = "online";

                PrincipalSerializedModel serializedModel = new PrincipalSerializedModel();

                serializedModel.Username = _userModel.Username;

                serializedModel.Password = _userModel.Password;

                serializedModel.SessionID = HttpContext.Current.Session.SessionID;

                JavaScriptSerializer serializer = new JavaScriptSerializer();

                string userData = serializer.Serialize(serializedModel);

                FormsAuthenticationTicket authenticationTicket = new FormsAuthenticationTicket
                    (1, _userModel.Username, DateTime.Now, DateTime.Now.AddMinutes(30), true, userData);

                string encryptedTicket = FormsAuthentication.Encrypt(authenticationTicket);

                HttpCookie authenticationCookie = new HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);

                HttpResponse response = HttpContext.Current.Response;

                response.Cookies.Add(authenticationCookie);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void LogoutFromSession(out string message)
        {
            try
            {
                message = "";

                FormsAuthentication.SignOut();
            }
            catch (Exception error)
            {
                message = error.Message;
            }
        }
    }
}
