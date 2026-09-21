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

function Get-NativeText {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Command,

        [Parameter(Mandatory = $true)]
        [string[]]$Arguments,

        [switch]$AllowFailure
    )

    # Native commands are allowed to legitimately return no stdout (for example,
    # git ls-remote when a tag does not exist). Always normalize that situation
    # to an empty string instead of calling .Trim() on $null.
    $oldErrorActionPreference = $ErrorActionPreference
    try {
        $ErrorActionPreference = 'Continue'
        $lines = @(& $Command @Arguments 2>$null)
        $exitCode = $LASTEXITCODE
    }
    finally {
        $ErrorActionPreference = $oldErrorActionPreference
    }

    if (($exitCode -ne 0) -and (-not $AllowFailure)) {
        throw "$Command failed with exit code $exitCode."
    }

    if ($lines.Count -eq 0) {
        return ''
    }

    return (($lines | ForEach-Object { [string]$_ }) -join [Environment]::NewLine).Trim()
}

function Test-NativeSuccess {
    param(
        [Parameter(Mandatory = $true)]
        [string]$Command,

        [Parameter(Mandatory = $true)]
        [string[]]$Arguments
    )

    $oldErrorActionPreference = $ErrorActionPreference
    try {
        $ErrorActionPreference = 'Continue'
        & $Command @Arguments *> $null
        return ($LASTEXITCODE -eq 0)
    }
    finally {
        $ErrorActionPreference = $oldErrorActionPreference
    }
}

function Get-Sha256Hex([string]$Path) {
    $resolved = (Resolve-Path -LiteralPath $Path).Path
    $stream = [System.IO.File]::OpenRead($resolved)
    try {
        $sha256 = [System.Security.Cryptography.SHA256]::Create()
        try {
            $bytes = $sha256.ComputeHash($stream)
        }
        finally {
            $sha256.Dispose()
        }
    }
    finally {
        $stream.Dispose()
    }

    return ([System.BitConverter]::ToString($bytes)).Replace('-', '').ToLowerInvariant()
}

function Read-Utf8Text([string]$Path) {
    $resolved = (Resolve-Path -LiteralPath $Path).Path
    $utf8Strict = New-Object System.Text.UTF8Encoding($false, $true)
    try {
        return [System.IO.File]::ReadAllText($resolved, $utf8Strict)
    }
    catch {
        throw "File '$resolved' is not valid UTF-8. Convert it to UTF-8 before releasing. $($_.Exception.Message)"
    }
}

function Write-Utf8Text([string]$Path, [string]$Text) {
    $utf8NoBom = New-Object System.Text.UTF8Encoding($false)
    [System.IO.File]::WriteAllText($Path, $Text, $utf8NoBom)
}

function Assert-NoReadmeMojibake([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path)) { return }
    $text = Read-Utf8Text $Path

    # Common byte-pattern artifacts caused by UTF-8 text being decoded as an ANSI code page.
    $badCodePoints = @(
        [char]0x00C3, # Ã
        [char]0x00C2, # Â
        [char]0x00C4  # Ä
    )
    foreach ($ch in $badCodePoints) {
        if ($text.IndexOf($ch) -ge 0) {
            throw "README.md appears to contain mojibake/encoding corruption. Release aborted before git commit."
        }
    }
}

function Get-ForbiddenDatabaseArtifacts([string[]]$Paths) {
    if ($null -eq $Paths) { return @() }

    return @($Paths | Where-Object {
        (-not [string]::IsNullOrWhiteSpace($_)) -and
        ($_ -match '(?i)((^|/)(DatabaseFiles|Backups|QR)/|\.(mdf|ldf|ndf|bak|trn)$)')
    })
}

function Assert-NoTrackedDatabaseArtifacts {
    $trackedText = Get-NativeText 'git' @('ls-files')
    $trackedPaths = if ([string]::IsNullOrWhiteSpace($trackedText)) {
        @()
    }
    else {
        @($trackedText -split "`r?`n")
    }

    $forbidden = Get-ForbiddenDatabaseArtifacts $trackedPaths
    if ($forbidden.Count -gt 0) {
        $details = ($forbidden | ForEach-Object { "  - $_" }) -join [Environment]::NewLine
        throw @"
Local database/backup/QR artifacts are currently tracked by Git. Release aborted to prevent publishing operational data.
Remove them from Git tracking while keeping the local files, then rewrite/purge exposed history if necessary:
$details
"@
    }
}

