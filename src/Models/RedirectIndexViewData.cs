using System.Collections.Generic;
using BVNetwork.NotFound.Core.CustomRedirects;

namespace BVNetwork.NotFound.Models
{
    public class RedirectIndexViewData
    {
        public List<CustomRedirect> CustomRedirectList { get; set; }
        public int PageSize {  get; set; }
        public int PageNumber { get; set; }
        public int PagerSize {  get;  set; }
        public int TotalItemsCount { get; set; }
        public string ActionInformation { get; set; }
        public string SearchWord { get; set; }
        public bool IsSuggestions { get; set; }
        public int HighestSuggestionValue { get; set; }
        public int LowestSuggestionValue { get; set; }

        public int SiteId { get; set; }


        public IEnumerable<int> Pages
        {
            get
            {
                List<int> list2 = new List<int>();
                list2.Add(1);
                List<int> list = list2;
                if (((PageNumber - PagerSize) - 1) > 1)
                {
                    list.Add(0);
                }
                for (int i = PageNumber - PagerSize; i <= (PageNumber + PagerSize); i++)
                {
                    if ((i > 1) && (i < TotalPagesCount))
                    {
                        list.Add(i);
                    }
                }
                if (((PageNumber + PagerSize) + 1) < TotalPagesCount)
                {
                    list.Add(0);
                }
                if (TotalPagesCount > 1)
                {
                    list.Add(TotalPagesCount);
                }
                return list;
            }
        }


        public int TotalPagesCount
        {
            get
            {
                return (((TotalItemsCount - 1) / PageSize) + 1);
            }
        }

        public int MaxIndexOfItem
        {
            get
            {
                if ((PageNumber * PageSize) <= TotalItemsCount)
                {
                    return (PageNumber * PageSize);
                }
                return TotalItemsCount;
            }
        }

        public int MinIndexOfItem
        {
            get
            {
                if (TotalItemsCount <= 0)
                {
                    return 0;
                }
                return (((PageNumber - 1) * PageSize) + 1);
            }
        }



    }   
}
