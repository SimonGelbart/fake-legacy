<%@ Page Title="Teams" Language="C#" MasterPageFile="~/Site.Master" AutoEventWireup="true" CodeBehind="Teams.aspx.cs" Inherits="aspnetapp.Teams" %>
<asp:Content ID="TeamsContent" ContentPlaceHolderID="MainContent" runat="server">
    <h1>Teams</h1>
    <asp:Label runat="server" AssociatedControlID="SearchText" Text="Search by ID or display name" />
    <asp:TextBox ID="SearchText" runat="server" MaxLength="80" />
    <div class="checks"><asp:CheckBox ID="WithFuture" runat="server" Text="Include future teams" />
    <asp:CheckBox ID="WithArchived" runat="server" Text="Include archived teams" /></div>
    <asp:Button ID="SearchButton" runat="server" Text="Search" OnClick="Search_Click" />
    <a href="TeamEdit.aspx">Create team</a>
    <asp:Label ID="Message" runat="server" CssClass="error" role="status" />
    <asp:GridView ID="TeamGrid" runat="server" AutoGenerateColumns="false" EmptyDataText="No teams match your search.">
        <Columns>
            <asp:BoundField DataField="Id" HeaderText="ID" />
            <asp:BoundField DataField="Name" HeaderText="Display name" />
            <asp:BoundField DataField="Status" HeaderText="Status" />
            <asp:BoundField DataField="StartDate" HeaderText="Start date" />
            <asp:HyperLinkField DataNavigateUrlFields="Id" DataNavigateUrlFormatString="TeamEdit.aspx?id={0}" Text="Edit" />
            <asp:HyperLinkField DataNavigateUrlFields="Id" DataNavigateUrlFormatString="TeamMembers.aspx?id={0}" Text="Members" />
        </Columns>
    </asp:GridView>
</asp:Content>
