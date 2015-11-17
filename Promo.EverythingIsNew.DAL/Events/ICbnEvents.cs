using Promo.EverythingIsNew.DAL.Cbn;
using Promo.EverythingIsNew.DAL.Cbn.Dto;
using System.Diagnostics.Tracing;
using System.Net.Http;

namespace Promo.EverythingIsNew.DAL.Events
{
    public interface ICbnEvents
    {
        [Event(1, Level = EventLevel.Error, Message = "Error while using Cbn Client")]
        void CbnGeneralExceptionError(string method, CbnException response);

        [Event(2, Level = EventLevel.Informational, Message = "Cbn getting status started")]
        void CbnGetStatusStarted(Status status);

        [Event(3, Level = EventLevel.Informational, Message = "Cbn getting status finished")]
        void CbnGetStatusFinished(StatusResult result);

        [Event(4, Level = EventLevel.Informational, Message = "Cbn posting message started")]
        void CbnPostMessageStarted(Message message);
        [Event(5, Level = EventLevel.Informational, Message = "Cbn posting message finished")]
        void CbnPostMessageFinished(MessageResult result);

        [Event(6, Level = EventLevel.Informational, Message = "Cbn updating started")]
        void CbnUpdateStarted(Update update);
        [Event(7, Level = EventLevel.Informational, Message = "Cbn updating finished")]
        void CbnUpdateFinished(UpdateResult result);

        [Event(8, Level = EventLevel.Informational, Message = "Test Cbn getting status started")]
        void TestCbnGetStatusStarted(Status status);
        [Event(9, Level = EventLevel.Error, Message = "Error while using Test Cbn Client")]
        void TestCbnGeneralExceptionError(string method, CbnException response);
        [Event(10, Level = EventLevel.Informational, Message = "Test Cbn response")]
        void TestCbnGetStatusResponse(HttpResponseMessage response);
        [Event(11, Level = EventLevel.Informational, Message = "Test Cbn result")]
        void TestCbnGetStatusFinished(StatusResult result);
    }
}
