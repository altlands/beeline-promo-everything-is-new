using AltLanDS.Beeline.DpcProxy.Client;
using Promo.EverythingIsNew.DAL.Events;
using Promo.EverythingIsNew.DAL.Vk;
using Promo.EverythingIsNew.WebApp.Models;
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

        public async Task<ActionResult> Vk()
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

        public async Task<ActionResult> VkResult(string code, string error)
        {
            VkEvents.Log.GetCodeFinished(code);
            try
            {
                if (!string.IsNullOrEmpty(code))
                {
                    UserProfileViewModel userProfile = Helpers.MapToUserProfileViewModel(await VkClient.GetUserData(
                        code, MvcApplication.VkAppId, MvcApplication.VkAppSecretKey, MvcApplication.RedirectUri));
                    Helpers.EncodeToCookies(userProfile, this.ControllerContext);
                    return RedirectToAction("Index");
                }
                else
                {
                    return Redirect(MvcApplication.PersonalBeelineUrl);
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
            var userProfile = Helpers.DecodeFromCookies(this.ControllerContext);

            // if birthsday year is not provided  it is necessary to compare with the current year
            if (userProfile.Birthday != null &&
                userProfile.Birthday.Value.Year != DateTime.Now.Year &&
                !Helpers.IsAgeAllowed(userProfile.Birthday))
            {
                return Redirect(MvcApplication.PersonalBeelineUrl);
            }

            var cities = Helpers.GetCities();
            ViewBag.Cities = cities;
            ViewBag.SelectedCity = Helpers.CalculateSelectedCity(userProfile, cities);
            return View(userProfile);
        }


        [HttpPost]
        public async Task<ActionResult> Index(UserProfileViewModel userProfile)
        {
            ActionResult result = RedirectToAction("Offer");
            Helpers.RestoreUserProfile(this.ControllerContext, userProfile);

            result = Helpers.CheckAgeIsAllowed(userProfile, result);

            result = Helpers.CheckRegionIsAllowed(userProfile, result);

            result = Helpers.CheckPersonalDataIsAllowed(userProfile, result);

            var updateResult = await Helpers.SendUserProfileToCbn(userProfile);
            // Add ModelState validation messages
            // return index page if ModelState is not valid

            var statusResult = await MvcApplication.CbnClient.GetStatus(Helpers.MapToStatus(userProfile));
            result = Helpers.CheckCtnUidAlreadyUsed(statusResult, result);

            Helpers.EncodeToCookies(userProfile, this.ControllerContext);
            return result;
        }



        public async Task<ActionResult> Offer()
        {
            var userProfile = Helpers.DecodeFromCookies(this.ControllerContext);
            OfferViewModel model = Helpers.GetOfferViewModel(userProfile);

            return View(model);
        }

        [HttpPost]
        [ActionName("Offer")]
        public async Task<ActionResult> OfferPost(string email)
        {
            var userProfile = Helpers.DecodeFromCookies(this.ControllerContext);
            if (!string.IsNullOrEmpty(email))
            {
                userProfile.Email = email;
            }
            var result = await MvcApplication.CbnClient.PostMessage(Helpers.MapToMessage(userProfile));
            if (result.is_message_sent == true)
            {
                return Content(userProfile.Email);
            }
            else
            {
                return new HttpStatusCodeResult(400);
            }
        }
    }
}