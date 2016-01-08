using EPiServer.Data;
using EPiServer.Data.Dynamic;
using Knowit.NotFound.Core.Data;

namespace Knowit.NotFound.Core.CustomRedirects 
{

	[EPiServerDataStore(AutomaticallyCreateStore = true, AutomaticallyRemapStore = true)]
	public class CustomRedirect : IDynamicData
	{

		private string _oldUrl;
		private string _newUrl;
	    public int NotfoundErrorCount;
	    public int SiteId { set; get; }

		/// <summary>
		/// Gets or sets a value indicating whether to skip appending the 
		/// old url fragment to the new one. Default value is false.
		/// </summary>
		/// <remarks>
		/// If you want to redirect many addresses below a specifc one to
		/// one new url, set this to true. If we get a wild card match on
		/// this url, the new url will be used in its raw format, and the
		/// old url will not be appended to the new one.
		/// </remarks>
		/// <value><c>true</c> to skip appending old url if wild card match; otherwise, <c>false</c>.</value>
		public bool WildCardSkipAppend { get; set; }

	    public string OldUrl
		{
			get
			{
				return _oldUrl.ToLower();
			}
			set
			{
				_oldUrl = value;
			}
		}

		public string NewUrl
		{
			get
			{
			   
				return  _newUrl != null ? _newUrl.ToLower() : null;
			}
			set
			{
				_newUrl = value;
			}
		}


		public int  State { get; set; }

	    /// <summary>
		/// Tells if the new url is a virtual url, not containing
		/// the base root url to redirect to. All urls starting with
		/// "/" is determined to be virtuals.
		/// </summary>
		public bool IsVirtual
		{
			get
			{
				return _newUrl.StartsWith("/");
			}
		}

		/// <summary>
		/// The hash code for the CustomRedirect class is the
		/// old url string, which is the one we'll be doing lookups
		/// based on.
		/// </summary>
		/// <returns>The Hash code of the old Url</returns>
		public override int GetHashCode()
		{
		  
			//TODO: should not have to check for null
			return _oldUrl != null ? _oldUrl.GetHashCode() : 0;
		}

		public Identity Id { get; set; }
		

		#region constructors...
		public CustomRedirect()
		{

		}

		public CustomRedirect(string oldUrl, string newUrl, bool skipWildCardAppend, int siteId)
			: this(oldUrl, newUrl, siteId) 
		{
			WildCardSkipAppend = skipWildCardAppend;
		}

        public CustomRedirect(string oldUrl, string newUrl)
        {
            _oldUrl = oldUrl;
            _newUrl = newUrl;
            SiteId = DataHandler.GetSiteIdFromUrl(oldUrl);
        }

        public CustomRedirect(string oldUrl, string newUrl, int siteId)
		{
			_oldUrl = oldUrl;
			_newUrl = newUrl;
		    SiteId = siteId;
		}


		public CustomRedirect(string oldUrl, int state, int count, int siteId)
		{
			_oldUrl = oldUrl;
			State = state;
			NotfoundErrorCount = count;
		    SiteId = siteId;

		}

		public CustomRedirect(CustomRedirect redirect)
		{
			_oldUrl = redirect._oldUrl;
			_newUrl = redirect._newUrl;
			WildCardSkipAppend = redirect.WildCardSkipAppend;
		    SiteId = redirect.SiteId;
		}
		#endregion
		
	}
}
