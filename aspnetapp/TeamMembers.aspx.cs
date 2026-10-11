using System;
using System.Data;
using System.DirectoryServices.Protocols;
using System.Web.UI.WebControls;
using aspnetapp.Infrastructure;

namespace aspnetapp
{
    public partial class TeamMembers : System.Web.UI.Page
    {
        private string TeamId { get { return Request.QueryString["id"]; } }
        protected bool CanModify { get; private set; }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (!LegacyRules.ValidKey(TeamId)) { Reject(400, "Invalid team ID."); return; }
            Summary.TeamId = TeamId;
            EditLink.NavigateUrl = "TeamEdit.aspx?id=" + TeamId;
            // Recheck the parent even on a postback; hidden controls alone are not the rule.
            try
            {
                using (var ldap = LegacyDirectory.Open())
                {
                    var parent = (SearchResponse)ldap.SendRequest(new SearchRequest(LegacyDirectory.TeamDn(TeamId),
                        "(objectClass=fakeTeam)", SearchScope.Base, "fakeStatus"));
                    if (parent.Entries.Count == 0) { Reject(404, "Team not found."); return; }
                    CanModify = LegacyDirectory.Value(parent.Entries[0], "fakeStatus") != "archived";
                }
                AddPanel.Visible = CanModify;
                if (!CanModify) Message.Text = "Archived teams are read-only. Reactivate the team to change members.";
                if (!IsPostBack) BindMembers();
            }
            catch (DirectoryOperationException ex)
            {
                if (ex.Response != null && ex.Response.ResultCode == ResultCode.NoSuchObject) Reject(404, "Team not found.");
                else Reject(503, "Directory unavailable.");
            }
            catch (LdapException) { Reject(503, "Directory unavailable."); }
        }

        private void BindMembers()
        {
            var table = new DataTable();
            foreach (var column in new[] { "Id", "Name", "Email" }) table.Columns.Add(column);
            using (var ldap = LegacyDirectory.Open())
            {
                var result = (SearchResponse)ldap.SendRequest(new SearchRequest(LegacyDirectory.TeamDn(TeamId),
                    "(objectClass=inetOrgPerson)", SearchScope.OneLevel, "employeeNumber", "displayName", "mail"));
                foreach (SearchResultEntry entry in result.Entries)
                    table.Rows.Add(LegacyDirectory.Value(entry, "employeeNumber"), LegacyDirectory.Value(entry, "displayName"), LegacyDirectory.Value(entry, "mail"));
            }
            table.DefaultView.Sort = "Name ASC, Id ASC";
            MemberGrid.DataSource = table.DefaultView;
            MemberGrid.DataBind();
        }

        protected void Add_Click(object sender, EventArgs e)
        {
            if (!MembersPanel.Visible || !CanModify) return;
            var id = MemberKey.Text.Trim().ToLowerInvariant();
            var name = FullName.Text.Trim();
            var surname = Surname.Text.Trim();
            var email = Email.Text.Trim().ToLowerInvariant();
            var error = LegacyRules.MemberError(id, name, surname, email);
            if (error != null) { Message.Text = error; return; }
            try
            {
                using (var ldap = LegacyDirectory.Open())
                {
                    ldap.SendRequest(new AddRequest(LegacyDirectory.MemberDn(TeamId, id),
                        new DirectoryAttribute("objectClass", "top", "person", "organizationalPerson", "inetOrgPerson"),
                        new DirectoryAttribute("uid", TeamId + "." + id),
                        // cn/uid are globally unique in the base image; displayName is the human name.
                        new DirectoryAttribute("cn", TeamId + "." + id), new DirectoryAttribute("employeeNumber", id),
                        new DirectoryAttribute("displayName", name), new DirectoryAttribute("sn", surname), new DirectoryAttribute("mail", email)));
                }
                Response.Redirect("TeamMembers.aspx?id=" + TeamId, false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (DirectoryOperationException ex)
            {
                Message.Text = ex.Response != null && ex.Response.ResultCode == ResultCode.EntryAlreadyExists
                    ? "A member with this ID already exists in this team." : "The directory rejected the member. Reload and try again.";
            }
            catch (LdapException) { Message.Text = "Directory unavailable. The member was not added."; }
        }

        protected void Member_Command(object sender, GridViewCommandEventArgs e)
        {
            if (e.CommandName != "RemoveMember" || !MembersPanel.Visible || !CanModify) return;
            var id = Convert.ToString(e.CommandArgument);
            if (!LegacyRules.ValidKey(id)) { Message.Text = "Invalid member ID."; return; }
            try
            {
                using (var ldap = LegacyDirectory.Open()) ldap.SendRequest(new DeleteRequest(LegacyDirectory.MemberDn(TeamId, id)));
                Response.Redirect("TeamMembers.aspx?id=" + TeamId, false);
                Context.ApplicationInstance.CompleteRequest();
            }
            catch (DirectoryOperationException) { Message.Text = "The directory rejected the removal. The member may have changed."; }
            catch (LdapException) { Message.Text = "Directory unavailable. The member was not removed."; }
        }

        private void Reject(int status, string text)
        {
            Response.StatusCode = status;
            Response.TrySkipIisCustomErrors = true;
            MembersPanel.Visible = false;
            Summary.TeamId = null;
            Message.Text = text;
        }
    }
}
