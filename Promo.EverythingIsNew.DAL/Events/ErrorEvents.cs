using System;
using AltLanDS.Common.Events;
using EventSourceProxy;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Promo.EverythingIsNew.DAL.Events
{
    public class ErrorEvents
    {
        private static readonly Lazy<IErrorEvents> _log = new Lazy<IErrorEvents>(
            () =>
            {
                TraceParameterProvider.Default.For<IErrorEvents>().AddActivityIdContext();
                return EventSourceImplementer.GetEventSourceAs<IErrorEvents>();
            });

        public static IErrorEvents Log
        {
            get
            {
                return _log.Value;
            }
        }

        public static EventSource LogEventSource
        {
            get
            {
                return _log.Value as EventSource;
            }
        }
    }
}
