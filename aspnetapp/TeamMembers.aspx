<%@ Page Title="Team members" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="TeamMembers.aspx.cs" Inherits="aspnetapp.TeamMembers" %>
<%@ Register Src="~/Controls/TeamSummary.ascx" TagPrefix="legacy" TagName="TeamSummary" %>
<asp:Content ID="MembersContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Team members</h1>
    <legacy:TeamSummary ID="Summary" runat="server" />
    <asp:Label ID="Message" runat="server" CssClass="error" role="status" />
    <asp:Panel ID="MembersPanel" runat="server">
        <asp:GridView ID="MemberGrid" runat="server" AutoGenerateColumns="false" OnRowCommand="Member_Command" EmptyDataText="This team has no members.">
            <Columns>
                <asp:BoundField DataField="Id" HeaderText="Member ID" />
                <asp:BoundField DataField="Name" HeaderText="Full name" />
                <asp:BoundField DataField="Email" HeaderText="Email" />
                <asp:TemplateField><ItemTemplate>
                    <asp:LinkButton ID="RemoveMember" runat="server" Text="Remove" CommandName="RemoveMember" CommandArgument='<%# Eval("Id") %>' Visible='<%# CanModify %>' />
                </ItemTemplate></asp:TemplateField>
            </Columns>
        </asp:GridView>
        <asp:Panel ID="AddPanel" runat="server">
            <h2>Add member</h2>
            <asp:Label runat="server" AssociatedControlID="MemberKey" Text="Member ID" />
            <asp:TextBox ID="MemberKey" runat="server" MaxLength="32" />
            <asp:Label runat="server" AssociatedControlID="FullName" Text="Full name" />
            <asp:TextBox ID="FullName" runat="server" MaxLength="80" />
            <asp:Label runat="server" AssociatedControlID="Surname" Text="Surname" />
            <asp:TextBox ID="Surname" runat="server" MaxLength="80" />
            <asp:Label runat="server" AssociatedControlID="Email" Text="Email" />
            <asp:TextBox ID="Email" runat="server" MaxLength="254" />
            <div><asp:Button ID="AddButton" runat="server" Text="Add member" OnClick="Add_Click" /></div>
        </asp:Panel>
        <p><asp:HyperLink ID="EditLink" runat="server" Text="Back to team" /></p>
    </asp:Panel>
    <p><a href="Teams.aspx">Back to teams</a></p>
</asp:Content>
