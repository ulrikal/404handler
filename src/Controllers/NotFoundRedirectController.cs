using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security;
using System.Web;
using System.Web.Mvc;
using BVNetwork.NotFound.Core.CustomRedirects;
using BVNetwork.NotFound.Core.Data;
using BVNetwork.NotFound.Models;
using EPiServer.Logging;
using EPiServer.Framework.Localization;
using EPiServer.Security;
using EPiServer.Shell.Gadgets;

namespace BVNetwork.NotFound.Controllers
{
    [EPiServer.Shell.Web.ScriptResource("ClientResources/Scripts/jquery.blockUI.js")]
    [Gadget(ResourceType = typeof(NotFoundMultiSiteRedirectController),
           NameResourceKey = "GadgetName", DescriptionResourceKey = "GadgetDescription")]
    [EPiServer.Shell.Web.CssResource("ClientResources/Content/RedirectGadget.css")]
    [EPiServer.Shell.Web.ScriptResource("ClientResources/Scripts/jquery.form.js")]
    [Authorize]
    public class NotFoundMultiSiteRedirectController : Controller
    {

        private static readonly ILogger Logger = LogManager.GetLogger();
        private void CheckAccess()
        {
            if (!PrincipalInfo.HasEditAccess)
            {
                throw new SecurityException("Access denied");
            }
        }

        public ActionResult Index(int? pageNumber, string searchWord, int? pageSize, bool? isSuggestions, int siteId)
        {

            CheckAccess();

            if (!string.IsNullOrEmpty(CustomRedirectHandler.CustomRedirectHandlerException))
            {
                return Content("An error has occured in the dynamic data store: " + CustomRedirectHandler.CustomRedirectHandlerException);
            }
            List<CustomRedirect> customRedirectList;
            if (isSuggestions.HasValue && isSuggestions.Value)
            {
                customRedirectList = GetSuggestions(searchWord, siteId);

                var viewData = GetRedirectIndexViewData(pageNumber, customRedirectList, GetSearchResultInfo(searchWord, customRedirectList.Count, true, siteId), searchWord, pageSize, true, siteId);
                if (customRedirectList.Count > 0)
                {
                    viewData.HighestSuggestionValue = customRedirectList.First().NotfoundErrorCount;
                    viewData.LowestSuggestionValue = customRedirectList.Last().NotfoundErrorCount;
                }
                return View("Index", viewData);
            }
            customRedirectList = GetData(searchWord, siteId);
            return View("Index", GetRedirectIndexViewData(pageNumber, customRedirectList, GetSearchResultInfo(searchWord, customRedirectList.Count, false, siteId), searchWord, pageSize, false, siteId));
        }

        public ActionResult SaveSuggestion(string oldUrl, string newUrl, string skipWildCardAppend, int? pageNumber, int? pageSize, int siteId)
        {
            CheckAccess();
            SaveRedirect(oldUrl, newUrl, skipWildCardAppend, siteId);

            // delete rows from DB
            var dbAccess = DataAccessBaseEx.GetWorker();
            dbAccess.DeleteRowsForRequest(oldUrl, siteId);

            //
            List<CustomRedirect> customRedirectList = GetSuggestions(null, siteId);
            string actionInfo = string.Format(LocalizationService.Current.GetString("/gadget/redirects/saveredirect"), oldUrl, newUrl);
            DataStoreEventHandlerHook.DataStoreUpdated();
            var viewData = GetRedirectIndexViewData(pageNumber, customRedirectList, actionInfo, null, pageSize, true, siteId);
            viewData.HighestSuggestionValue = customRedirectList.First().NotfoundErrorCount;
            viewData.LowestSuggestionValue = customRedirectList.Last().NotfoundErrorCount;
            return View("Index", viewData);
        }

        public ActionResult Suggestions(int siteId)
        {
            CheckAccess();
            return Index(null, "", null, true, siteId);
        }

        [GadgetAction(Text = "Administer")]
        public ActionResult Administer()
        {
            CheckAccess();
            return View();
        }

        [ValidateInput(false)]
        public ActionResult Save(string oldUrl, string newUrl, string skipWildCardAppend, int? pageNumber, int? pageSize, int siteId)
        {
            CheckAccess();
            SaveRedirect(oldUrl, newUrl, skipWildCardAppend, siteId);
            List<CustomRedirect> redirectList = GetData(null, siteId);
            string actionInfo = string.Format(LocalizationService.Current.GetString("/gadget/redirects/saveredirect"), oldUrl, newUrl);
            return View("Index", GetRedirectIndexViewData(pageNumber, redirectList, actionInfo, null, pageSize, false, siteId));

        }

