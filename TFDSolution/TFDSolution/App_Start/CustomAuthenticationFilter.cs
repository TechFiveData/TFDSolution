using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc.Filters;
using System.Web.Mvc;
using System.Web.Routing;
using System.IO;
using TFDSolution.Common;
using TFDSolution.Models;

namespace TFDSolution.App_Start
{
    public class CustomAuthenticationFilter : ActionFilterAttribute, IAuthenticationFilter
    {
        public void OnAuthentication(AuthenticationContext filterContext)
        {
            if (string.IsNullOrEmpty(Convert.ToString(filterContext.HttpContext.Session["LoginUserInfo"])))
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }
        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            if (filterContext.Result == null || filterContext.Result is HttpUnauthorizedResult)
            {
                LogUnauthorizedAccess();
                // Redirecting the user to the Login View of Account Controller
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "controller", "Login" },
                        { "action", "Index" }
                    });
            }
        }
        private void LogUnauthorizedAccess()
        {
            // Write code here (e.g., save to DB or log file)
            System.Diagnostics.Debug.WriteLine("Unauthorized access logged.");
        }
    }

    public class LinceseAuthenticationFilter : ActionFilterAttribute, IAuthenticationFilter
    {
        public void OnAuthentication(AuthenticationContext filterContext)
        {
            LicenseModel res = ValidationKey.IsValid();
            if (res != null && res.Status != 1)
            {
                filterContext.Result = new HttpUnauthorizedResult();
            }
        }
        public void OnAuthenticationChallenge(AuthenticationChallengeContext filterContext)
        {
            if (filterContext.Result == null || filterContext.Result is HttpUnauthorizedResult)
            {
                // Redirecting the user to the Login View of Account Controller
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary
                    {
                        { "controller", "Default" },
                        { "action", "Index" }
                    });
            }
        }
    }
}