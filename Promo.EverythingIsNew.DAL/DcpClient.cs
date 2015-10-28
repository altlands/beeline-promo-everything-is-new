﻿using AltLanDS.Beeline.DpcProxy.Client;
using AltLanDS.Beeline.DpcProxy.Client.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Promo.EverythingIsNew.DAL
{
    public class DcpClient
    {
        public static MobileTariff GetTariff(string connectionString, string Soc)
        {
            var db = new DpcProxyDbContext(); // unity per call

            //var targetTarif = Db.MobileTariffs.FirstOrDefault(t => t.SocName == "12_VSE4M" && t.Regions.Any(r => r.MarketCode == "MarketCode"));
            var targetTarif = db.MobileTariffs.FirstOrDefault(t => t.SocName == Soc);
            return targetTarif;
        }
    }
}
