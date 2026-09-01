using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace TFDSolution
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                namespaces: new[] { "TFDSolution.Controllers" } // 👈 restrict namespace here
            );

            routes.MapRoute(
                name: "MyPage",
                url: "{controller}/{action}/{pagename}",
                defaults: new { controller = "Home", action = "MyPage", pagename = UrlParameter.Optional },
                namespaces: new[] { "TFDSolution.Controllers" } // 👈 restrict namespace here
            );

            routes.MapRoute(
                name: "DashboardPage",
                url: "{controller}/{action}/{dashboardName}",
                defaults: new { controller = "Dashboard", action = "Index", dashboardName = UrlParameter.Optional },
                namespaces: new[] { "TFDSolution.Controllers" }
            );
        }
    }
}
