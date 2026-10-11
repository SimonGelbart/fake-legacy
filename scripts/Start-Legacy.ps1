param(
    [string]$LdapHost = "127.0.0.1",
    [int]$LdapPort = 1389,
    [string]$LdapPassword = "FakeLegacyOnly!1",
    [int]$Port = 8080
)
$ErrorActionPreference = "Stop"
$repoRoot = Split-Path -Parent $PSScriptRoot
$vswhere = "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe"
$msbuild = Get-Command msbuild -ErrorAction SilentlyContinue
if ($msbuild) { $buildPath = $msbuild.Source }
elseif (Test-Path $vswhere) { $buildPath = & $vswhere -latest -products * -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe | Select-Object -First 1 }
if (!$buildPath) { throw "Install Visual Studio Build Tools with ASP.NET/web development and the .NET Framework 4.8 targeting pack." }
& $buildPath "$repoRoot\aspnetapp.sln" /p:Configuration=Debug /verbosity:minimal
if ($LASTEXITCODE -ne 0) { throw "Web Forms build failed." }
$iis = "${env:ProgramFiles}\IIS Express\iisexpress.exe"
if (!(Test-Path $iis)) { $iis = "${env:ProgramFiles(x86)}\IIS Express\iisexpress.exe" }
if (!(Test-Path $iis)) { throw "Install IIS Express (available with Visual Studio's ASP.NET workload)." }
$env:FAKE_LDAP_HOST = $LdapHost
$env:FAKE_LDAP_PORT = "$LdapPort"
$env:FAKE_LDAP_PASSWORD = $LdapPassword
Write-Host "Open http://localhost:$Port/Teams.aspx (Ctrl+C to stop)."
& $iis "/path:$repoRoot\aspnetapp" "/port:$Port"
if ($LASTEXITCODE -ne 0) { throw "IIS Express exited with code $LASTEXITCODE." }
