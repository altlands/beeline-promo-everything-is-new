using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Promo.EverythingIsNew.DAL.Events;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Cache;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Promo.EverythingIsNew.DAL.Vk
{
    public class VkClient
    {
        public static async Task<VkModel> GetUserData(string code, string vkAppId, string vkAppSecretKey, string redirectUri)
        {
            VkModel userData;
            AccessData accessData;
            using (var client = new WebClient())
            {
                client.UseDefaultCredentials = true;
                client.Headers["Content-Type"] = "application/json";
                client.Encoding = Encoding.UTF8;
                client.CachePolicy = new RequestCachePolicy(RequestCacheLevel.NoCacheNoStore);

                string urlToGetAccessData = null;
                string urlToGetInfo = null;

                try
                {
                    VkEvents.Log.GetAccessDataStarted(code, vkAppId, vkAppSecretKey, redirectUri);

                    urlToGetAccessData = VkHelpers.GetTokenUrl(code, vkAppId, vkAppSecretKey, redirectUri);
                    var accessInfo = client.DownloadString(urlToGetAccessData);
                    accessData = JsonConvert.DeserializeObject<AccessData>(accessInfo);

                    VkEvents.Log.GetAccessDataFinished(accessData);
                }
                catch (Exception e)
                {
                    VkEvents.Log.GeneralExceptionError(urlToGetAccessData, e);
                    throw;
                }

                try
                {
                    VkEvents.Log.GetUserDataStarted(accessData);

                    urlToGetInfo = VkHelpers.UserApiUrl(accessData);
                    var userInfo = client.DownloadString(urlToGetInfo);

                    var birthdayStart = userInfo.IndexOf("bdate\":\"");
                    var mySubstring = userInfo.Substring(birthdayStart+8, 11);
                    var endPosition = mySubstring.IndexOf("\"");
                    if (endPosition < 8)
                    {
                        userInfo = userInfo.Insert(birthdayStart + 8 + endPosition, ".2015");
                    }


                    var converter = new IsoDateTimeConverter { Culture = new CultureInfo("ru-RU")};
                    userData = JsonConvert.DeserializeObject<VkModel>(userInfo, converter);
                    userData.Response.FirstOrDefault().Email = accessData.Email;

                    VkEvents.Log.GetUserDataFinished(userData);
                    return userData;
                }
                catch (Exception e)
                {
                    VkEvents.Log.GeneralExceptionError(urlToGetInfo, e);
                    throw;
                }
            }

        }
    }
}