function Assert-NoStagedDatabaseArtifacts {
    $stagedText = Get-NativeText 'git' @(
        'diff', '--cached', '--name-only', '--diff-filter=ACMRTUXB', '--'
    )
    $stagedPaths = if ([string]::IsNullOrWhiteSpace($stagedText)) {
        @()
    }
    else {
        @($stagedText -split "`r?`n")
    }

    $forbidden = Get-ForbiddenDatabaseArtifacts $stagedPaths
    if ($forbidden.Count -gt 0) {
        $details = ($forbidden | ForEach-Object { "  - $_" }) -join [Environment]::NewLine
        throw @"
Local database/backup/QR artifacts are staged for commit. Release aborted before commit/push:
$details
"@
    }
}

function Read-ProjectVersion {
    $projectFile = Join-Path $root 'ITDeviceManager\ITDeviceManager.csproj'
    $content = Read-Utf8Text $projectFile
    $match = [regex]::Match($content, '<Version>(?<v>[^<]+)</Version>')
    if (-not $match.Success) {
        throw 'Could not read <Version> from ITDeviceManager.csproj.'
    }
    return $match.Groups['v'].Value.Trim()
}

function Set-ProjectVersion([string]$NewVersion) {
    $projectFile = Join-Path $root 'ITDeviceManager\ITDeviceManager.csproj'
    $content = Read-Utf8Text $projectFile
    $content = [regex]::new('<Version>[^<]+</Version>').Replace($content, "<Version>$NewVersion</Version>", 1)
    $content = [regex]::new('<AssemblyVersion>[^<]+</AssemblyVersion>').Replace($content, "<AssemblyVersion>$NewVersion.0</AssemblyVersion>", 1)
    $content = [regex]::new('<FileVersion>[^<]+</FileVersion>').Replace($content, "<FileVersion>$NewVersion.0</FileVersion>", 1)
    Write-Utf8Text $projectFile $content

    $versionFile = Join-Path $root 'VERSION.txt'
    # Keep VERSION.txt LF-normalized even though this PowerShell script itself uses CRLF.
    # This avoids Git staging warnings on Windows when .gitattributes declares eol=lf.
    $versionText = "ITDeviceManager V$NewVersion`nUpgrade-in-place target:`nD:\LienThongDH\Lap_trinh_tren_moi_truong_window_A01\ITDeviceManager`n"
    Write-Utf8Text $versionFile $versionText

    $readme = Join-Path $root 'README.md'
    if (Test-Path $readme) {
        $readmeText = Read-Utf8Text $readme
        if ($null -ne $readmeText) {
            $readmeText = [regex]::new('^# IT Device Manager - V[^\r\n]+', [Text.RegularExpressions.RegexOptions]::Multiline).Replace($readmeText, "# IT Device Manager - V$NewVersion", 1)
            Write-Utf8Text $readme $readmeText
        }
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
    if (-not (Test-NativeSuccess 'git' @('check-ignore', '-q', '--', '.env'))) {
        throw '.env exists but is not ignored. Release aborted to protect secrets.'
    }

    $trackedEnv = Get-NativeText 'git' @('ls-files', '--', '.env')
    if (-not [string]::IsNullOrWhiteSpace($trackedEnv)) {
        throw '.env is tracked by Git. Run "git rm --cached .env" before releasing.'
    }
}

Assert-NoTrackedDatabaseArtifacts

$userName = Get-NativeText 'git' @('config', '--get', 'user.name') -AllowFailure
$userEmail = Get-NativeText 'git' @('config', '--get', 'user.email') -AllowFailure
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
    throw 'Version must use X.Y.Z format, for example 1.2.6.'
}

$tag = "v$Version"
Write-Host "[Release] Preparing $tag for $Repository" -ForegroundColor Cyan
Set-ProjectVersion $Version
Assert-NoReadmeMojibake (Join-Path $root 'README.md')

Write-Host '[Release] Restoring and building Release...' -ForegroundColor Cyan
Invoke-Native 'dotnet' @('restore', '.\ITDeviceManager.sln')
Invoke-Native 'dotnet' @('build', '.\ITDeviceManager.sln', '-c', 'Release', '--no-restore')

Assert-NoReadmeMojibake (Join-Path $root 'README.md')
Write-Host '[Release] Staging source...' -ForegroundColor Cyan
Invoke-Native 'git' @('add', '-A')
Assert-NoStagedDatabaseArtifacts

$hasStagedChanges = -not (Test-NativeSuccess 'git' @('diff', '--cached', '--quiet'))
if ($hasStagedChanges) {
    if ([string]::IsNullOrWhiteSpace($Message)) {
        $Message = "Release $tag"
    }
    Invoke-Native 'git' @('commit', '-m', $Message)
}
else {
    Write-Host '[Git] No source changes to commit.'
}

if (-not (Test-NativeSuccess 'git' @('rev-parse', '--verify', 'HEAD'))) {
    throw 'Repository has no commit to release.'
}

