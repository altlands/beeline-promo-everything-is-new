using Newtonsoft.Json;
using Promo.EverythingIsNew.DAL.Cbn.Dto;
using Promo.EverythingIsNew.DAL.Dcp;
using Promo.EverythingIsNew.DAL.Vk;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;

namespace Promo.EverythingIsNew.WebApp.Models
{
    public class Helpers
    {
        public static TariffGroupViewModel MapTariffGroup(int id, IEnumerable<AltLanDS.Beeline.DpcProxy.Client.Dpc.ProductParameter> lines)
        {
            return new TariffGroupViewModel
            {
                Id = id,
                Name = lines.FirstOrDefault().Group.Title,
                SortOrder = lines.FirstOrDefault().Group.SortOrder,
                Lines = lines.Select(l => MapTariffLine(l)).OrderBy(s => s.SortOrder).ToList()
            };
        }

        public static TariffLineViewModel MapTariffLine(AltLanDS.Beeline.DpcProxy.Client.Dpc.ProductParameter l)
        {
            return new TariffLineViewModel
            {
                Title = l.Title,
                NumValue = l.NumValue.ToString(),
                UnitDisplay = (l.Unit != null) ? l.Unit.Display : null,
                Value = l.Value,
                SortOrder = l.SortOrder
            };
        }

        public static UserProfileViewModel MapToUserProfileViewModel(VkModel userData)
        {
            var model = userData.Response.Select(x =>
                new UserProfileViewModel
                {
                    Uid = x.Id,
                    Academy = x.Academy,
                    Birthday = x.Birthday,
                    Day = x.Birthday.Day,
                    Month = x.Birthday.Month,
                    Year = x.Birthday.Year == DateTime.Now.Year ? DateTime.Now.Year - 10 : x.Birthday.Year,
                    SelectMyCity = x.City.Title,
                    Email = x.Email,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    CTN = !string.IsNullOrEmpty(x.Phone) ? x.Phone.Replace("(", "").Replace(")", "").Replace("-", "").Trim().Substring(Math.Max(0, x.Phone.Length - 10)) : null,
                }).FirstOrDefault();
            return model;
        }

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

        public static Update MapToUpdate(UserProfileViewModel userProfile)
        {
            return new Update
            {
                birth_date = userProfile.Birthday.ToString(),
                ctn = userProfile.CTN,
                email = userProfile.Email,
                email_unsubscribe = (!userProfile.IsMailingAgree),
                name = userProfile.LastName,
                surname = userProfile.LastName,
                region = userProfile.SelectMyCity
            };
        }

        public static Message MapToMessage(UserProfileViewModel userProfile)
        {
            return new Message
            {
                ctn = userProfile.CTN,
                uid = userProfile.Uid,
                email = userProfile.Email,
            };
        }

        internal static async Task<MessageResult> SendUserProfileToCbn(UserProfileViewModel userProfile)
        {
            MessageResult result;
            if (userProfile.IsPersonalDataAgree == true)
            {
                result = await MvcApplication.CbnClient.PostMessage(Helpers.MapToMessage(userProfile));
            }
            else
            {
                var ctnOnlyUserProfile = new UserProfileViewModel { CTN = userProfile.CTN };
                result = await MvcApplication.CbnClient.PostMessage(Helpers.MapToMessage(userProfile));
            }

            return result;
        }

        public static OfferViewModel GetOfferViewModel(UserProfileViewModel userProfile)
        {
            var targetTarif = DcpClient.GetTariff(MvcApplication.dcpConnectionString, userProfile.Soc, userProfile.MarketCode);

            var groups = targetTarif.DpcProduct.Parameters
                    .GroupBy(g => g.Group.Id, (id, lines) => Helpers.MapTariffGroup(id, lines))
                    .OrderBy(s => s.SortOrder).ToList();

            var model = new OfferViewModel
            {
                UserName = userProfile.FirstName,
                TariffName = targetTarif.DpcProduct.MarketingProduct.Title,
                Groups = groups
            };
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

            if (birthday.AddYears(MvcApplication.MinimumAge) <= DateTime.Now &&
                birthday.AddYears(MvcApplication.MaximumAge) >= DateTime.Now)
            {
                return true;
            }
            return false;
        }

        internal static void RestoreUserProfile(ControllerContext controllerContext, UserProfileViewModel userProfile)
        {
            var oldUserProfile = Helpers.DecodeFromCookies(controllerContext);
            userProfile.Uid = oldUserProfile.Uid;
            userProfile.MarketCode = Helpers.GetMarketCodeFromCity(userProfile.SelectMyCity);
            userProfile.Soc = Helpers.GetSocFromCity(userProfile.SelectMyCity);
            userProfile.Birthday = new DateTime(userProfile.Year, userProfile.Month, userProfile.Day);
        }

        internal static ActionResult CheckRegionIsAllowed(UserProfileViewModel userProfile, ActionResult result)
        {
            if (userProfile.SelectMyCity == MvcApplication.OtherRegion)
            {
                result = new RedirectResult(MvcApplication.PersonalBeelineUrl);
            }

            return result;
        }

        internal static ActionResult CheckAgeIsAllowed(UserProfileViewModel userProfile, ActionResult result)
        {
            if (!Helpers.IsAgeAllowed(userProfile.Birthday ?? new DateTime()))
            {
                result = new RedirectResult(MvcApplication.PersonalBeelineUrl);
            }

            return result;
        }

        internal static ActionResult CheckPersonalDataIsAllowed(UserProfileViewModel userProfile, ActionResult result)
        {
            if (!userProfile.IsPersonalDataAgree)
            {
                result = new RedirectResult(MvcApplication.PersonalBeelineUrl);
            }

            return result;
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