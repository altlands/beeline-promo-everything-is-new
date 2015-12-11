using AltLanDS.Beeline.DpcProxy.Client;
using Promo.EverythingIsNew.DAL.Events;
using Promo.EverythingIsNew.DAL.Vk;
using Promo.EverythingIsNew.WebApp.Models;
using Promo.EverythingIsNew.WebApp.Helpers;
using System;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace Promo.EverythingIsNew.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public DpcProxyDbContext Db;

        public ActionResult Choose()
        {
            ViewBag.PersonalBeelineUrl = MvcApplication.PersonalBeelineUrl;
            return View();
        }

        public ActionResult Vk()
        {
            string urlToGetCode = null;

            try
            {
                VkEvents.Log.GetCodeStarted(MvcApplication.VkAppId, MvcApplication.RedirectUri);
                urlToGetCode = VkHelpers.GetCodeUrl(MvcApplication.VkAppId, MvcApplication.RedirectUri);
                return Redirect(urlToGetCode);
            }
            catch (Exception e)
            {
                VkEvents.Log.GeneralExceptionError(urlToGetCode, e);
                throw;
            }
        }

        public ActionResult VkResult(string code, string error)
        {
            VkEvents.Log.GetCodeFinished(code);
            try
            {
                if (!string.IsNullOrEmpty(code))
                {
                    UserProfileViewModel userProfile = MappingHelpers.MapToUserProfileViewModel(VkClient.GetUserData(
                        code, MvcApplication.VkAppId, MvcApplication.VkAppSecretKey, MvcApplication.RedirectUri));
                    CommonHelpers.EncodeToCookies(userProfile, this.ControllerContext);
                    return RedirectToAction("Index");
                }
                else if (!string.IsNullOrEmpty(error))
                {
                    return Redirect(MvcApplication.PersonalBeelineUrl);
                }
                else
                {
                    return RedirectToAction("Vk");
                }
            }
            catch (Exception e)
            {
                VkEvents.Log.GeneralError(e);
                throw;
            }
        }

        public ActionResult Index()
        {
            var userProfile = CommonHelpers.DecodeFromCookies(this.ControllerContext);

            // if birthsday year is not provided  it is necessary to compare with the current year
            if (userProfile.Birthday != null &&
                userProfile.Birthday.Value.Year != DateTime.Now.Year &&
                !CommonHelpers.IsAgeAllowed(userProfile.Birthday))
            {
                return Redirect(MvcApplication.PersonalBeelineUrl);
            }

            var cities = CommonHelpers.GetCities();
            ViewBag.Cities = cities;
            ViewBag.SelectedCity = CommonHelpers.CalculateSelectedCity(userProfile, cities);
            return View(userProfile);
        }


        [HttpPost]
        public async Task<ActionResult> Index(UserProfileViewModel userProfile)
        {
            ActionResult result = RedirectToAction("Offer");
            CommonHelpers.RestoreUserProfile(this.ControllerContext, userProfile);

            result = RedirectHelpers.RedirectByAge(userProfile, result);

            result = RedirectHelpers.RedirectByRegion(userProfile, result);

            result = RedirectHelpers.RedirectOnPersonalDataAgreement(userProfile, result);

            var updateResult = await CommonHelpers.SendUserProfileToCbn(userProfile);
            // Add ModelState validation messages
            // return index page if ModelState is not valid

            var statusResult = await MvcApplication.CbnClient.GetStatus(MappingHelpers.MapToStatus(userProfile));
            result = RedirectHelpers.RedirectOnCtnAndUidAlreadyUsed(statusResult, result);

            result = RedirectHelpers.RedirectOnBeelineCtn(statusResult, result);

            CommonHelpers.EncodeToCookies(userProfile, this.ControllerContext);
            return result;
        }



        public ActionResult Offer()
        {
            if (MvcApplication.IsStubMode)
            {
                return View(CommonHelpers.GetOfferViewModel(new UserProfileViewModel{
                    FirstName = "Иван",
                    Soc = "03_ALLFY",
                    MarketCode = "UFA"
                }));
            }
            var userProfile = CommonHelpers.DecodeFromCookies(this.ControllerContext);
            OfferViewModel model = CommonHelpers.GetOfferViewModel(userProfile);

            return View(model);
        }

        [HttpPost]
        [ActionName("Offer")]
        public async Task<JsonResult> OfferPost(string email)
        {
            if (MvcApplication.IsStubMode)
            {
                return Json(new Promo.EverythingIsNew.DAL.Cbn.Dto.MessageResult {
                    is_message_sent = true,
                    description = "description",
                    is_change_email_available = true,
                    sended_on_email = "email"
                });
            }

            var userProfile = CommonHelpers.DecodeFromCookies(this.ControllerContext);
            if (!string.IsNullOrEmpty(email))
            {
                userProfile.Email = email;
            }
            var result = await MvcApplication.CbnClient.PostMessage(MappingHelpers.MapToMessage(userProfile));
            result.sended_on_email = userProfile.Email;
            return Json(result);
        }
    }
}
