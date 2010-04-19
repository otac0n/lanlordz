namespace LanLordz
{
    using System;
    using System.Web.Mvc;
    using System.Web.Routing;
    using Spark;
    using Spark.Web.Mvc;
    using LanLordz.Models;

    public class LanLordzApplication : System.Web.HttpApplication
    {
        private static object locker = new object();
        public static object Locker
        {
            get
            {
                return locker;
            }
        }

        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                "PngFix",
                "PngFix.htc",
                new { controller = "Home", action = "PngFix" });

            routes.MapRoute(
                "Sitemap",
                "public_html/sitemap.xml",
                new { controller = "Sitemap", action = "Index" });

            routes.MapRoute(
                "Search In URL",
                "Forums/Search/{searchTerms}",
                new { controller = "Forums", action = "Search", id = (int?)null });

            routes.MapRoute(
                "RenderTournamentPng",
                "Events/RenderTournament/{id}.svg",
                new { controller = "Events", action = "RenderTournament", id = (int?)null });

            routes.MapRoute(
                "DefaultWithTitle",
                "{controller}/{action}/{id}/{title}",
                new { controller = "Home", action = "Index", id = (int?)null },
                new { id = @"\d*" });

            routes.MapRoute(
                "Default",
                "{controller}/{action}/{id}",
                new { controller = "Home", action = "Index", id = (int?)null },
                new { id = @"\d*" });
        }

        protected void Session_Start()
        {
            var db = new LanLordzDataContext();
            lock (Locker)
            {
                db.ExecuteCommand("UPDATE dbo.Configuration SET Value = cast(Value as bigint) + 1 WHERE Property = 'Visitors'");
            }
        }

        protected void Application_Start()
        {
            ViewEngines.Engines.Add(new SparkViewFactory());

            RegisterRoutes(RouteTable.Routes);
        }
    }
}