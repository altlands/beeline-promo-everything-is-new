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
    public class CbnEvents
    {
        private static readonly Lazy<ICbnEvents> _log = new Lazy<ICbnEvents>(
            () =>
            {
                TraceParameterProvider.Default.For<ICbnEvents>().AddActivityIdContext();
                return EventSourceImplementer.GetEventSourceAs<ICbnEvents>();
            });

        public static ICbnEvents Log
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
