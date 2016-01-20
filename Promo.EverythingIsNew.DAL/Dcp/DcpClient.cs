using AltLanDS.Beeline.DpcProxy.Client;
using AltLanDS.Beeline.DpcProxy.Client.Domain;
using System.Linq;

namespace Promo.EverythingIsNew.DAL.Dcp
{
    public class DcpClient
    {
        public static MobileTariff GetTariff(string connectionString, string soc, string marketCode)
        {
            using (var db = new DpcProxyDbContext(connectionString)) // unity per call
            {
                //var targetTarif = Db.MobileTariffs.FirstOrDefault(t => t.SocName == "12_VSE4M" && t.Regions.Any(r => r.MarketCode == "MarketCode"));
                var targetTarif = db.MobileTariffs.FirstOrDefault(t => t.SocName == soc);
                return targetTarif;
            }
        }
    }
}
