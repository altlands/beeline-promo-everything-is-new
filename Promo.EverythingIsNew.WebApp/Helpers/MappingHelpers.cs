using Promo.EverythingIsNew.DAL.Cbn.Dto;
using Promo.EverythingIsNew.DAL.Vk;
using Promo.EverythingIsNew.WebApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Promo.EverythingIsNew.WebApp.Helpers
{
    public class MappingHelpers
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
                Title = l.Title == null ? null : l.Title.Replace("href=\"/", "href=\"https://beeline.ru/"),
                NumValue = l.NumValue.ToString(),
                UnitDisplay = (l.Unit != null) ? l.Unit.Display : null,
                Value = l.Value == null ? null : l.Value.Replace("href=\"/", "href=\"https://beeline.ru/"),
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
                    CTN = x.Phone
                }).FirstOrDefault();
            model.CTN = !string.IsNullOrEmpty(model.CTN) ? model.CTN.Replace("(", "").Replace(")", "").Replace("-", "").Trim() : "";
            model.CTN = model.CTN.Substring(Math.Max(0, model.CTN.Length - 10));
            return model;
        }

        public static Status MapToStatus(UserProfileViewModel userProfile)
        {
            return new Status
            {
                ctn = userProfile.CTN,
                uid = userProfile.Uid,
            };
        }

        public static Update MapToUpdate(UserProfileViewModel userProfile)
        {
            return new Update
            {
                birth_date = userProfile.Birthday.ToString(),
                ctn = userProfile.CTN,
                email = userProfile.Email,
                email_unsubscribe = (!userProfile.IsMailingAgree),
                name = userProfile.FirstName,
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
                market = userProfile.MarketCode
            };
        }
    }
}