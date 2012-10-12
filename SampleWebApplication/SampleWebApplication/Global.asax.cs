using System.Data.Common;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Routing;

using MonkeyOrm;

using Sinbadsoft.Lib.UserManagement;

namespace SampleWebApplication
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class MvcApplication : System.Web.HttpApplication
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "Default", // Route name
                "{controller}/{action}/{id}", // URL with parameters
                new { controller = "Home", action = "Index", id = UrlParameter.Optional } // Parameter defaults
            );
        }

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            RegisterGlobalFilters(GlobalFilters.Filters);
            RegisterRoutes(RouteTable.Routes);

            var connectionSettings = WebConfigurationManager.ConnectionStrings["default"];

            // Optional: creates the Users table if it is not already there.
            using (var connection = DbProviderFactories.GetFactory(connectionSettings.ProviderName).CreateConnection())
            {
                connection.ConnectionString = connectionSettings.ConnectionString;
                connection.Open();
                UsersTable.CreateIfMissing(connection);
            }
        }
    }
}