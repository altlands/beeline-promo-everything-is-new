using Promo.EverythingIsNew.DAL.Cbn.Dto;
using Promo.EverythingIsNew.DAL.Events;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Promo.EverythingIsNew.DAL.Cbn
{
    public class CbnClient
    {
        private string CbnUrl;
        private string CbnUser;
        private string CbnPassword;
        private HttpClient client;

        public CbnClient(string cbnUrl, string cbnUser, string cbnPassword)
        {
            this.CbnUrl = cbnUrl;
            this.CbnUser = cbnUser;
            this.CbnPassword = cbnPassword;
            this.client = new HttpClient();

            client.BaseAddress = new Uri(CbnUrl);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var byteArray = Encoding.ASCII.GetBytes(CbnUser + ":" + CbnPassword);
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
        }

        public async Task<StatusResult> GetStatus(Status status)
        {
            CbnEvents.Log.CbnGetStatusStarted(status);
            string request = null;
            HttpResponseMessage response;

            try
            {
                request = String.Format("status?ctn={0}&uid={1}", status.ctn, status.uid);
                response = await client.GetAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<StatusResult>();
                    CbnEvents.Log.CbnGetStatusFinished(result);
                    return result;
                }
            }
            catch
            {
                //CbnEvents.Log.CbnGeneralExceptionError(MethodBase.GetCurrentMethod().Name, new CbnException(response.ToString()));
                throw;
            }
            return null;
        }

        public async Task<MessageResult> PostMessage(Message message)
        {
            CbnEvents.Log.CbnPostMessageStarted(message);
            HttpResponseMessage response;

            try
            {
                response = await client.PostAsJsonAsync("message", message);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<MessageResult>();
                    CbnEvents.Log.CbnPostMessageFinished(result);
                    return result;
                }
            }
            catch
            {
                //CbnEvents.Log.CbnGeneralExceptionError(MethodBase.GetCurrentMethod().Name, new CbnException(response.ToString()));
                throw;
            }
            return null;
        }

        public async Task<UpdateResult> Update(Update update)
        {
            CbnEvents.Log.CbnUpdateStarted(update);
            HttpResponseMessage response;

            try
            {
                response = await client.PostAsJsonAsync("update", update);
                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadAsAsync<UpdateResult>();
                    CbnEvents.Log.CbnUpdateFinished(result);
                    return result;
                }
                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var result = await response.Content.ReadAsAsync<UpdateResult>();
                    CbnEvents.Log.CbnUpdateFinished(result);
                    return result;
                }
            }
            catch
            {
                //CbnEvents.Log.CbnGeneralExceptionError(MethodBase.GetCurrentMethod().Name, new CbnException(response.ToString()));
                throw;
            }
            return null;
        }
    }
}
