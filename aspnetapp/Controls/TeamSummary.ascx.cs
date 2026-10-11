using System;
using System.DirectoryServices.Protocols;
using aspnetapp.Infrastructure;

namespace aspnetapp.Controls
{
    public partial class TeamSummary : System.Web.UI.UserControl
    {
        public string TeamId { get; set; }

        protected override void OnPreRender(EventArgs e)
        {
            SummaryPanel.Visible = LegacyRules.ValidKey(TeamId) && !Page.Response.IsRequestBeingRedirected;
            if (SummaryPanel.Visible)
            {
                try
                {
                    using (var ldap = LegacyDirectory.Open())
                    {
                        var parent = (SearchResponse)ldap.SendRequest(new SearchRequest(LegacyDirectory.TeamDn(TeamId),
                            "(objectClass=fakeTeam)", SearchScope.Base, "displayName"));
                        var children = (SearchResponse)ldap.SendRequest(new SearchRequest(LegacyDirectory.TeamDn(TeamId),
                            "(objectClass=inetOrgPerson)", SearchScope.OneLevel, "1.1"));
                        SummaryText.Text = (parent.Entries.Count == 0 ? TeamId : LegacyDirectory.Value(parent.Entries[0], "displayName"))
                            + " · " + children.Entries.Count + " member(s)";
                    }
                }
                catch (LdapException) { SummaryText.Text = "Member summary unavailable."; }
                catch (DirectoryOperationException) { SummaryText.Text = "Member summary unavailable."; }
            }
            base.OnPreRender(e);
        }
    }
}
