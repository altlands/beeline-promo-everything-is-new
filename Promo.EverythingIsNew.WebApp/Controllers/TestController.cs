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

        public async Task<JsonResult> I2()
        {
            var update = new Update
            {
                ctn = "9175036509",
                name = "Павел",
                surname = "Абрамов",
                region = "Нижний Новгород",
                email = "m.c.peru@gmail.com",
                birth_date = "29.03.1999 0:00:00",
                email_unsubscribe = false
            };

            CbnEvents.Log.CbnUpdateStarted(update);
            string request = null;
            HttpResponseMessage response = null;
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(MvcApplication.CbnUrl);
            string json;


            client.DefaultRequestHeaders.ExpectContinue = false;



            var byteArray = Encoding.ASCII.GetBytes(MvcApplication.CbnUser + ":" + MvcApplication.CbnPassword);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

            UpdateResult result = new UpdateResult();

            try
            {
                response = await client.PostAsJsonAsync("update", update);
                if (response.IsSuccessStatusCode)
                {
                    result = await response.Content.ReadAsAsync<UpdateResult>();
                    CbnEvents.Log.CbnUpdateFinished(result);
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    result = await response.Content.ReadAsAsync<UpdateResult>();
                    CbnEvents.Log.CbnUpdateFinished(result);
                    return Json(result, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception e)
            {
                CbnEvents.Log.CbnGeneralExceptionError(MethodBase.GetCurrentMethod().Name, new CbnException(response != null ? response.ToString() : "response = null", e.InnerException));


                return Json(result, JsonRequestBehavior.AllowGet);

            }
            return Json(result, JsonRequestBehavior.AllowGet);
        }
    }
}