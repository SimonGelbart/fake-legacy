# Fake legacy: Web Forms → LDAP

A runnable fictional legacy resource for testing an incremental migration workflow.
The ASPX code-behind talks **directly to LDAP**. The starting point contains no API
or V2 pages: those are the work to be performed by the migration workflow later.

The project is based on the Web Forms structure in
[playright-test](https://github.com/SimonGelbart/playright-test) and the OpenLDAP
container approach in `ldap-testcontainer`. See [source baselines](THIRD_PARTY_NOTICES.md).
All directory definitions, data and credentials here are fictional local fixtures.

## Start the baseline

Use Windows for the ASP.NET Framework application, with Docker Desktop in
**Linux-container mode** for LDAP. This avoids trying to run Linux LDAP and a
Windows ASP.NET container in the same Docker engine.

Requirements:

- Visual Studio or Build Tools with ASP.NET/web build tools and .NET Framework 4.8 targeting pack.
- IIS Express (normally installed with the Visual Studio ASP.NET workload).
- Docker Desktop with Linux containers, or a reachable Linux Docker/Podman machine.
- PowerShell. Python is optional for static checks; .NET 10 is only needed for browser tests.

From the repository root, start LDAP:

```powershell
docker compose up --build --wait --wait-timeout 120
```

In a second terminal, build and start the app:

```powershell
.\scripts\Start-Legacy.ps1
```

Open **http://localhost:8080/Teams.aspx**. The default bind is
`cn=admin,dc=example,dc=test` at `127.0.0.1:1389`, with the public fixture password
`FakeLegacyOnly!1`. These match the Compose defaults and `Web.config`.

If LDAP runs on another machine, set that host explicitly:

```powershell
.\scripts\Start-Legacy.ps1 -LdapHost 192.0.2.10 -LdapPort 1389
```

The example IP must be replaced with the reachable machine. Compose binds to
loopback by default; remote access needs an intentional port-binding change on
that test machine. Do not expose this unauthenticated fixture to the Internet.
For a changed fixture password/port, copy `.env.example` to `.env` for Compose
and pass matching `-LdapPassword`/`-LdapPort` values to the start script.
IIS Express inherits environment settings when started; restart it after changes.

You may also open `aspnetapp.sln` in Visual Studio and run the web application.
Its exact ASPX URLs remain enabled; the upstream FriendlyUrls/bundling packages
are omitted, so the baseline application needs no external NuGet packages.

## What to exercise

The resource has three pages and one shared control:

| Page | Workflows |
|---|---|
| `Teams.aspx` | Search, include future/archived teams, navigate to edit or members |
| `TeamEdit.aspx` | Create, view/edit, change status and delete a team |
| `TeamMembers.aspx` | List, add and remove child members; archived teams are read-only |
| `Controls/TeamSummary.ascx` | Read team name and count children directly from LDAP |

The seed contains Alpha Operations (two members), Bravo Support (empty), Future
Research (planned, starts 2099-01-01), and Archived Services. Normally only Alpha
and Bravo appear. The seed's future date remains a future example until 2099;
update it if evaluating beyond that date.

Try creating a team, changing its description, adding a member, and then deleting
or archiving it. Remove the member before retrying. Include future/archived teams
and search for a literal `*` to exercise LDAP filter escaping. Changes persist.

Connection/encoding helpers live in `Infrastructure/`; the queries, directory
checks and writes remain in the page/control code-behind. Validation is partly
in a helper and partly dependent on actual directory contents.

## Use it as a migration baseline

1. Give discovery [the resource brief](docs/resource-brief.md), `aspnetapp/`, `ldap/` and `compose.yaml`.
2. Produce the legacy snapshot and coverage inventory. Review them before target definition.
3. Evaluate against [the separate expected-findings checklist](evaluation/expected-discovery.md).
4. Define the target, write acceptance scenarios, and implement the API incrementally.
5. Later duplicate/connect V2 pages and compare behavior with the preserved legacy.

For a blind evaluation, exclude `evaluation/`, `tests/` and this README's behavior
description from the discovery agent's accessible directory. The brief explains
that boundary. The expected-findings checklist is an evaluator's answer key.

Only the legacy baseline is implemented in this repository. There is no invented
target contract, API stub or automatic migration engine to constrain later decisions.

## Verification

Portable source/seed check:

```powershell
python scripts/Check-Fixture.py
```

Windows code and markup checks, from a Developer PowerShell:

```powershell
msbuild tests/LegacyRules.Tests/LegacyRules.Tests.csproj /p:Configuration=Release
.\tests\LegacyRules.Tests\bin\LegacyRules.Tests.exe
& "$env:WINDIR\Microsoft.NET\Framework64\v4.0.30319\aspnet_compiler.exe" -v / -p "$pwd\aspnetapp"
```

LDAP integration check, against an unmodified seed (Git Bash/Linux):

```bash
bash scripts/Test-Ldap.sh
```

Browser checks reuse the baseline's **TUnit + Playwright** stack and run against
your already-running app and LDAP. They do not start containers automatically:

```powershell
dotnet build tests/LegacyWeb.Playwright.Tests
pwsh tests/LegacyWeb.Playwright.Tests/bin/Debug/net10.0/playwright.ps1 install chromium
$env:FAKE_LEGACY_BASE_URL = 'http://localhost:8080'
dotnet run --project tests/LegacyWeb.Playwright.Tests
```

Browser tests require the original seed. The write journey uses a unique team ID
and cleans up only its own records. If an interrupted run leaves a `test-*` team,
remove that team's children and the team manually, or reset this disposable directory.

CI compiles the Windows application, ASPX/ASCX/master markup, rule checks and browser
scenarios, and runs a real LDAP smoke check on Linux. The full browser journey is
a separate execution against a running Windows app plus reachable Linux LDAP;
compiling browser tests alone does not verify that journey.

## Stop and reset

Stop IIS Express with Ctrl+C. Stop LDAP while keeping edits:

```powershell
docker compose down
```

Restore the original seed by removing **this project's disposable LDAP volumes**:

```powershell
docker compose down --volumes
docker compose up --build --wait --wait-timeout 120
```

Reset discards edits made through the fixture. There is no real directory data.

## Optional Windows container

The root `Dockerfile` retains the upstream Windows SDK/IIS container pattern.
Run it on a Windows-container engine and connect to LDAP on a **separate Linux
engine**. `localhost` inside the Windows container refers to that container.

```powershell
docker build -t fake-legacy-web .
docker run --rm -p 127.0.0.1:8080:80 -e FAKE_LDAP_HOST=host.docker.internal -e FAKE_LDAP_PORT=1389 fake-legacy-web
```

`host.docker.internal` works only when the host can reach the separate LDAP service
on that port; otherwise use its reachable host address. This optional route is
not needed for the default IIS Express setup and is not covered by hosted CI.

## LDAP scope

The custom schema is a tiny synthetic auxiliary class, using documentation OIDs.
It follows the same OpenLDAP image approach as the LDAP repository without needing
private AD LDS access. It supports fixture CRUD, mappings, LDAP filters/scopes and
hierarchy. It does not emulate AD ACLs, backlinks, replication or constructed
attributes. A later test with the real application still needs its actual schema
and directory-specific behavior.
