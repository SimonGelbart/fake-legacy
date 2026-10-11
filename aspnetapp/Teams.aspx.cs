using System;
using System.Data;
using System.DirectoryServices.Protocols;
using aspnetapp.Infrastructure;

namespace aspnetapp
{
    public partial class Teams : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                SearchText.Text = Request.QueryString["q"] ?? "";
                WithFuture.Checked = string.Equals(Request.QueryString["withFuture"], "true", StringComparison.OrdinalIgnoreCase);
                BindTeams();
            }
        }

        protected void Search_Click(object sender, EventArgs e) { BindTeams(); }

        private void BindTeams()
        {
            Message.Text = "";
            var table = new DataTable();
            foreach (var column in new[] { "Id", "Name", "Status", "StartDate" }) table.Columns.Add(column);
            try
            {
                using (var ldap = LegacyDirectory.Open())
                {
                    var filter = "(&(objectClass=fakeTeam)";
                    if (!WithArchived.Checked) filter += "(!(fakeStatus=archived))";
                    var query = SearchText.Text.Trim();
                    if (query.Length > 0)
                    {
                        var escaped = LegacyDirectory.EscapeFilter(query);
                        filter += "(|(ou=*" + escaped + "*)(displayName=*" + escaped + "*))";
                    }
                    filter += ")";
                    var result = (SearchResponse)ldap.SendRequest(new SearchRequest(
                        LegacyDirectory.TeamsDn, filter, SearchScope.OneLevel, "ou", "displayName", "fakeStatus", "fakeStartDate"));
                    foreach (SearchResultEntry entry in result.Entries)
                    {
                        var date = LegacyDirectory.Value(entry, "fakeStartDate");
                        // This filter is deliberately in code, not in the LDAP search.
                        if (!WithFuture.Checked && LegacyRules.Future(date)) continue;
                        table.Rows.Add(LegacyDirectory.Value(entry, "ou"), LegacyDirectory.Value(entry, "displayName"),
                            LegacyDirectory.Value(entry, "fakeStatus"), date);
                    }
                }
            }
            catch (LdapException) { Unavailable(); }
            catch (DirectoryOperationException) { Unavailable(); }
            table.DefaultView.Sort = "Name ASC, Id ASC";
            TeamGrid.DataSource = table.DefaultView;
            TeamGrid.DataBind();
        }

        private void Unavailable()
        {
            Message.Text = "Directory unavailable. Check the local LDAP container and connection settings.";
            Response.StatusCode = 503;
            Response.TrySkipIisCustomErrors = true;
        }
    }
}
