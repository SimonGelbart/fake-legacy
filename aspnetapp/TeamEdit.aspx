<%@ Page Title="Edit team" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="TeamEdit.aspx.cs" Inherits="aspnetapp.TeamEdit" %>
<%@ Register Src="~/Controls/TeamSummary.ascx" TagPrefix="legacy" TagName="TeamSummary" %>
<asp:Content ID="EditContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1><asp:Literal ID="Heading" runat="server" Mode="Encode" /></h1>
    <legacy:TeamSummary ID="Summary" runat="server" />
    <asp:Label ID="Message" runat="server" CssClass="error" role="status" />
    <asp:Panel ID="Editor" runat="server">
        <asp:Label runat="server" AssociatedControlID="TeamKey" Text="Team ID" />
        <asp:TextBox ID="TeamKey" runat="server" MaxLength="32" />
        <asp:Label runat="server" AssociatedControlID="DisplayName" Text="Display name" />
        <asp:TextBox ID="DisplayName" runat="server" MaxLength="80" />
        <asp:Label runat="server" AssociatedControlID="Status" Text="Status" />
        <asp:DropDownList ID="Status" runat="server" AutoPostBack="true" OnSelectedIndexChanged="Status_Changed">
            <asp:ListItem Value="active">Active</asp:ListItem><asp:ListItem Value="planned">Planned</asp:ListItem><asp:ListItem Value="archived">Archived</asp:ListItem>
        </asp:DropDownList>
        <asp:Label runat="server" AssociatedControlID="StartDate" Text="Start date (yyyy-MM-dd, optional unless planned)" />
        <asp:TextBox ID="StartDate" runat="server" MaxLength="10" />
        <asp:Label ID="PlannedHint" runat="server" CssClass="help" Text="A planned team must start after today (UTC)." />
        <asp:Label runat="server" AssociatedControlID="Description" Text="Description" />
        <asp:TextBox ID="Description" runat="server" TextMode="MultiLine" Rows="3" MaxLength="500" />
        <div><asp:Button ID="SaveButton" runat="server" Text="Save team" OnClick="Save_Click" />
        <asp:Button ID="DeleteButton" runat="server" Text="Delete team" OnClick="Delete_Click" /></div>
        <asp:HyperLink ID="MembersLink" runat="server" Text="Manage members" />
    </asp:Panel>
    <p><a href="Teams.aspx">Back to teams</a></p>
</asp:Content>