        public void SaveRedirect(string oldUrl, string newUrl, string skipWildCardAppend, int siteId)
        {

            Logger.Debug("Adding redirect for site {2}: '{0}' -> '{1}'", oldUrl, newUrl, siteId);
            // Get hold of the datastore
            DataStoreHandler dsHandler = new DataStoreHandler();
            dsHandler.SaveCustomRedirect(new CustomRedirect(oldUrl.Trim(), newUrl.Trim(), skipWildCardAppend != null, siteId));
            DataStoreEventHandlerHook.DataStoreUpdated();

        }

        public ActionResult IgnoreRedirect(string oldUrl, int pageNumber, string searchWord, int pageSize, int siteId)
        {
            CheckAccess();
            // delete rows from DB
            var dbAccess = DataAccessBaseEx.GetWorker();
            dbAccess.DeleteRowsForRequest(oldUrl, siteId);

            // add redirect to dds with state "ignored"
            var redirect = new CustomRedirect
            {
                OldUrl = oldUrl,
                State = Convert.ToInt32(DataStoreHandler.State.Ignored)
            };
            DataStoreHandler dsHandler = new DataStoreHandler();
            dsHandler.SaveCustomRedirect(redirect);
            DataStoreEventHandlerHook.DataStoreUpdated();

            List<CustomRedirect> customRedirectList = GetSuggestions(searchWord, siteId);
            string actionInfo = string.Format(LocalizationService.Current.GetString("/gadget/redirects/ignoreredirect"), oldUrl);
            RedirectIndexViewData viewData = GetRedirectIndexViewData(pageNumber, customRedirectList, actionInfo, searchWord, pageSize, true, siteId);
            viewData.HighestSuggestionValue = customRedirectList.First().NotfoundErrorCount;
            viewData.LowestSuggestionValue = customRedirectList.Last().NotfoundErrorCount;

            return View("Index", viewData);
        }

        [ValidateInput(false)]
        public ActionResult Delete(string oldUrl, int? pageNumber, string searchWord, int? pageSize, int siteId)
        {
            CheckAccess();

            Logger.Debug("Deleting redirect: '{0}'", oldUrl);

            DataStoreHandler dsHandler = new DataStoreHandler();
            dsHandler.DeleteCustomRedirect(oldUrl, siteId);
            DataStoreEventHandlerHook.DataStoreUpdated();
            List<CustomRedirect> customRedirectList = GetData(searchWord, siteId);
            //Make sure that the searchinfo is contained after an item has been deleted - if there is any.
            return View("Index", GetRedirectIndexViewData(pageNumber, customRedirectList, GetSearchResultInfo(searchWord, customRedirectList.Count, false, siteId), searchWord, pageSize, false, siteId));
        }

        /// <summary>
        /// Get the data that will be presented in the view(s).
        /// </summary>
        /// <param name="pageNumber">The current page number for the pager view</param>
        /// <param name="redirectList">The List of redirects</param>
        /// <param name="actionInformation">Text that will be presented in the view</param>
        /// <param name="searchWord">search word</param>
        /// <param name="pageSize"></param>
        /// <param name="isSuggestions"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public RedirectIndexViewData GetRedirectIndexViewData(int? pageNumber, List<CustomRedirect> redirectList, string actionInformation, string searchWord, int? pageSize, bool isSuggestions, int siteId)
        {
            RedirectIndexViewData indexData = new RedirectIndexViewData
            {
                IsSuggestions = isSuggestions,
                ActionInformation = actionInformation,
                SearchWord = searchWord,
                TotalItemsCount = redirectList.Count,
                PageNumber = pageNumber ?? 1,
                PagerSize = 4,
                PageSize = pageSize ?? 30,
                SiteId = siteId
            };
            //TODO: read pagersize and pagesize from configuration.
            if (redirectList.Count > indexData.PageSize)
                indexData.CustomRedirectList = redirectList.GetRange(indexData.MinIndexOfItem - 1, indexData.MaxIndexOfItem - indexData.MinIndexOfItem + 1);
            else
                indexData.CustomRedirectList = redirectList;
            return indexData;

        }

