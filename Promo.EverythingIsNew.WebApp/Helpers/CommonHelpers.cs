using AltLanDS.Beeline.DpcProxy.Client.Domain;
using Newtonsoft.Json;
using Promo.EverythingIsNew.DAL.Cbn.Dto;
using Promo.EverythingIsNew.DAL.Dcp;
using Promo.EverythingIsNew.DAL.Events;
using Promo.EverythingIsNew.WebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Promo.EverythingIsNew.WebApp.Helpers
{
    public class CommonHelpers
    {
        public static void EncodeToCookies(UserProfileViewModel userProfile, ControllerContext controllerContext)
        {
            var cookie = new HttpCookie("UserProfile");
            var json = JsonConvert.SerializeObject(userProfile);
            var bytes = Encoding.Unicode.GetBytes(json);
            var encoded = MachineKey.Protect(bytes);
            var base64 = Convert.ToBase64String(encoded);
            cookie.Value = base64;
            controllerContext.HttpContext.Response.Cookies.Add(cookie);
        }

        public static UserProfileViewModel DecodeFromCookies(ControllerContext controllerContext)
        {
            if (controllerContext.HttpContext.Request.Cookies.AllKeys.Contains("UserProfile"))
            {
                var cookie = controllerContext.HttpContext.Request.Cookies["UserProfile"];
                var encoded = Convert.FromBase64String(cookie.Value);
                var decoded = MachineKey.Unprotect(encoded);
                var json = Encoding.Unicode.GetString(decoded);
                UserProfileViewModel userProfile = JsonConvert.DeserializeObject<UserProfileViewModel>(json);
                return userProfile;
            }
            return null;
        }



        internal static async Task<UpdateResult> SendUserProfileToCbn(UserProfileViewModel userProfile)
        {
            UpdateResult result;
            if (userProfile.IsPersonalDataAgree == true)
            {
                result = await MvcApplication.CbnClient.Update(MappingHelpers.MapToUpdate(userProfile));
            }
            else
            {
                var ctnOnlyUserProfile = new UserProfileViewModel { CTN = userProfile.CTN };
                result = await MvcApplication.CbnClient.Update(MappingHelpers.MapToUpdate(ctnOnlyUserProfile));
            }

            return result;
        }

        public static OfferViewModel GetOfferViewModel(UserProfileViewModel userProfile)
        {
            OfferViewModel model = new OfferViewModel();
            List<TariffGroupViewModel> groups = new List<TariffGroupViewModel>();
            MobileTariff targetTarif;

            try
            {
                targetTarif = DcpClient.GetTariff(MvcApplication.dcpConnectionString, userProfile.Soc, userProfile.MarketCode);
            }
            catch (Exception e)
            {
                ErrorEvents.Log.DpcConnectionGeneralExceptionError(e);
                throw;
            }
            

            if (targetTarif != null)
            {
                groups = targetTarif.DpcProduct.Parameters
                        .GroupBy(g => g.Group.Id, (id, lines) => MappingHelpers.MapTariffGroup(id, lines))
                        .OrderBy(s => s.SortOrder).ToList();

                model = new OfferViewModel
                {
                    UserName = userProfile.FirstName,
                    Groups = groups
                };
                if (targetTarif.DpcProduct != null && targetTarif.DpcProduct.MarketingProduct != null)
                {
                    model.TariffName = targetTarif.DpcProduct.MarketingProduct.Title;
                }
            }
            else
            {
                ErrorEvents.Log.DpcTafirrNotFoundError();
            }
            return model;
        }

        public static List<string> GetCities()
        {
            var cities = new List<string>();
            foreach (var item in MvcApplication.TariffIndexes)
            {
                cities.Add(item.City);
            }
            cities.OrderBy(x => x).ToList();
            cities.Add(MvcApplication.OtherRegion);
            return cities;
        }

        internal static string GetMarketCodeFromCity(string city)
        {
            var items = from TariffIndexElement s in MvcApplication.TariffIndexes
                        where s.City == city
                        select s.MarketCode;
            return items.FirstOrDefault();
        }

        internal static string GetSocFromCity(string city)
        {
            var items = from TariffIndexElement s in MvcApplication.TariffIndexes
                        where s.City == city
                        select s.Soc;
            return items.FirstOrDefault();
        }

        public static bool IsAgeAllowed(DateTime? oldBirthday)
        {
            DateTime birthday = oldBirthday ?? new DateTime();

            if (birthday.Year == 1804)
            {
                return true;
            }

            if (birthday.AddYears(MvcApplication.MinimumAge) <= DateTime.Now &&
                birthday.AddYears(MvcApplication.MaximumAge) >= DateTime.Now)
            {
                return true;
            }
            return false;
        }

        internal static void RestoreUserProfile(ControllerContext controllerContext, UserProfileViewModel userProfile)
        {
            var oldUserProfile = CommonHelpers.DecodeFromCookies(controllerContext);
            userProfile.Uid = oldUserProfile.Uid;
            userProfile.MarketCode = CommonHelpers.GetMarketCodeFromCity(userProfile.SelectMyCity);
            userProfile.Soc = CommonHelpers.GetSocFromCity(userProfile.SelectMyCity);
            userProfile.Birthday = new DateTime(userProfile.Year, userProfile.Month, userProfile.Day);
        }

        internal static string CalculateSelectedCity(UserProfileViewModel userProfile, System.Collections.Generic.List<string> cities)
        {
            string result;
            if (userProfile.SelectMyCity == null)
            {
                result = cities.FirstOrDefault();
            }
            else
            {
                if (cities.FirstOrDefault(x => x == userProfile.SelectMyCity) != null)
                {
                    result = cities.FirstOrDefault(x => x == userProfile.SelectMyCity);
                }
                else
                {
                    result = cities[cities.Count - 1];
                }

            }
            return result;
        }
    }
}