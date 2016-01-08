<%@ Control Language="C#" Inherits="System.Web.Mvc.ViewUserControl<Knowit.NotFound.Models.RedirectIndexViewData>" %>
<%@ Import Namespace="EPiServer.Cms.Shell" %>
<%@ Import Namespace="EPiServer.Core" %>
<%@ Import Namespace="EPiServer.Shell.Web.Mvc.Html" %>
<%@ Import Namespace="Knowit.NotFound.Core.CustomRedirects" %>
<%@ Import Namespace="Knowit.NotFound.Core.Data" %>


<!-- <div class="notfound about"> 
    </div>
    -->
    <div>
        <% Html.BeginGadgetForm("Index"); %>
        <div class="epi-formArea">
            <fieldset>
                <div class="rightsearch">
                    <input type="text" class="notfound-search" name="searchWord" />
                     <button type="submit" class="notfoundbutton search"><%=Html.Translate("/gadget/redirects/search")%></button>
                    <input type="hidden" name="pageSize" value='<%=Model.PageSize %>' />
                    <input type="hidden" name="isSuggestions" value='<%=Model.IsSuggestions %>' />
                    <input type="hidden" name="siteId" value='<%=Model.SiteId %>'/>
                </div>
                <%=Model.ActionInformation as string %>
            </fieldset>
        </div>
        <% Html.EndForm(); %>
       
        <input type="hidden" name="pageSize" value='<%=Model.PageSize %>' />
        <% Html.RenderPartial("Menu", "Index"); %>
        <% Html.BeginGadgetForm("Save"); %>
        <table class="epi-default">
            <thead>
                <tr>
                    <th>
                        <label>
                            <%=EPiServer.Framework.Localization.LocalizationService.Current.GetString("/gadget/redirects/oldurl")%></label>
                    </th>
                    <th>
                        <label>
                            <%=EPiServer.Framework.Localization.LocalizationService.Current.GetString("/gadget/redirects/newurl")%></label>
                    </th>
                    <th style="text-align: center">
                        <label>
                            <%=EPiServer.Framework.Localization.LocalizationService.Current.GetString("/gadget/redirects/wildcard")%></label>
                    </th>
                    <th>
                    </th>
                </tr>
            </thead>
          
            <tr>
                <td class="longer">
                    <input style="word-wrap: break-word" name="oldUrl" class="required redirect-longer" />
                </td>
                <td class="longer">
                    <input name="newUrl" class="required redirect-longer" />
                </td>
                <td class="shorter delete">
                    <input name="skipWildCardAppend" type="checkbox" />
                </td>
                <td class="shorter delete">
                     <button type="submit" class="notfoundbutton">Add</button>
                </td>
            </tr>
         
            <% if (Model.CustomRedirectList.Count > 0)
               {
                   foreach (CustomRedirect m in Model.CustomRedirectList)
                   {
            %>
            <tr>
                <td class="longer">
                    <%= Html.Encode(m.OldUrl)%>
                </td>
                <td class="longer">
                    <%if (m.State.Equals((int)DataStoreHandler.State.Ignored))
                      {%>
                    <i>[<%=EPiServer.Framework.Localization.LocalizationService.Current.GetString("/gadget/redirects/ignored")%>]</i>
                    <%}
                      else
                      {  %>
                    <%= m.NewUrl%>
                    <% } %>
                </td>
                <td class="shorter delete">
                    <img src="/App_Themes/Default/Images/Tools/<%=m.WildCardSkipAppend ? "CheckBoxOn.gif" : "CheckBoxOff.gif" %>" />
                </td>
                <td class="shorter delete">
                    <%= Html.ViewLink(
                        "",  // html helper
                        "Delete",  // title
                        "Delete", // Action name
                                        "epi-quickLinksDelete epi-iconToolbar-item-link epi-iconToolbar-delete", // css class
                        "Index",
                        new { oldUrl =  Uri.EscapeDataString(m.OldUrl), pageNumber = Model.PageNumber, searchWord = Model.SearchWord, pageSize = Model.PageSize, siteId = Model.SiteId })%>
                </td>
            </tr>
            <%} %>
            <% 
               } %>
        </table>
        <input type="hidden" name="siteId" value='<%=Model.SiteId %>'/>
        <% Html.EndForm(); %>
    </div>
