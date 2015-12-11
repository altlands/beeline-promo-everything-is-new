using AltLanDS.Beeline.DpcProxy.Client;
using Newtonsoft.Json;
using Promo.EverythingIsNew.DAL.Cbn;
using Promo.EverythingIsNew.DAL.Cbn.Dto;
using Promo.EverythingIsNew.DAL.Events;
using Promo.EverythingIsNew.DAL.Vk;
using Promo.EverythingIsNew.WebApp.Models;
using System;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Promo.EverythingIsNew.WebApp.Controllers
{
    public class TestController : Controller
    {
        public async Task<ActionResult> Index()
        {
            var status = new Status { ctn = "9999999999", uid = "0000000000" };
            CbnEvents.Log.TestCbnGetStatusStarted(status);
            string request = null;
            HttpResponseMessage response;
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(MvcApplication.CbnUrl);

            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var byteArray = Encoding.ASCII.GetBytes(MvcApplication.CbnUser + ":" + MvcApplication.CbnPassword);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            StatusResult result = new StatusResult();

            try
            {
                request = String.Format("status?ctn={0}&uid={1}", status.ctn, status.uid);
                response = await client.GetAsync(request);
                CbnEvents.Log.TestCbnGetStatusResponse(response);
                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadAsAsync<StatusResult>();
                    CbnEvents.Log.TestCbnGetStatusFinished(result);
                }
            }
            catch (Exception e)
            {
                CbnEvents.Log.CbnGeneralExceptionError(MethodBase.GetCurrentMethod().Name, new CbnException("catch", e.InnerException));
            }

            //MainUssHandler h = new MainUssHandler();
            //UssApiClient cl = new UssApiClient(h);

            //var method = new CreateSsoRequestMethod(ussLogin, linkedAccountLogin, isInvite, nickName);
            //var response = await cl.ProcessRequestAsync(method);
            //return await method.ParseResponseAsync(response);

            //HttpMessageHandler handler = new MainUssHandler(delegatingHandler);
            //_httpClient = new HttpClient(handler);

            return Content(JsonConvert.SerializeObject(result));
        }

        public async Task<ActionResult> I2()
        {
            var status = new Status { ctn = "9999999999", uid = "0000000000" };
            CbnEvents.Log.TestCbnGetStatusStarted(status);
            string request = null;
            HttpResponseMessage response;
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(MvcApplication.CbnUrl);
            string json;

            var byteArray = Encoding.ASCII.GetBytes(MvcApplication.CbnUser + ":" + MvcApplication.CbnPassword);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            StatusResult result = new StatusResult();

            try
            {
                request = String.Format("status?ctn={0}&uid={1}", status.ctn, status.uid);
                response = await client.GetAsync(request);
                CbnEvents.Log.TestCbnGetStatusResponse(response);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                    json = JsonConvert.DeserializeObject<dynamic>(content);
                    CbnEvents.Log.TestCbnGetStatusFinished(result);
                }
            }
            catch (Exception e)
            {
                CbnEvents.Log.CbnGeneralExceptionError(MethodBase.GetCurrentMethod().Name, new CbnException("catch", e.InnerException));
            }

            return Content(JsonConvert.SerializeObject(result));
        }
    }
}