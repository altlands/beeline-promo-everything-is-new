using Promo.EverythingIsNew.DAL.Cbn;
using Promo.EverythingIsNew.DAL.Cbn.Dto;
using Promo.EverythingIsNew.DAL.Vk;
using System;
using System.Diagnostics.Tracing;

namespace Promo.EverythingIsNew.DAL.Events
{
    public interface IErrorEvents
    {
        [Event(1, Level = EventLevel.Error, Message = "Error while using Cbn Client")]
        void CbnGeneralExceptionError(string method, CbnException response);
        [Event(2, Level = EventLevel.Error, Message = "Error during VK Api call")]
        void VkGeneralExceptionError(string url, Exception e);
        [Event(3, Level = EventLevel.Error, Message = "Error during DPC call")]
        void DpcConnectionGeneralExceptionError(Exception e);
        [Event(4, Level = EventLevel.Error, Message = "Tariff not found")]
        void DpcTafirrNotFoundError();
        [Event(500, Level = EventLevel.Error, Message = "Error during service call")]
        void GeneralError(Exception e);

    }
}
