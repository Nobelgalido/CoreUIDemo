using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using secPrincipal = System.Security.Principal;
using CoreUIDemo.Models;

namespace CoreUIDemo.App_Start
{
    public class Principal : IPrincipal
    {
        public secPrincipal.IIdentity Identity { get; private set; }
        public bool IsInRole(string role)
        {
            return true;
        }

        public Principal(string name)
        {
            Identity = new secPrincipal.GenericIdentity(name);
        }

        public string Username { get; set; }
        public string Password { get; set; }
        public string SessionID { get; set; }
    }

    public class PrincipalSerializedModel
    {
        public string Username { get; set; }
        public string Password { get; set; }
        public string SessionID { get; set; }
    }

    interface IPrincipal : secPrincipal.IPrincipal
    {
        string Username { get; set; }
        string Password { get; set; }
        string SessionID { get; set; }
    }
}
