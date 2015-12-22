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
    public class DpcEvents
    {
        private static readonly Lazy<IDpcEvents> _log = new Lazy<IDpcEvents>(
            () =>
            {
                TraceParameterProvider.Default.For<IDpcEvents>().AddActivityIdContext();
                return EventSourceImplementer.GetEventSourceAs<IDpcEvents>();
            });

        public static IDpcEvents Log
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
