using AltLanDS.Beeline.DpcProxy.Client;
using Newtonsoft.Json;
using Promo.EverythingIsNew.DAL.Cbn;
using Promo.EverythingIsNew.DAL.Cbn.Dto;
using Promo.EverythingIsNew.DAL.Events;
using Promo.EverythingIsNew.DAL.Vk;
using Promo.EverythingIsNew.Domain;
using Promo.EverythingIsNew.WebApp.Models;
using System;
using System.Configuration;
using System.Linq;
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
                !Helpers.IsAgeAllowed(userProfile.Birthday ?? new DateTime()))
            {
                return Redirect(MvcApplication.PersonalBeelineUrl);
            }

            var cities = Helpers.GetCities();
            ViewBag.Cities = cities;
            ViewBag.SelectedCity = cities.FirstOrDefault(x => x == userProfile.SelectMyCity) ?? cities[0];
            return View(userProfile);
        }

        [HttpPost]
        public async Task<ActionResult> Index(UserProfileViewModel userProfile)
        {
            var oldUserProfile = Helpers.DecodeFromCookies(this.ControllerContext);
            userProfile.Uid = oldUserProfile.Uid;
            userProfile.MarketCode = Helpers.GetMarketCodeFromCity(userProfile.SelectMyCity);
            userProfile.Soc = Helpers.GetSocFromCity(userProfile.SelectMyCity);
            userProfile.Birthday = new DateTime(userProfile.Year, userProfile.Month, userProfile.Day);


            // var result = await MvcApplication.CbnClient.Update(Helpers.MapToUpdate(userProfile));
            // Add ModelState validation messages
            // return index page if ModelState is not valid

            // post to cbn api status and redirect if account is already used or if 

            // check age
            if (!Helpers.IsAgeAllowed(userProfile.Birthday ?? new DateTime()))
            {
                return Redirect(MvcApplication.PersonalBeelineUrl);
            }

            if (userProfile.SelectMyCity == "Другой регион")
            {
                return Redirect(MvcApplication.PersonalBeelineUrl);
            }

            Helpers.EncodeToCookies(userProfile, this.ControllerContext);
            return RedirectToAction("Offer");
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

        public async Task<ActionResult> Test()
        {

            var result = await MvcApplication.CbnClient.Update(new Update
            {
                ctn = "777",
                email = "777",
                //name = "777",
            });

            //var result = await MvcApplication.CbnClient.GetStatus(new Status
            //{
            //    ctn = "9653868754",
            //    uid = "1083308",
            //});


            return Content(JsonConvert.SerializeObject(result));
        }


    }
}