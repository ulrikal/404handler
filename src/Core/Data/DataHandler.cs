using System;
using System.Collections.Generic;
using System.Data;
using System.Web;
using Castle.Components.DictionaryAdapter;
using EPiServer.Web;

namespace BVNetwork.NotFound.Core.Data
{
    public class DataHandler
    {
        public static string UknownReferer = "Uknown referers";

        public static Dictionary<string, int> GetRedirects(int siteId)
        {
            var keyCounts = new Dictionary<string, int>();
            new List<string>();
            DataAccessBaseEx dabe = DataAccessBaseEx.GetWorker();
            var allkeys = dabe.GetAllClientRequestCount(siteId);

            string oldUrl;
            foreach (DataTable table in allkeys.Tables)
            {
                foreach (DataRow row in table.Rows)
                {
                    oldUrl = row[0].ToString();


                    keyCounts.Add(oldUrl, Convert.ToInt32(row[1]));
                }
            }

            return keyCounts;

        }

        public static Dictionary<string, int> GetReferers(string url, int siteId)
        {
            var dataAccess = DataAccessBaseEx.GetWorker();
            var referersDs = dataAccess.GetRequestReferers(url, siteId);

            Dictionary<string, int> referers = new Dictionary<string, int>();
            if (referersDs.Tables[0] != null)
            {
                int unknownReferers = 0;
                for (int i = 0; i < referersDs.Tables[0].Rows.Count; i++)
                {
                   
                    var referer = referersDs.Tables[0].Rows[i][0].ToString();
                    int count = Convert.ToInt32(referersDs.Tables[0].Rows[i][1].ToString());
                    if (referer.Trim() != string.Empty && !referer.Contains("(null)"))
                    {
                        if (!referer.Contains("://"))
                            referer = referer.Insert(0, "/");
                        referers.Add(referer, count);
                    }
                    else
                        unknownReferers += count;
                    

                }
                if (unknownReferers > 0)
                    referers.Add(UknownReferer, unknownReferers);
            }
            return referers;
        }

        public static int GetTotalSuggestionCount(int siteId)
        {
            var dataAccess = DataAccessBaseEx.GetWorker();
            var totalSuggestionCountDs = dataAccess.GetTotalNumberOfSuggestions(siteId);
            if (totalSuggestionCountDs != null && totalSuggestionCountDs.Tables.Count > 0)
            {
                return Convert.ToInt32(totalSuggestionCountDs.Tables[0].Rows[0][0]);
            }
            return 0;
        }

        public static int GetSiteIdFromUrl(string url)
        {
            //TODO FIXA!
            string[] urlHostArray = url.Split('/');
            string urlHost = urlHostArray[0];
            if (urlHostArray.Length > 2)
            {
               urlHost = (string)urlHostArray.GetValue(urlHostArray.Length - 2);
            }
            

            var dataAccess = DataAccessBaseEx.GetWorker();
            var hostDataSet = dataAccess.FindSiteIdByHost(urlHost);

            foreach (DataTable table in hostDataSet.Tables)
            {
                if (table.Rows.Count > 0)
                {
                    var row = table.Rows[0];
                    return Convert.ToInt32(row[0]);
                }
            }
            return -1;
        }


        public static List<int> GetAllSiteIds()
        {
            List<int> siteIds = new List<int>();
            var dataAccess = DataAccessBaseEx.GetWorker();
            var hostDataSet = dataAccess.FindSiteIds();
            foreach (DataTable table in hostDataSet.Tables)
            {
                if (table.Rows.Count > 0)
                {
                    foreach (DataRow row in table.Rows)
                    {
                        siteIds.Add(Convert.ToInt32(row[0]));
                    }
                }
                
            }
            return siteIds;
        }

        public static int GetCurrentSiteId()
        {
            var dataAccess = DataAccessBaseEx.GetWorker();
            var hostDataSet = dataAccess.FindSiteIdByHost(SiteDefinition.Current.SiteUrl.Host);
            foreach (DataTable table in hostDataSet.Tables)
            {
                if (table.Rows.Count > 0)
                {
                    var row = table.Rows[0];
                    return Convert.ToInt32(row[0]);
                }
            }
            return -1;

        }
    }
}