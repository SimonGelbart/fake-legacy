using System;
using System.DirectoryServices.Protocols;
using aspnetapp.Infrastructure;

namespace aspnetapp
{
    public partial class TeamEdit : System.Web.UI.Page
    {
        private string TeamId { get { return Request.QueryString["id"]; } }
        private bool Existing { get { return TeamId != null; } }

        protected void Page_Load(object sender, EventArgs e)
        {
            Heading.Text = Existing ? "Edit team" : "Create team";
            TeamKey.Enabled = !Existing;
            DeleteButton.Visible = Existing;
            MembersLink.Visible = Existing;
            Summary.TeamId = TeamId;
            if (Existing && !LegacyRules.ValidKey(TeamId)) { Reject(400, "Invalid team ID."); return; }
            if (Existing) MembersLink.NavigateUrl = "TeamMembers.aspx?id=" + TeamId;
            if (!IsPostBack && Existing) LoadTeam();
        }

        protected override void OnPreRender(EventArgs e)
        {
            PlannedHint.Visible = Status.SelectedValue == "planned";
            base.OnPreRender(e);
        }

        protected void Status_Changed(object sender, EventArgs e) { Message.Text = ""; }

        private void LoadTeam()
        {
            try
            {
                using (var ldap = LegacyDirectory.Open())
                {
                    var result = (SearchResponse)ldap.SendRequest(new SearchRequest(LegacyDirectory.TeamDn(TeamId),
                        "(objectClass=fakeTeam)", SearchScope.Base, "displayName", "fakeStatus", "fakeStartDate", "description"));
                    if (result.Entries.Count == 0) { Reject(404, "Team not found."); return; }
                    var entry = result.Entries[0];
                    TeamKey.Text = TeamId;
                    DisplayName.Text = LegacyDirectory.Value(entry, "displayName");
                    Status.SelectedValue = LegacyDirectory.Value(entry, "fakeStatus");
                    StartDate.Text = LegacyDirectory.Value(entry, "fakeStartDate");
                    Description.Text = LegacyDirectory.Value(entry, "description");
                }
            }
            catch (DirectoryOperationException ex)
            {
                if (ex.Response != null && ex.Response.ResultCode == ResultCode.NoSuchObject) Reject(404, "Team not found.");
                else Reject(503, "Directory unavailable.");
            }
            catch (LdapException) { Reject(503, "Directory unavailable."); }
        }

        protected void Save_Click(object sender, EventArgs e)
        {
            if (!Editor.Visible) return;
            var id = Existing ? TeamId : TeamKey.Text.Trim().ToLowerInvariant();
            var name = DisplayName.Text.Trim();
            var description = Description.Text.Trim();
            var date = StartDate.Text.Trim();
            var error = LegacyRules.TeamError(id, name, Status.SelectedValue, date, description);
            if (error != null) { Message.Text = error; return; }
            try
            {
                using (var ldap = LegacyDirectory.Open())
                {
                    var dn = LegacyDirectory.TeamDn(id);
                    if (Existing)
                    {
                        // Directory-dependent validation is intentionally mixed into this handler.
                        if (Status.SelectedValue == "archived")
                        {
                            var children = (SearchResponse)ldap.SendRequest(new SearchRequest(dn, "(objectClass=*)", SearchScope.OneLevel, "1.1"));
                            if (children.Entries.Count > 0) { Message.Text = "Remove all members before archiving the team."; return; }
                        }
                        ldap.SendRequest(new ModifyRequest(dn,
                            LegacyDirectory.Replace("displayName", name), LegacyDirectory.Replace("fakeStatus", Status.SelectedValue),
                            LegacyDirectory.Replace("fakeStartDate", date), LegacyDirectory.Replace("description", description)));
                    }
                    else
                    {
                        var request = new AddRequest(dn,
                            new DirectoryAttribute("objectClass", "top", "organizationalUnit", "fakeTeam"),
                            new DirectoryAttribute("ou", id), new DirectoryAttribute("displayName", name),
                            new DirectoryAttribute("fakeStatus", Status.SelectedValue));
                        if (date.Length > 0) request.Attributes.Add(new DirectoryAttribute("fakeStartDate", date));
                        if (description.Length > 0) request.Attributes.Add(new DirectoryAttribute("description", description));
                        ldap.SendRequest(request);
                    }
                }
                RedirectTo("TeamEdit.aspx?id=" + id);
            }
            catch (DirectoryOperationException ex)
            {
                Message.Text = ex.Response != null && ex.Response.ResultCode == ResultCode.EntryAlreadyExists
                    ? "A team with this ID already exists." : "The directory rejected the change. Reload the team and try again.";
            }
            catch (LdapException) { Message.Text = "Directory unavailable. Your change was not saved."; }
        }

        protected void Delete_Click(object sender, EventArgs e)
        {
            if (!Editor.Visible || !Existing) return;
            try
            {
                using (var ldap = LegacyDirectory.Open())
                {
                    var dn = LegacyDirectory.TeamDn(TeamId);
                    var children = (SearchResponse)ldap.SendRequest(new SearchRequest(dn, "(objectClass=*)", SearchScope.OneLevel, "1.1"));
                    if (children.Entries.Count > 0) { Message.Text = "Remove all members before deleting the team."; return; }
                    ldap.SendRequest(new DeleteRequest(dn));
                }
                RedirectTo("Teams.aspx");
            }
            catch (DirectoryOperationException) { Message.Text = "The directory rejected the deletion. The team may have changed."; }
            catch (LdapException) { Message.Text = "Directory unavailable. The team was not deleted."; }
        }

        private void Reject(int status, string text)
        {
            Response.StatusCode = status;
            Response.TrySkipIisCustomErrors = true;
            Editor.Visible = false;
            Message.Text = text;
            Summary.TeamId = null;
        }

        private void RedirectTo(string path)
        {
            Response.Redirect(path, false);
            Context.ApplicationInstance.CompleteRequest();
        }
    }
}
