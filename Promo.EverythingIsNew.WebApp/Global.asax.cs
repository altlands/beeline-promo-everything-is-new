using FullScale180.SemanticLogging;
using Microsoft.Practices.EnterpriseLibrary.SemanticLogging;
using Promo.EverythingIsNew.DAL;
using Promo.EverythingIsNew.DAL.Cbn;
using Promo.EverythingIsNew.WebApp.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Microsoft.Practices.Unity;
using AltLanDS.Logging;
using AltLanDS.Framework;
using Promo.EverythingIsNew.DAL.Events;
using System.Reflection;

namespace Promo.EverythingIsNew.WebApp
{
    public class MvcApplication : System.Web.HttpApplication
    {
        public static string VkAppId = ConfigurationManager.AppSettings["VkAppId"];
        public static string VkAppSecretKey = ConfigurationManager.AppSettings["VkAppSecretKey"];
        public static string Hostname = ConfigurationManager.AppSettings["RedirectHostname"];
        public static string RedirectUri = Hostname + (Hostname.Substring(Hostname.Length - 1, 1) == "/" ? "VkResult" : "/VkResult");
        public static string PersonalBeelineUrl = ConfigurationManager.AppSettings["PersonalBeelineUrl"];
        public static int MinimumAge = int.Parse(ConfigurationManager.AppSettings["MinimumAge"]);
        public static int MaximumAge = int.Parse(ConfigurationManager.AppSettings["MaximumAge"]);

        public static string CbnUrl = ConfigurationManager.AppSettings["CbnUrl"];
        public static string CbnUser = ConfigurationManager.AppSettings["CbnUser"];
        public static string CbnPassword = ConfigurationManager.AppSettings["CbnPassword"];

        public static CbnClient CbnClient = new CbnClient(CbnUrl, CbnUser, CbnPassword);

        public static string Soc = ConfigurationManager.AppSettings["Soc"];
        public static string dcpConnectionString = ConfigurationManager.AppSettings["DcpConnectionString"];

        public static TariffIndexesCollection TariffIndexes =
            ((TariffsConfiguration)ConfigurationManager.GetSection("tariffsConfiguration")).Codes;

        protected void Application_Start()
        {

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            AltLanDS.Framework.Application.Current.Start();

            var vkObservable = new ObservableEventListener();
            vkObservable.EnableEvents(VkEvents.LogEventSource, EventLevel.Verbose, (EventKeywords)(-1));
            vkObservable.LogToCategory("vk");

            var cbnObservable = new ObservableEventListener();
            cbnObservable.EnableEvents(CbnEvents.LogEventSource, EventLevel.Verbose, (EventKeywords)(-1));
            cbnObservable.LogToCategory("cbn");

            LogtestEvents();

#if DEBUG
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
#endif

            //var observable = new ObservableEventListener();
            //observable.EnableEvents(SelfCareWidgetEvents.LogEventSource, EventLevel.Verbose, (EventKeywords)(-1));
            //observable.LogToCategory("fttbSelfCare");

            //var observable2 = new ObservableEventListener();
            //observable2.EnableEvents(UssEvents.LogEventSource, EventLevel.Verbose, (EventKeywords)(-1));
            //observable2.LogToCategory("uss");

            //var observable3 = new ObservableEventListener();
            //observable3.EnableEvents(UssEvents.LogEventSource, EventLevel.Error, (EventKeywords)(-1));
            //observable3.LogToCategory("errors");

        }

        private static void LogtestEvents()
        {
            var listener = new ObservableEventListener();
            listener.EnableEvents(TestEvents.Log, EventLevel.LogAlways, Keywords.All);

            listener.LogToConsole();
            listener.LogToElasticsearch("SLABEL", "http://localhost:9200", "myindex", "mytype", bufferingCount: 1);

            TestEvents.Log.Critical("Hello world In-Process Critical");
            TestEvents.Log.Error("Hello world In-Process Error");
            TestEvents.Log.Informational("Hello world In-Process Informational");
        }
    }
}
