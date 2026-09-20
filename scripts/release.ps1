[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [string]$Version,

    [string]$Repository = 'TamNhien/ITDeviceManager',

    [string]$Message
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

function Require-Command([string]$Name, [string]$InstallHint) {
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' was not found. $InstallHint"
    }
}

function Invoke-Native([string]$Command, [string[]]$Arguments) {
    & $Command @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "$Command failed with exit code $LASTEXITCODE."
    }
}

function Read-ProjectVersion {
    $projectFile = Join-Path $root 'ITDeviceManager\ITDeviceManager.csproj'
    $content = Get-Content -Raw -LiteralPath $projectFile
    $match = [regex]::Match($content, '<Version>(?<v>[^<]+)</Version>')
    if (-not $match.Success) {
        throw 'Could not read <Version> from ITDeviceManager.csproj.'
    }
    return $match.Groups['v'].Value.Trim()
}

function Set-ProjectVersion([string]$NewVersion) {
    $projectFile = Join-Path $root 'ITDeviceManager\ITDeviceManager.csproj'
    $content = Get-Content -Raw -LiteralPath $projectFile
    $content = [regex]::new('<Version>[^<]+</Version>').Replace($content, "<Version>$NewVersion</Version>", 1)
    $content = [regex]::new('<AssemblyVersion>[^<]+</AssemblyVersion>').Replace($content, "<AssemblyVersion>$NewVersion.0</AssemblyVersion>", 1)
    $content = [regex]::new('<FileVersion>[^<]+</FileVersion>').Replace($content, "<FileVersion>$NewVersion.0</FileVersion>", 1)
    [IO.File]::WriteAllText($projectFile, $content, (New-Object Text.UTF8Encoding($false)))

    $versionFile = Join-Path $root 'VERSION.txt'
    $versionText = @"
ITDeviceManager V$NewVersion
Upgrade-in-place target:
D:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager
"@
    [IO.File]::WriteAllText($versionFile, $versionText.TrimStart(), (New-Object Text.UTF8Encoding($false)))

    $readme = Join-Path $root 'README.md'
    if (Test-Path $readme) {
        $readmeText = Get-Content -Raw -LiteralPath $readme
        $readmeText = [regex]::new('^# IT Device Manager - V[^\r\n]+', [Text.RegularExpressions.RegexOptions]::Multiline).Replace($readmeText, "# IT Device Manager - V$NewVersion", 1)
        [IO.File]::WriteAllText($readme, $readmeText, (New-Object Text.UTF8Encoding($false)))
    }
}

Require-Command dotnet 'Install the .NET 10 SDK.'
Require-Command git 'Install Git for Windows.'
Require-Command gh 'Install GitHub CLI, then run: gh auth login'

Invoke-Native 'gh' @('auth', 'status')

if (-not (Test-Path '.git')) {
    Write-Host '[Git] Initializing repository...' -ForegroundColor Cyan
    Invoke-Native 'git' @('init')
}

Invoke-Native 'git' @('branch', '-M', 'main')

if (Test-Path '.env') {
    & git check-ignore -q -- .env
    if ($LASTEXITCODE -ne 0) {
        throw '.env exists but is not ignored. Release aborted to protect secrets.'
    }

    $trackedEnv = @(& git ls-files -- .env)
    if ($LASTEXITCODE -ne 0) {
        throw 'git ls-files failed while checking .env.'
    }
    if ($trackedEnv.Count -gt 0) {
        throw '.env is tracked by Git. Run "git rm --cached .env" before releasing.'
    }
}

$userName = ([string](& git config user.name)).Trim()
$userEmail = ([string](& git config user.email)).Trim()
if ([string]::IsNullOrWhiteSpace($userName) -or [string]::IsNullOrWhiteSpace($userEmail)) {
    throw @'
Git user.name/user.email are not configured.
Example:
  git config --global user.name "TamNhien"
  git config --global user.email "your-email@example.com"
'@
}

if ([string]::IsNullOrWhiteSpace($Version)) {
    $Version = Read-ProjectVersion
}
$Version = $Version.Trim()
if ($Version.StartsWith('v', [StringComparison]::OrdinalIgnoreCase)) {
    $Version = $Version.Substring(1)
}
if ($Version -notmatch '^\d+\.\d+\.\d+$') {
    throw 'Version must use X.Y.Z format, for example 1.2.2.'
}

$tag = "v$Version"
Write-Host "[Release] Preparing $tag for $Repository" -ForegroundColor Cyan
Set-ProjectVersion $Version

Write-Host '[Release] Restoring and building Release...' -ForegroundColor Cyan
Invoke-Native 'dotnet' @('restore', '.\ITDeviceManager.sln')
Invoke-Native 'dotnet' @('build', '.\ITDeviceManager.sln', '-c', 'Release', '--no-restore')

Write-Host '[Release] Staging source...' -ForegroundColor Cyan
Invoke-Native 'git' @('add', '-A')