# Create the GitHub repository automatically if it does not exist yet.
$repoExists = Test-NativeSuccess 'gh' @('repo', 'view', $Repository, '--json', 'nameWithOwner')
if (-not $repoExists) {
    Write-Host "[GitHub] Creating public repository $Repository..." -ForegroundColor Cyan
    Invoke-Native 'gh' @(
        'repo', 'create', $Repository,
        '--public',
        '--source', '.',
        '--description', 'C# WinForms + Entity Framework IT device management coursework project'
    )
}

$origin = Get-NativeText 'git' @('remote', 'get-url', 'origin') -AllowFailure
if ([string]::IsNullOrWhiteSpace($origin)) {
    Invoke-Native 'git' @('remote', 'add', 'origin', "https://github.com/$Repository.git")
}
elseif ($origin -notmatch [regex]::Escape($Repository)) {
    throw "The existing origin remote points to '$origin', not '$Repository'. Refusing to push to the wrong repository."
}

# Refuse to overwrite an existing tag/release.
if (Test-NativeSuccess 'git' @('rev-parse', '-q', '--verify', "refs/tags/$tag")) {
    throw "Local tag $tag already exists. Use a new version number."
}

$remoteTag = Get-NativeText 'git' @('ls-remote', '--tags', 'origin', "refs/tags/$tag") -AllowFailure
if (-not [string]::IsNullOrWhiteSpace($remoteTag)) {
    throw "Remote tag $tag already exists. Use a new version number."
}

if (Test-NativeSuccess 'gh' @('release', 'view', $tag, '--repo', $Repository)) {
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

foreach ($supportFile in @('README.md', 'VERSION.txt', '.env.example')) {
    $source = Join-Path $root $supportFile
    if (Test-Path $source) {
        Copy-Item $source $publishDir -Force
    }
}

$appZip = Join-Path $distRoot "ITDeviceManager-$tag-win-x64.zip"
$sourceZip = Join-Path $distRoot "ITDeviceManager-$tag-source.zip"
$checksums = Join-Path $distRoot 'SHA256SUMS.txt'

Compress-Archive -Path (Join-Path $publishDir '*') -DestinationPath $appZip -CompressionLevel Optimal -Force
Invoke-Native 'git' @('archive', '--format=zip', "--output=$sourceZip", 'HEAD')

$checksumLines = @()
foreach ($asset in @($appZip, $sourceZip)) {
    $hash = Get-Sha256Hex $asset
    $checksumLines += "$hash  $(Split-Path -Leaf $asset)"
}
[IO.File]::WriteAllLines($checksums, $checksumLines, (New-Object Text.UTF8Encoding($false)))

Write-Host '[GitHub] Pushing main branch...' -ForegroundColor Cyan
Invoke-Native 'git' @('push', '-u', 'origin', 'main')

# Configure GitHub Pages from main:/docs. The password-reset email uses this HTTPS
# bridge because Gmail/webmail blocks custom URI schemes such as itdevicemanager://.
Write-Host '[GitHub Pages] Configuring reset-password bridge...' -ForegroundColor Cyan
$pagesPayload = '{"source":{"branch":"main","path":"/docs"}}'
$pagesPayloadPath = Join-Path ([IO.Path]::GetTempPath()) ("itdm-pages-" + [Guid]::NewGuid().ToString('N') + '.json')
try {
    [IO.File]::WriteAllText($pagesPayloadPath, $pagesPayload, (New-Object Text.UTF8Encoding($false)))

    $pagesConfigured = $false
    $oldErrorActionPreference = $ErrorActionPreference
    try {
        $ErrorActionPreference = 'Continue'
        $pagesExists = Test-NativeSuccess 'gh' @('api', "repos/$Repository/pages")
        if ($pagesExists) {
            & gh api --method PUT "repos/$Repository/pages" --input $pagesPayloadPath *> $null
        }
        else {
            & gh api --method POST "repos/$Repository/pages" --input $pagesPayloadPath *> $null
        }
        $pagesConfigured = ($LASTEXITCODE -eq 0)

        if ($pagesConfigured) {
            # A newly-created Pages site may already be building. Rebuild is best-effort.
            & gh api --method POST "repos/$Repository/pages/builds" *> $null
        }
    }
    finally {
        $ErrorActionPreference = $oldErrorActionPreference
    }

    if ($pagesConfigured) {
        Write-Host "[GitHub Pages] Reset bridge: https://tamnhien.github.io/ITDeviceManager/reset-password.html" -ForegroundColor Green
    }
    else {
        Write-Warning 'GitHub Pages could not be configured automatically. The release will continue; enable Pages from main:/docs manually if needed.'
    }
}
finally {
    if (Test-Path $pagesPayloadPath) {
        Remove-Item -Force $pagesPayloadPath
    }
}

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
