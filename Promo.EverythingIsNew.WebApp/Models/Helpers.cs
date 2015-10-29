﻿using Newtonsoft.Json;
using Promo.EverythingIsNew.DAL.Cbn.Dto;
using Promo.EverythingIsNew.DAL.Dcp;
using Promo.EverythingIsNew.DAL.Vk;
using Promo.EverythingIsNew.Domain;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
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

        public static EntryForm MapToEntryForm(VkModel userData)
        {
            var model = userData.Response.Select(x =>
                new EntryForm
                {
                    Uid = x.Id,
                    Academy = x.Academy,
                    Birthday = x.Birthday,
                    SelectMyCity = x.City.Title,
                    Email = x.Email,
                    FirstName = x.FirstName,
                    LastName = x.LastName,
                    CTN = x.Phone,
                }).FirstOrDefault();
            return model;
        }

        public static void EncodeToCookies(EntryForm userProfile, ControllerContext controllerContext)
        {
            var cookie = new HttpCookie("UserProfile");
            var json = JsonConvert.SerializeObject(userProfile);
            var bytes = Encoding.Unicode.GetBytes(json);
            var encoded = MachineKey.Protect(bytes);
            var base64 = Convert.ToBase64String(encoded);
            cookie.Value = base64;
            controllerContext.HttpContext.Response.Cookies.Add(cookie);
        }

        public static EntryForm DecodeFromCookies(ControllerContext controllerContext)
        {
            if (controllerContext.HttpContext.Request.Cookies.AllKeys.Contains("UserProfile"))
            {
                var cookie = controllerContext.HttpContext.Request.Cookies["UserProfile"];
                var encoded = Convert.FromBase64String(cookie.Value);
                var decoded = MachineKey.Unprotect(encoded);
                var json = Encoding.Unicode.GetString(decoded);
                EntryForm userProfile = JsonConvert.DeserializeObject<EntryForm>(json);
                return userProfile;
            }
            return null;
        }

        public static Update MapToUpdate(EntryForm userProfile)
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

        public static Message MapToMessage(EntryForm userProfile)
        {
            return new Message
            {
                ctn = userProfile.CTN,
                uid = userProfile.Uid,
                email = userProfile.Email,
            };
        }

        public static OfferViewModel GetOfferViewModel(string userFirstName)
        {
            var targetTarif = DcpClient.GetTariff(MvcApplication.dcpConnectionString, MvcApplication.Soc);

            var groups = targetTarif.DpcProduct.Parameters
                    .GroupBy(g => g.Group.Id, (id, lines) => Helpers.MapTariffGroup(id, lines))
                    .OrderBy(s => s.SortOrder).ToList();

            var model = new OfferViewModel
            {
                UserName = userFirstName,
                TariffName = targetTarif.DpcProduct.MarketingProduct.Title,
                Groups = groups
            };
            return model;
        }

        public static Dictionary<string, string> GetMarketCodes()
        {
            NameValueCollection section = (NameValueCollection)ConfigurationManager.GetSection("MarketCodes");
            Dictionary<string, string> dictionary = new Dictionary<string, string>();
            foreach (var k in section.AllKeys)
            {
                dictionary.Add(k, section[k]);
            }
            return dictionary;
        }
    }
}