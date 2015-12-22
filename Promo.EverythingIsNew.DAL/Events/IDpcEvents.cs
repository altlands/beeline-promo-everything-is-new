using AltLanDS.Beeline.DpcProxy.Client.Domain;
using Promo.EverythingIsNew.DAL.Cbn;
using Promo.EverythingIsNew.DAL.Cbn.Dto;
using Promo.EverythingIsNew.DAL.Vk;
using System;
using System.Diagnostics.Tracing;

namespace Promo.EverythingIsNew.DAL.Events
{
    public interface IDpcEvents
    {



        [Event(10, Level = EventLevel.Informational, Message = "Getting tariff started")]
        void GetTariffStarted(string socName, string marketCode);

        [Event(11, Level = EventLevel.Informational, Message = "Getting tariff ended")]
        void GetTariffEnded(MobileTariff mobileTariff);









    }
}
