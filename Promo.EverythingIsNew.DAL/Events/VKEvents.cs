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
    public class VkEvents
    {
        private static readonly Lazy<IVkEvents> _log = new Lazy<IVkEvents>(
            () =>
            {
                TraceParameterProvider.Default.For<IVkEvents>().AddActivityIdContext();
                return EventSourceImplementer.GetEventSourceAs<IVkEvents>();
            });

        public static IVkEvents Log
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
