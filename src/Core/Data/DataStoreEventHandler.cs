using System;
using System.Threading;
using EPiServer.Events;
using EPiServer.Events.Clients;
using EPiServer.Logging;
using EPiServer.PlugIn;
using Knowit.NotFound.Core.CustomRedirects;

namespace Knowit.NotFound.Core.Data
{

    public class DataStoreEventHandlerHook : PlugInAttribute
    {
        private static readonly ILogger _log = LogManager.GetLogger(typeof(DataStoreEventHandlerHook));
        private static readonly Guid DataStoreUpdateEventId = new Guid("{96FE2985-D4C6-4879-85B5-DCAC7DA89713}");
        private static readonly Guid DataStoreUpdateRaiserId = new Guid("{832C2FA6-153D-4281-91A6-384457202708}");

        public static void Start()
        {
            try
            {
                if (Event.EventsEnabled)
                {

                    _log.Debug("Begin: Initializing Data Store Invalidation Handler on '{0}'", Environment.MachineName);

                    _log.Debug("Domain ID: '{0}', Friendly Name: '{1}', Basedir: '{2}', Thread: '{3}'",
                        AppDomain.CurrentDomain.Id.ToString(),
                        AppDomain.CurrentDomain.FriendlyName,
                        AppDomain.CurrentDomain.BaseDirectory,
                        Thread.CurrentThread.ManagedThreadId.ToString());
                    // Listen to events
                    Event dataStoreInvalidationEvent = Event.Get(DataStoreUpdateEventId);
                    dataStoreInvalidationEvent.Raised += dataStoreInvalidationEvent_Raised;

                    _log.Debug("End: Initializing Data Store Invalidation Handler on '{0}'", Environment.MachineName);

                }
                else
                    _log.Debug("NOT Initializing Data Store Invalidation Handler on '{0}'. Events are disabled for this site.", Environment.MachineName);
            }
            catch (Exception ex)
            {
                _log.Error("Cannot Initialize Data Store Invalidation Handler Correctly", ex);
            }
        }

        static void dataStoreInvalidationEvent_Raised(object sender, EventNotificationEventArgs e)
        {
            _log.Debug("dataStoreInvalidationEvent '{2}' handled - raised by '{0}' on '{1}'", e.RaiserId, Environment.MachineName, e.EventId);
            _log.Debug("Begin: Clearing cache on '{0}'", Environment.MachineName);
            CustomRedirectHandler.ClearCache();
            _log.Debug("End: Clearing cache on '{0}'", Environment.MachineName);

        }
        public static void DataStoreUpdated()
        {
            // File is changing, notify the other servers
            Event dataStoreInvalidateEvent = Event.Get(DataStoreUpdateEventId);
            // Raise event
            dataStoreInvalidateEvent.Raise(DataStoreUpdateRaiserId, null);

        }

    }

}
