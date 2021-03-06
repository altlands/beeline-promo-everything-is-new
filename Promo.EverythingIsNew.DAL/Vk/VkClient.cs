﻿using Newtonsoft.Json;
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
        public static VkModel GetUserData(string code, string vkAppId, string vkAppSecretKey, string redirectUri)
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
                    ErrorEvents.Log.VkGeneralExceptionError(urlToGetAccessData, e);
                    throw;
                }

                try
                {
                    VkEvents.Log.GetUserDataStarted(accessData);

                    urlToGetInfo = VkHelpers.UserApiUrl(accessData);
                    var userInfo = client.DownloadString(urlToGetInfo);

                    userInfo = VkHelpers.FixEmptyYear(userInfo);

                    var converter = new IsoDateTimeConverter { Culture = new CultureInfo("ru-RU") };
                    userData = JsonConvert.DeserializeObject<VkModel>(userInfo, converter);
                    userData.Response.FirstOrDefault().Email = accessData.Email;

                    VkEvents.Log.GetUserDataFinished(userData);
                    return userData;
                }
                catch (Exception e)
                {
                    ErrorEvents.Log.VkGeneralExceptionError(urlToGetInfo, e);
                    throw;
                }
            }
        }
    }
}
