# Evaluator's checklist — keep separate from discovery inputs

Evaluate evidence and coverage, not identical wording. A source-only run should
identify behavior below without claiming it has observed a browser or live LDAP.
This is an answer key for the fixture, not a target specification for its migration.

| Area | Expected legacy finding | Source |
|---|---|---|
| Boundary | Teams list, team create/edit/delete, child-member page and shared summary control | `Teams.aspx`, links, `TeamEdit.aspx`, `TeamMembers.aspx`, registered control |
| Lifecycle | Initial data load guarded by `!IsPostBack`; search/save and status postbacks have distinct effects | page `Page_Load` methods and handlers |
| Defaults | Search starts empty; future and archived teams excluded; default create status is active | `Teams.aspx`, `Teams.aspx.cs`, `TeamEdit.aspx` |
| Query string | `q` initializes search; `withFuture=true` is case-insensitive; `id` selects existing team; absent `id` creates; invalid IDs return 400, missing team 404 | page code-behind |
| Search | Escaped substring OR filter over `ou` and `displayName`; `fakeTeam` object class; archived exclusion in LDAP | `Teams.BindTeams`, `LegacyDirectory.EscapeFilter` |
| Scope | Teams and members searched with `OneLevel`; individual parent with `Base`; not arbitrary subtree search | all `SearchRequest` calls |
| Future | Optional `fakeStartDate` parsed as ISO calendar date; later than UTC today filtered in C#, regardless of status; missing/today included | `LegacyRules.Future`, `Teams.BindTeams` |
| Ordering | Name then ID sorted in `DataView`; no LDAP server sort and no pagination | grid-binding methods |
| Identity | Team `ou=<key>`; child `uid=<team>.<member>`; immutable team key on edit; key normalization only on create/add | `LegacyDirectory`, edit/member handlers |
| Validation | Key pattern, name lengths, description length, exact date format, allowed statuses; planned status requires future date | `LegacyRules.TeamError` |
| Conditional UI | Planned hint toggles after status postback; changing status alone does not save; delete/members links absent on create | `TeamEdit` lifecycle and markup |
| State rules | Cannot archive or delete a team with children; all child object classes considered; OpenLDAP rejects non-leaf delete too | `TeamEdit.Save_Click`, `Delete_Click` |
| Update semantics | Trim inputs; replace listed attributes; empty description/start date removes optional attribute; repeated clears allowed | `TeamEdit.Save_Click`, `LegacyDirectory.Replace` |
| Child rules | Members require ID, full name, surname and email; email is normalized; duplicate ID within team rejected; no member edit workflow | `TeamMembers`, `LegacyRules.MemberError` |
| Read-only | Archived teams can be viewed; add/remove suppressed and server-side guarded; parent state re-read on each request | `TeamMembers.Page_Load`, handlers |
| LDAP mapping | Team uses organizationalUnit + custom auxiliary fakeTeam; member uses inetOrgPerson; employeeNumber is local member ID; cn/uid prefixed because base image enforces global uniqueness | code-behind, schema, Docker base image |
| Shared control | Performs its own parent/count queries at pre-render; summary refreshes from LDAP rather than trusting view state | `Controls/TeamSummary.ascx.cs` |
| Data effects | Adds, modifies and deletes committed straight from Web Forms; no API, application service or transaction across requests | `SendRequest` calls |
| Outcomes | Save redirects to persisted edit page; add/remove redirects to member page; deletion returns to list; empty-state messages | markup and event handlers |
| Failures | Duplicate records have specific messages; LDAP read failures produce 503 on pages; summary failure has its own fallback | exception branches |
| Limitations | Runtime/IIS behavior, actual server matching and outage behavior require execution; no AD ACL/backlink/replication claims | evidence limits |

Record each row as **found with correct evidence**, **partial**, **missed**, or
**incorrect**. Keep unsupported claims separate from omissions. The evaluator
should also inspect newly added behavior if the fixture is changed.

## Small follow-up experiments

1. Move a rule to another helper; rerun discovery and check it follows the call.
2. Add a linked child page; rerun and check the coverage inventory expands.
3. Change an archived/future condition; compare the new findings with the diff.
4. Stop LDAP; compare runtime limitations with source-only findings.
5. Introduce a mismatch between UI choices and server validation; expect an explicit discrepancy.

## Deliberate legacy constraints to discuss during target definition

There is no authentication, optimistic concurrency, cross-request transaction,
paging or AD-specific security model. Team and member forms preserve typical
code-behind coupling. These are documented limits of the local fixture; do not
silently invent them as capabilities or automatically carry them into a target API.