        public ActionResult Ignored(int siteId)
        {
            CheckAccess();
            DataStoreHandler dsHandler = new DataStoreHandler();
            var ignoredRedirects = dsHandler.GetIgnoredRedirect(siteId);
            return View("Ignored", ignoredRedirects);
        }
        public ActionResult Deleted(int siteId)
        {
            CheckAccess();
            DataStoreHandler dsHandler = new DataStoreHandler();
            var deletedRedirects = dsHandler.GetDeletedRedirect(siteId);
            return View("Deleted", deletedRedirects);
        }


        public ActionResult Unignore(string url, int siteId)
        {
            CheckAccess();
            DataStoreHandler dsHandler = new DataStoreHandler();
            dsHandler.DeleteCustomRedirect(url, siteId);
            return Ignored(siteId);
        }

        public ActionResult Referers(string url, int siteId)
        {
            CheckAccess();
            var referers = DataHandler.GetReferers(url, siteId);
            ViewData.Add("refererUrl", url);
            return View("Referers", referers);
        }

        public ActionResult DeleteAllIgnored(int siteId)
        {
            CheckAccess();
            var dsHandler = new DataStoreHandler();
            int deleteCount = dsHandler.DeleteAllIgnoredRedirects(siteId);
            string infoText = string.Format(LocalizationService.Current.GetString("/gadget/redirects/ignoredremoved"), deleteCount);
            ViewData["information"] = infoText;
            return View("Administer");
        }

        public ActionResult DeleteAllSuggestions(int siteId)
        {
            CheckAccess();
            DataAccessBaseEx.GetWorker().DeleteAllSuggestions(siteId);
            ViewData["information"] = LocalizationService.Current.GetString("/gadget/redirects/suggestionsdeleted");
            return View("Administer");
        }

        public ActionResult DeleteAllRedirects()
        {
            CheckAccess();
            DataStoreHandler dsHandler = new DataStoreHandler();
            dsHandler.DeleteAllCustomRedirects();
            DataStoreEventHandlerHook.DataStoreUpdated();
            ViewData["information"] = LocalizationService.Current.GetString("/gadget/redirects/redirectsdeleted");
            return View("Administer");
        }

