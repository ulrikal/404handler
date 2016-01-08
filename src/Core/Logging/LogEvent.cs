using System;

namespace Knowit.NotFound.Core.Logging
{
    public class LogEvent
    {

        public LogEvent(string oldUrl, DateTime requested, string referer, int siteId)
        {
            OldUrl = oldUrl;
            Requested = requested;
            Referer = referer;
            SiteId = siteId;
        }

        public string OldUrl { get; set; }
        public DateTime Requested { get; set; }
        public string Referer { get; set; }

        public int SiteId { get; set; }
    }
}