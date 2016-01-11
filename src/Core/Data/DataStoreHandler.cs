using System.Collections.Generic;
using System.Linq;
using System.Web;
using EPiServer.Data.Dynamic;
using Knowit.NotFound.Core.Configuration;
using Knowit.NotFound.Core.CustomRedirects;
using Knowit.NotFound.Core.Logging;

namespace Knowit.NotFound.Core.Data
{
    public class DataStoreHandler
    {

        public enum State
        {
            Saved = 0,
            Suggestion = 1,
            Ignored = 2,
            Deleted
        };

        private const string OldUrlPropertyName = "OldUrl";
        private const string SiteIdPropertyName = "SiteId";
        public void SaveCustomRedirect(CustomRedirect currentCustomRedirect)
        {
            // Get hold of the datastore
            DynamicDataStore store = DataStoreFactory.GetStore(typeof(CustomRedirect));
            //check if there is an exisiting object with matching property "OldUrl"
            Dictionary<string, object> filter = new Dictionary<string, object>
            {
                {OldUrlPropertyName, currentCustomRedirect.OldUrl.ToLower()},
                {SiteIdPropertyName, currentCustomRedirect.SiteId}
            };
            CustomRedirect match = store.Find<CustomRedirect>(filter).SingleOrDefault();
            //if there is a match, replace the value.
            if (match != null)
                store.Save(currentCustomRedirect, match.Id);
            else
                store.Save(currentCustomRedirect);
        }


        /// <summary>
        /// Returns a list of all CustomRedirect objects in the Dynamic Data Store.
        /// </summary>
        /// <returns></returns>
        public List<CustomRedirect> GetCustomRedirects(bool excludeIgnored, int siteId)
        {
            DynamicDataStore store = DataStoreFactory.GetStore(typeof(CustomRedirect));
            IEnumerable<CustomRedirect> customRedirects;
            if (excludeIgnored)
            {
                customRedirects = from s in store.Items<CustomRedirect>().OrderBy(cr => cr.OldUrl)
                                  where s.State.Equals((int)State.Saved) & s.SiteId == siteId
                                  select s;
            }
            else
            {
                customRedirects = from s in store.Items<CustomRedirect>().OrderBy(cr => cr.OldUrl)
                                  where s.SiteId == siteId
                                  select s;
            }
            if (customRedirects.Any())
            {
                return customRedirects.ToList();
            }
            return new List<CustomRedirect>();
            
        }

        public List<CustomRedirect> GetIgnoredRedirect(int siteId)
        {
            DynamicDataStore store = DataStoreFactory.GetStore(typeof(CustomRedirect));

            var customRedirects = from s in store.Items<CustomRedirect>().OrderBy(cr => cr.OldUrl)
                              where s.State.Equals(State.Ignored) & s.SiteId.Equals(siteId)
                              select s;
            return customRedirects.ToList();

        }
        public List<CustomRedirect> GetDeletedRedirect(int siteId)
        {
            DynamicDataStore store = DataStoreFactory.GetStore(typeof(CustomRedirect));

            var deletedRedirects = from s in store.Items<CustomRedirect>().OrderBy(cr => cr.OldUrl)
                              where s.State.Equals(State.Deleted) & s.SiteId.Equals(siteId)
                              select s;
            return deletedRedirects.ToList();

        }

        public void UnignoreRedirect(string oldUrl, int siteId)
        {
            // Get hold of the datastore
            DynamicDataStore store = DataStoreFactory.GetStore(typeof(CustomRedirect));

            //find object with matching property "OldUrl"
            var urlEncode = HttpUtility.UrlEncode(oldUrl);
            if (urlEncode != null)
            {
                var savedUrl = urlEncode.ToLower().Replace("%2f", "/");
                var filters = new Dictionary<string, object>
                {
                    {OldUrlPropertyName, savedUrl.ToLower()},
                    {SiteIdPropertyName, siteId}
                };
                CustomRedirect match = store.Find<CustomRedirect>(filters).SingleOrDefault();
                if (match != null)
                {
                    // log request to database - if logging is turned on.
                    if (Configuration.Configuration.Logging == LoggerMode.On)
                    {
                        store.Delete(match);
                        // Safe logging
                        RequestLogger.Instance.LogRequest(savedUrl, string.Empty, siteId);
                    }
                }
            }
        }


        /// <summary>
        /// Delete CustomObject object from Data Store that has given "OldUrl" property
        /// </summary>
        /// <param name="oldUrl"></param>
        /// <param name="siteId"></param>
        public void DeleteCustomRedirect(string oldUrl, int siteId)
        {
            // Get hold of the datastore
            DynamicDataStore store = DataStoreFactory.GetStore(typeof(CustomRedirect));

            //find object with matching property "OldUrl"
            var urlEncode = HttpUtility.UrlEncode(oldUrl);
            if (urlEncode != null)
            {
                var savedUrl = urlEncode.ToLower().Replace("%2f", "/");
                var filters = new Dictionary<string, object>
                {
                    {OldUrlPropertyName, savedUrl.ToLower()},
                    {SiteIdPropertyName, siteId}
                };
                CustomRedirect match = store.Find<CustomRedirect>(filters).SingleOrDefault();
                if (match != null)
                    store.Delete(match);
            }
        }

        /// <summary>
        /// Delete all CustomRedirect objects
        /// </summary>
        public void DeleteAllCustomRedirects(int siteId)
        {
            // In order to avoid a database timeout, we delete the items one by one.
            DynamicDataStore store = DataStoreFactory.GetStore(typeof(CustomRedirect));
            foreach (CustomRedirect redirect in GetCustomRedirects(false, siteId))
            {
                store.Delete(redirect);
            }
        }

        public int DeleteAllIgnoredRedirects(int siteId)
        {
            // In order to avoid a database timeout, we delete the items one by one.
            DynamicDataStore store = DataStoreFactory.GetStore(typeof(CustomRedirect));
            var ignoredRedirects = GetIgnoredRedirect(siteId);
            foreach (CustomRedirect redirect in ignoredRedirects)
            {
                store.Delete(redirect);
            }
            return ignoredRedirects.Count;
        }


        /// <summary>
        /// Find all CustomRedirect objects which has a OldUrl og NewUrl that contains the search word.
        /// </summary>
        /// <param name="searchWord"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public List<CustomRedirect> SearchCustomRedirects(string searchWord, int siteId)
        {
            DynamicDataStore store = DataStoreFactory.GetStore(typeof(CustomRedirect));
            var customRedirects = from s in store.Items<CustomRedirect>()
                                  where s.SiteId.Equals(siteId) && (s.NewUrl.Contains(searchWord) || s.OldUrl.Contains(searchWord))
                                  select s;

            return customRedirects.ToList();

        }


    }
}
