using Microsoft.Playwright;
using TUnit.Playwright;
using static Microsoft.Playwright.Assertions;

namespace LegacyWeb.Playwright.Tests;

public sealed class TeamsTests : PageTest
{
    public override string BrowserName => "chromium";

    private async Task OpenAsync(string path)
    {
        var configured = Environment.GetEnvironmentVariable("FAKE_LEGACY_BASE_URL")
            ?? throw new InvalidOperationException("Set FAKE_LEGACY_BASE_URL to the running, seeded fixture (for example http://localhost:8080).");
        await Page.GotoAsync(new Uri(new Uri(configured), path).ToString());
    }

    [Test]
    public async Task Search_defaults_exclude_future_and_archived_and_escape_literal_asterisks()
    {
        await OpenAsync("/Teams.aspx");
        var grid = Page.Locator("#TeamGrid");
        await Expect(grid).ToContainTextAsync("Alpha Operations");
        await Expect(grid).ToContainTextAsync("Bravo Support");
        await Expect(grid).Not.ToContainTextAsync("Future Research");
        await Expect(grid).Not.ToContainTextAsync("Archived Services");
        await Page.GetByLabel("Include future teams").CheckAsync();
        await Page.GetByLabel("Include archived teams").CheckAsync();
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search", Exact = true }).ClickAsync();
        await Expect(grid).ToContainTextAsync("Future Research");
        await Expect(grid).ToContainTextAsync("Archived Services");
        await Page.GetByLabel("Search by ID or display name").FillAsync("*");
        await Page.GetByRole(AriaRole.Button, new() { Name = "Search", Exact = true }).ClickAsync();
        await Expect(grid).ToContainTextAsync("No teams match your search.");
    }

    [Test]
    public async Task Query_inputs_and_member_summary_follow_the_legacy_behavior()
    {
        await OpenAsync("/Teams.aspx?q=future&withFuture=true");
        await Expect(Page.Locator("#TeamGrid")).ToContainTextAsync("Future Research");
        await Expect(Page.Locator("#TeamGrid")).Not.ToContainTextAsync("Alpha Operations");
        await OpenAsync("/TeamMembers.aspx?id=alpha");
        await Expect(Page.Locator("#SummaryPanel")).ToContainTextAsync("2 member(s)");
        await Expect(Page.Locator("#MemberGrid")).ToContainTextAsync("Alice Example");
        await OpenAsync("/TeamMembers.aspx?id=archive");
        await Expect(Page.Locator("#AddPanel")).ToHaveCountAsync(0);
        await Expect(Page.Locator("#Message")).ToContainTextAsync("read-only");
    }

    [Test]
    public async Task Invalid_and_unknown_team_addresses_are_explicit()
    {
        var configured = Environment.GetEnvironmentVariable("FAKE_LEGACY_BASE_URL")
            ?? throw new InvalidOperationException("Set FAKE_LEGACY_BASE_URL.");
        var invalid = await Page.GotoAsync(new Uri(new Uri(configured), "/TeamEdit.aspx?id=bad%2Cid").ToString());
        await Assert.That(invalid!.Status).IsEqualTo(400);
        var missing = await Page.GotoAsync(new Uri(new Uri(configured), "/TeamMembers.aspx?id=missing-" + Guid.NewGuid().ToString("N")[..8]).ToString());
        await Assert.That(missing!.Status).IsEqualTo(404);
    }

    [Test]
    public async Task Team_journey_persists_changes_and_protects_nonempty_parents()
    {
        var key = "test-" + Guid.NewGuid().ToString("N")[..12];
        var created = false;
        try
        {
            await OpenAsync("/TeamEdit.aspx");
            await Page.GetByLabel("Team ID", new() { Exact = true }).FillAsync(key);
            await Page.GetByLabel("Display name", new() { Exact = true }).FillAsync(" Journey Team ");
            await Page.GetByLabel("Description", new() { Exact = true }).FillAsync("Temporary description");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Save team" }).ClickAsync();
            await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("id=" + key));
            created = true;
            await Expect(Page.Locator("#TeamKey")).ToBeDisabledAsync();
            await Expect(Page.Locator("#DisplayName")).ToHaveValueAsync("Journey Team");
            await Page.GetByLabel("Description", new() { Exact = true }).FillAsync("");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Save team" }).ClickAsync();
            await Expect(Page.Locator("#Description")).ToHaveValueAsync("");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Manage members" }).ClickAsync();
            await Page.GetByLabel("Member ID", new() { Exact = true }).FillAsync("sample");
            await Page.GetByLabel("Full name", new() { Exact = true }).FillAsync("Sample Person");
            await Page.GetByLabel("Surname", new() { Exact = true }).FillAsync("Person");
            await Page.GetByLabel("Email", new() { Exact = true }).FillAsync(" Sample@Example.Test ");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Add member" }).ClickAsync();
            await Expect(Page.Locator("#MemberGrid")).ToContainTextAsync("sample@example.test");
            await Expect(Page.Locator("#SummaryPanel")).ToContainTextAsync("1 member(s)");
            await Page.GetByRole(AriaRole.Link, new() { Name = "Back to team", Exact = true }).ClickAsync();
            await Page.GetByRole(AriaRole.Button, new() { Name = "Delete team" }).ClickAsync();
            await Expect(Page.Locator("#Message")).ToContainTextAsync("Remove all members before deleting");
            await Page.GetByLabel("Status", new() { Exact = true }).SelectOptionAsync("archived");
            await Page.GetByRole(AriaRole.Button, new() { Name = "Save team" }).ClickAsync();
            await Expect(Page.Locator("#Message")).ToContainTextAsync("Remove all members before archiving");
        }
        finally
        {
            if (created)
            {
                // Own only this uniquely named team's data. Never reset the shared seed here.
                await OpenAsync("/TeamMembers.aspx?id=" + key);
                var remove = Page.GetByRole(AriaRole.Link, new() { Name = "Remove", Exact = true });
                while (await remove.CountAsync() > 0)
                {
                    await remove.First.ClickAsync();
                    await Expect(Page.Locator("#MemberGrid")).ToContainTextAsync("This team has no members.");
                }
                await OpenAsync("/TeamEdit.aspx?id=" + key);
                await Page.GetByRole(AriaRole.Button, new() { Name = "Delete team" }).ClickAsync();
                await Expect(Page).ToHaveURLAsync(new System.Text.RegularExpressions.Regex("/Teams\\.aspx$"));
            }
        }
    }
}
