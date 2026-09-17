using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Script.Serialization;
using System.Web.Security;
using CoreUIDemo.Models;
using CoreUIDemo.App_Start;

namespace CoreUIDemo.Helpers
{
    public class UniversalHelpers
    {
        public static UserModel CurrentUser
        {
            get
            {
                UserModel user = null;

                HttpCookie authCookie_coreuidemo = HttpContext.Current.Request.Cookies[FormsAuthentication.FormsCookieName];

                if (authCookie_coreuidemo != null)
                {
                    FormsAuthenticationTicket authTicket_coreuidemo = FormsAuthentication.Decrypt(authCookie_coreuidemo.Value);

                    JavaScriptSerializer serializer = new JavaScriptSerializer();

                    PrincipalSerializedModel serializedModel = serializer.Deserialize<PrincipalSerializedModel>(authTicket_coreuidemo.UserData);

                    Principal newUser = new Principal(authTicket_coreuidemo.Name);

                    newUser.Username = serializedModel.Username;

                    newUser.SessionID = serializedModel.SessionID;

                    HttpContext.Current.User = newUser;

                    using (var db = new loginDemoEntities())
                    {
                        var query = from a in db.USERS_ACCOUNTS
                                    where a.USERNAME == newUser.Username
                                    select new UserModel
                                    {
                                        ID = a.ID,
                                        Username = a.USERNAME,
                                        FirstName = a.FIRST_NAME,
                                        LastName = a.LAST_NAME,
                                        Role = a.ROLE,
                                        IsActive = a.IS_ACTIVE
                                    };

                        user = query.FirstOrDefault();
                    }

                    return user;
                }
                else
                {
                    return user;
                }
            }
        }
    }
}
