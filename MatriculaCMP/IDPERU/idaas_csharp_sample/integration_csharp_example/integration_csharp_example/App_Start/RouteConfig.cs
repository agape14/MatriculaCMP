using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

/**
 * @author Miguel Pazo (https://miguelpazo.com)
 */
namespace integration_csharp_example
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "",
                defaults: new { controller = "Index", action = "Index" }
            );

            routes.MapRoute(
                name: "Endpoint",
                url: "auth-endpoint",
                defaults: new { controller = "Index", action = "Endpoint" }
            );

            routes.MapRoute(
               name: "Home",
               url: "home",
               defaults: new { controller = "Home", action = "Index" }
           );

            routes.MapRoute(
                name: "Logout",
                url: "logout",
                defaults: new { controller = "Home", action = "Logout" }
            );
        }
    }
}