& git diff --cached --quiet
$hasStagedChanges = ($LASTEXITCODE -ne 0)
if ($hasStagedChanges) {
    if ([string]::IsNullOrWhiteSpace($Message)) {
        $Message = "Release $tag"
    }
    Invoke-Native 'git' @('commit', '-m', $Message)
}
else {
    Write-Host '[Git] No source changes to commit.'
}

& git rev-parse --verify HEAD *> $null
if ($LASTEXITCODE -ne 0) {
    throw 'Repository has no commit to release.'
}

# Create the GitHub repository automatically if it does not exist yet.
& gh repo view $Repository --json nameWithOwner *> $null
if ($LASTEXITCODE -ne 0) {
    Write-Host "[GitHub] Creating public repository $Repository..." -ForegroundColor Cyan
    Invoke-Native 'gh' @('repo', 'create', $Repository, '--public', '--source', '.', '--description', 'C# WinForms + Entity Framework IT device management coursework project')
}

$origin = ''
& git remote get-url origin *> $null
if ($LASTEXITCODE -eq 0) {
    $origin = ([string](& git remote get-url origin)).Trim()
}

if ([string]::IsNullOrWhiteSpace($origin)) {
    Invoke-Native 'git' @('remote', 'add', 'origin', "https://github.com/$Repository.git")
}
elseif ($origin -notmatch [regex]::Escape($Repository)) {
    throw "The existing origin remote points to '$origin', not '$Repository'. Refusing to push to the wrong repository."
}

# Refuse to overwrite an existing tag/release.
& git rev-parse -q --verify "refs/tags/$tag" *> $null
if ($LASTEXITCODE -eq 0) {
    throw "Local tag $tag already exists. Use a new version number."
}

$remoteTag = ([string](& git ls-remote --tags origin "refs/tags/$tag")).Trim()
if (-not [string]::IsNullOrWhiteSpace($remoteTag)) {
    throw "Remote tag $tag already exists. Use a new version number."
}

& gh release view $tag --repo $Repository *> $null
if ($LASTEXITCODE -eq 0) {
    throw "GitHub release $tag already exists. Use a new version number."
}

Write-Host '[Release] Building distributable package...' -ForegroundColor Cyan
$distRoot = Join-Path $root "dist\$tag"
$publishDir = Join-Path $distRoot 'publish-win-x64'
if (Test-Path $distRoot) {
    Remove-Item -Recurse -Force $distRoot
}
New-Item -ItemType Directory -Force -Path $publishDir | Out-Null

Invoke-Native 'dotnet' @(
    'publish', '.\ITDeviceManager\ITDeviceManager.csproj',
    '-c', 'Release',
    '-r', 'win-x64',
    '--self-contained', 'false',
    '-o', $publishDir
)

Copy-Item '.\README.md' $publishDir -Force
Copy-Item '.\VERSION.txt' $publishDir -Force
Copy-Item '.\.env.example' $publishDir -Force

$appZip = Join-Path $distRoot "ITDeviceManager-$tag-win-x64.zip"
$sourceZip = Join-Path $distRoot "ITDeviceManager-$tag-source.zip"
$checksums = Join-Path $distRoot 'SHA256SUMS.txt'

Compress-Archive -Path (Join-Path $publishDir '*') -DestinationPath $appZip -CompressionLevel Optimal -Force
Invoke-Native 'git' @('archive', '--format=zip', "--output=$sourceZip", 'HEAD')

$checksumLines = @()
foreach ($asset in @($appZip, $sourceZip)) {
    $hash = (Get-FileHash -Algorithm SHA256 -LiteralPath $asset).Hash.ToLowerInvariant()
    $checksumLines += "$hash  $(Split-Path -Leaf $asset)"
}
[IO.File]::WriteAllLines($checksums, $checksumLines, (New-Object Text.UTF8Encoding($false)))

Write-Host '[GitHub] Pushing main branch...' -ForegroundColor Cyan
Invoke-Native 'git' @('push', '-u', 'origin', 'main')

Write-Host "[Git] Creating tag $tag..." -ForegroundColor Cyan
Invoke-Native 'git' @('tag', '-a', $tag, '-m', "ITDeviceManager $tag")
Invoke-Native 'git' @('push', 'origin', $tag)

Write-Host '[GitHub] Creating release and uploading assets...' -ForegroundColor Cyan
Invoke-Native 'gh' @(
    'release', 'create', $tag,
    $appZip, $sourceZip, $checksums,
    '--repo', $Repository,
    '--verify-tag',
    '--title', "ITDeviceManager $tag",
    '--generate-notes'
)

Write-Host ''
Write-Host "RELEASE COMPLETED: $tag" -ForegroundColor Green
Write-Host "Repository: https://github.com/$Repository"
Write-Host "Release:    https://github.com/$Repository/releases/tag/$tag"
