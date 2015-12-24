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
        public static string OtherRegion = ConfigurationManager.AppSettings["OtherRegion"];

        public static string CbnUrl = ConfigurationManager.AppSettings["CbnUrl"];
        public static string CbnUser = ConfigurationManager.AppSettings["CbnUser"];
        public static string CbnPassword = ConfigurationManager.AppSettings["CbnPassword"];

        public static CbnClient CbnClient = new CbnClient(CbnUrl, CbnUser, CbnPassword);

        public static string Soc = ConfigurationManager.AppSettings["Soc"];
        public static string dcpConnectionString = ConfigurationManager.AppSettings["DcpConnectionString"];

        public static TariffIndexesCollection TariffIndexes =
            ((TariffsConfiguration)ConfigurationManager.GetSection("tariffsConfiguration")).Codes;

        public static bool VkStubMode = ConfigurationManager.AppSettings["VkStubMode"] == "true";
        public static bool IgnoreSsl = ConfigurationManager.AppSettings["IgnoreSsl"] == "true";
        public static bool IsRedirectForBeelineCtn = ConfigurationManager.AppSettings["IsRedirectForBeelineCtn"] == "true";
        public static bool IsStubMode = ConfigurationManager.AppSettings["IsStubMode"] == "true";
        public static string EndActionDate = ConfigurationManager.AppSettings["EndActionDate"];
        

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

            var dpcObservable = new ObservableEventListener();
            dpcObservable.EnableEvents(DpcEvents.LogEventSource, EventLevel.Verbose, (EventKeywords)(-1));
            dpcObservable.LogToCategory("dpc");

            var errorObservable = new ObservableEventListener();
            errorObservable.EnableEvents(ErrorEvents.LogEventSource, EventLevel.Verbose, (EventKeywords)(-1));
            errorObservable.LogToCategory("err");

            if (IgnoreSsl)
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
            }
        }

        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }

        void Application_Error(object sender, EventArgs e)
        {
            Exception exc = Server.GetLastError();
            ErrorEvents.Log.GeneralError(exc);
            Response.Write("<h2>Ошибка</h2>");
            //Response.Write("<p>" + exc.Message + "</p>\n");
            Response.Write("Вернуться на <a href='/'>Главную страницу</a>");
            Server.ClearError();
        }
    }
}
