using Promo.EverythingIsNew.DAL.Cbn;
using Promo.EverythingIsNew.DAL.Cbn.Dto;
using Promo.EverythingIsNew.DAL.Vk;
using System;
using System.Diagnostics.Tracing;

namespace Promo.EverythingIsNew.DAL.Events
{
    public interface IVkEvents
    {
        [Event(500, Level = EventLevel.Error, Message = "Error during service call")]
        void GeneralError(Exception e);

        [Event(1, Level = EventLevel.Error, Message = "Error during VK Api call")]
        void GeneralExceptionError(string url, Exception e);

        [Event(2, Level = EventLevel.Informational, Message = "Getting access data started")]
        void GetAccessDataStarted(string code, string vkAppId, string vkAppSecretKey, string redirectUri);
        [Event(3, Level = EventLevel.Informational, Message = "Getting access data finished")]
        void GetAccessDataFinished(AccessData accessData);

        [Event(4, Level = EventLevel.Informational, Message = "Getting user data started")]
        void GetUserDataStarted(AccessData accessData);

        [Event(5, Level = EventLevel.Informational, Message = "Getting user data finished")]
        void GetUserDataFinished(VkModel userData);

        [Event(6, Level = EventLevel.Informational, Message = "Getting code started")]
        void GetCodeStarted(string vkAppId, string redirectUri);

        [Event(7, Level = EventLevel.Informational, Message = "Getting code finished")]
        void GetCodeFinished(string code);

    }
}