        /// <summary>
        /// Removed Deleted (410) redirect
        /// </summary>
        /// <param name="url"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public ActionResult DeleteDeleted(string url, int siteId)
        {
            CheckAccess();
            var dsHandler = new DataStoreHandler();
            dsHandler.DeleteCustomRedirect(url, siteId);
            return Deleted(siteId);
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public FileUploadJsonResult ImportRedirects(HttpPostedFileBase xmlfile)
        {
            CheckAccess();
            // Read all redirects from xml file
            RedirectsXmlParser parser = new RedirectsXmlParser(xmlfile.InputStream);
            // Save all redirects from xml file
            CustomRedirectCollection redirects = parser.Load(0);
            string message;
            if (redirects != null && redirects.Count != 0)
            {
                CustomRedirectHandler.Current.SaveCustomRedirects(redirects);
                message = string.Format(LocalizationService.Current.GetString("/gadget/redirects/importsuccess"), redirects.Count);
            }
            else
            {
                message = LocalizationService.Current.GetString("/gadget/redirects/importnone");
            }
            return new FileUploadJsonResult { Data = new {message } };
        }

        [AcceptVerbs(HttpVerbs.Post)]
        public FileUploadJsonResult ImportDeleted(HttpPostedFileBase txtFile)
        {
            CheckAccess();
            var redirects = new CustomRedirectCollection();
            using (var streamReader = new StreamReader(txtFile.InputStream))
            {
                while (streamReader.Peek() >= 0)
                {
                    var url = streamReader.ReadLine();
                    if (!string.IsNullOrEmpty(url))
                    {
                        redirects.Add(new CustomRedirect
                        {
                            OldUrl = url,
                            State = (int)DataStoreHandler.State.Deleted,
                        });
                    }
                }
            }
            string message;
            if (redirects.Count != 0)
            {
                CustomRedirectHandler.Current.SaveCustomRedirects(redirects);
                message = string.Format(LocalizationService.Current.GetString("/gadget/redirects/importdeletedsuccess"), redirects.Count);
            }
            else
            {
                message = LocalizationService.Current.GetString("/gadget/redirects/importnone");
            }
            return new FileUploadJsonResult { Data = new {message } };
        }


        public ActionResult DeleteSuggestions(int maxErrors, int minimumDays, int siteId)
        {
            CheckAccess();
            DataAccessBaseEx.GetWorker().DeleteSuggestions(maxErrors, minimumDays, siteId);
            ViewData["information"] = LocalizationService.Current.GetString("/gadget/redirects/suggestionsdeleted");
            return View("Administer");
        }

        /// <summary>
        /// Get the text that will be displayed in the info area of the gadget.
        /// </summary>
        /// <param name="searchWord"></param>
        /// <param name="count"></param>
        /// <param name="isSuggestions"></param>
        /// <param name="siteId"></param>
        /// <returns></returns>
        public string GetSearchResultInfo(string searchWord, int count, bool isSuggestions, int siteId)
        {
            string actionInfo;
            if (string.IsNullOrEmpty(searchWord) && !isSuggestions)
            {
                actionInfo = string.Format(LocalizationService.Current.GetString("/gadget/redirects/storedredirects"), count);
                actionInfo += " " + string.Format(LocalizationService.Current.GetString("/gadget/redirects/andsuggestions"), DataHandler.GetTotalSuggestionCount(siteId));
            }
            else if (string.IsNullOrEmpty(searchWord) && isSuggestions)
                actionInfo = string.Format(LocalizationService.Current.GetString("/gadget/redirects/storedsuggestions"), count);
            else if (isSuggestions)
                actionInfo = string.Format(LocalizationService.Current.GetString("/gadget/redirects/searchsuggestions"), searchWord, count);
            else
                actionInfo = string.Format(LocalizationService.Current.GetString("/gadget/redirects/searchresult"), searchWord, count);
            return actionInfo;
        }

        /// <summary>
        /// Get custom redirect data from dynamic data store.
        /// </summary>
        /// <param name="searchWord"></param>
        /// <returns></returns>
        public List<CustomRedirect> GetData(string searchWord, int siteId)
        {
            DataStoreHandler dsHandler = new DataStoreHandler();
            var customRedirectList = string.IsNullOrEmpty(searchWord) ? dsHandler.GetCustomRedirects(true) : dsHandler.SearchCustomRedirects(searchWord, siteId);

            return customRedirectList;
        }

        public List<CustomRedirect> GetSuggestions(String searchWord, int siteId)
        {

            var customRedirectList = new List<CustomRedirect>();
            var dict = DataHandler.GetRedirects(siteId);

            foreach (KeyValuePair<string, int> redirect in dict)
            {
                customRedirectList.Add(new CustomRedirect(redirect.Key, Convert.ToInt32(DataStoreHandler.State.Suggestion), redirect.Value, siteId));
            }

            return customRedirectList;
        }
        public List<CustomRedirect> GetDeletedUrls(int siteId)
        {
            DataStoreHandler dsHandler = new DataStoreHandler();
            var customRedirectList = dsHandler.GetDeletedRedirect(siteId);

            return customRedirectList;
        }

        public static string GadgetEditMenuName
        {
            get { return LocalizationService.Current.GetString("/gadget/redirects/configure"); }
        }

        public static string GadgetName
        {
            get { return LocalizationService.Current.GetString("/gadget/redirects/name"); }
        }

        public static string GadgetDescription
        {
            get { return LocalizationService.Current.GetString("/gadget/redirects/description"); }
        }

        public ActionResult AddDeletedUrl(string oldUrl, int siteId)
        {
            CheckAccess();


            // add redirect to dds with state "deleted"
            var redirect = new CustomRedirect
            {
                OldUrl = oldUrl,
                State = Convert.ToInt32(DataStoreHandler.State.Deleted)
            };
            DataStoreHandler dsHandler = new DataStoreHandler();
            dsHandler.SaveCustomRedirect(redirect);
            DataStoreEventHandlerHook.DataStoreUpdated();

            // delete rows from DB
            var dbAccess = DataAccessBaseEx.GetWorker();
            dbAccess.DeleteRowsForRequest(oldUrl, siteId);

            //TODO why this?
            List<CustomRedirect> customRedirectList = GetDeletedUrls(siteId);
            DataStoreEventHandlerHook.DataStoreUpdated();
            return Deleted(siteId);
        }
    }

    public class FileUploadJsonResult : JsonResult
    {

        public override void ExecuteResult(ControllerContext context)
        {
            ContentType = "text/html";
            context.HttpContext.Response.Write("<textarea>");
            base.ExecuteResult(context);
            context.HttpContext.Response.Write("</textarea>");
        }
    }
}
