using Promo.EverythingIsNew.DAL.Cbn.Dto;
using Promo.EverythingIsNew.WebApp.Models;
using System;
using System.Web.Mvc;

namespace Promo.EverythingIsNew.WebApp.Helpers
{
    public class RedirectHelpers
    {
        internal static ActionResult RedirectByRegion(UserProfileViewModel userProfile, ActionResult result)
        {
            if (userProfile.SelectMyCity == MvcApplication.OtherRegion)
            {
                result = new RedirectResult(MvcApplication.PersonalBeelineUrl);
            }

            return result;
        }

        internal static ActionResult RedirectByAge(UserProfileViewModel userProfile, ActionResult result)
        {
            if (!CommonHelpers.IsAgeAllowed(userProfile.Birthday ?? new DateTime()))
            {
                result = new RedirectResult(MvcApplication.PersonalBeelineUrl);
            }

            return result;
        }

        internal static ActionResult RedirectOnPersonalDataAgreement(UserProfileViewModel userProfile, ActionResult result)
        {
            if (!userProfile.IsPersonalDataAgree)
            {
                result = new RedirectResult(MvcApplication.PersonalBeelineUrl);
            }

            return result;
        }

        internal static ActionResult RedirectOnCtnAndUidAlreadyUsed(StatusResult statusResult, ActionResult result)
        {
            if (statusResult.is_used_ctn ^ statusResult.is_used_uid)
            {
                result = new RedirectResult(MvcApplication.PersonalBeelineUrl);
            }

            if ((statusResult.is_used_ctn && statusResult.is_used_uid) == true && !statusResult.Is_used_uid_with_ctn)
            {
                result = new RedirectResult(MvcApplication.PersonalBeelineUrl);
            }
            return result;
        }

    }
}