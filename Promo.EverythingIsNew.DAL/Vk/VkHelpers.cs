using System;


namespace Promo.EverythingIsNew.DAL.Vk
{
    public class VkHelpers
    {
        public static string UserApiUrl(AccessData accessData)
        {
            var urlToGetInfo = "https://api.vk.com/method/users.get?user_id=" + accessData.UserId + "&fields=bdate,city,education,contacts&v=5.37&access_token=" + accessData.AccessToken + "&x=" + DateTime.Now.Ticks;
            return urlToGetInfo;
        }

        public static string GetTokenUrl(string code, string vkAppId, string vkAppSecretKey, string redirectUri)
        {
            var urlToGetAccessData = "https://oauth.vk.com/access_token?client_id=" + vkAppId + "&client_secret=" + vkAppSecretKey + "&redirect_uri=" + redirectUri + "&code=" + code + "&x=" + DateTime.Now.Ticks;
            return urlToGetAccessData;
        }

        public static string GetCodeUrl(string vkAppId, string redirectUri)
        {
            var urlToGetCode = "https://oauth.vk.com/authorize?client_id=" + vkAppId + "&display=page&redirect_uri=" + redirectUri + "&scope=email&response_type=code&v=5.37" + "&x=" + DateTime.Now.Ticks;
            return urlToGetCode;
        }

        public static string FixEmptyYear(string userInfo)
        {
            var currentYear = DateTime.Now.Year;

            var birthdayStart = userInfo.IndexOf("bdate\":\"");
            var mySubstring = userInfo.Substring(birthdayStart + 8, 11);
            var endPosition = mySubstring.IndexOf("\"");
            if (endPosition != -1 && endPosition < 8)
            {
                userInfo = userInfo.Insert(birthdayStart + 8 + endPosition, ".1804");
            }

            return userInfo;
        }
    }
}
