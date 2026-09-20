[CmdletBinding()]
param(
    [switch]$SkipDatabase
)

$ErrorActionPreference = 'Stop'
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

function Require-Command([string]$Name) {
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        throw "Required command '$Name' was not found in PATH."
    }
}

function Assert-ValidWindowsIcon([string]$Path) {
    if (-not (Test-Path -LiteralPath $Path)) {
        throw "Application icon is missing: $Path"
    }

    [byte[]]$bytes = [IO.File]::ReadAllBytes((Resolve-Path -LiteralPath $Path))
    if ($bytes.Length -lt 22) {
        throw "App.ico is too small to be a valid Windows ICO file."
    }

    # ICONDIR: WORD reserved=0, WORD type=1, WORD count>0.
    $reserved = [BitConverter]::ToUInt16($bytes, 0)
    $type = [BitConverter]::ToUInt16($bytes, 2)
    $count = [BitConverter]::ToUInt16($bytes, 4)
    if ($reserved -ne 0 -or $type -ne 1 -or $count -lt 1) {
        throw "App.ico is not a real Windows ICO file. Do NOT rename a PNG/JPG to .ico; use a converted ICO file."
    }

    $directoryEnd = 6 + (16 * $count)
    if ($directoryEnd -gt $bytes.Length) {
        throw "App.ico has a truncated ICO directory."
    }

    for ($i = 0; $i -lt $count; $i++) {
        $entry = 6 + (16 * $i)
        $bytesInResource = [BitConverter]::ToUInt32($bytes, $entry + 8)
        $imageOffset = [BitConverter]::ToUInt32($bytes, $entry + 12)

        if ($bytesInResource -eq 0) {
            throw "App.ico contains an empty image frame."
        }

        $end = [uint64]$imageOffset + [uint64]$bytesInResource
        if ($imageOffset -lt $directoryEnd -or $end -gt [uint64]$bytes.Length) {
            throw "App.ico contains an invalid image frame offset/length."
        }
    }

    Write-Host "[PASS] App.ico is a valid Windows ICO container ($count frame(s))."
}

Require-Command dotnet

Write-Host "[1/6] Checking .NET SDK..." -ForegroundColor Cyan
$sdks = @(dotnet --list-sdks)
if (-not ($sdks | Where-Object { $_ -match '^10\.' })) {
    throw ".NET 10 SDK is required."
}

Write-Host "[2/6] Checking secret hygiene..." -ForegroundColor Cyan
if (Test-Path '.env') {
    if ((Test-Path '.git') -and (Get-Command git -ErrorAction SilentlyContinue)) {
        & git check-ignore -q -- .env
        if ($LASTEXITCODE -ne 0) {
            throw ".env exists but is not ignored by Git. Refusing to continue."
        }

        # Do not use --error-unmatch here: an untracked .env is the expected/safe case,
        # and that option writes an error to stderr which PowerShell can promote to a failure.
        $trackedEnv = @(& git ls-files -- .env)
        if ($LASTEXITCODE -ne 0) {
            throw "git ls-files failed while checking .env."
        }
        if ($trackedEnv.Count -gt 0) {
            throw ".env is tracked by Git. Run: git rm --cached .env"
        }
        Write-Host "[PASS] .env is ignored and untracked."
    }
    else {
        $gitignore = Get-Content -Raw -LiteralPath '.gitignore'
        if ($gitignore -notmatch '(?m)^\.env\s*$') {
            throw ".env exists but .gitignore does not contain a .env rule."
        }
        Write-Host "[PASS] .env is protected by .gitignore (Git repository not initialized yet)."
    }
}
else {
    Write-Host "[INFO] No .env in project root. Environment/default settings will be used."
}

Write-Host "[3/6] Validating application icon..." -ForegroundColor Cyan
Assert-ValidWindowsIcon (Join-Path $root 'ITDeviceManager\Assets\App.ico')

Write-Host "[4/6] Restoring packages..." -ForegroundColor Cyan
dotnet restore '.\ITDeviceManager.sln'
if ($LASTEXITCODE -ne 0) { throw 'dotnet restore failed.' }

Write-Host "[5/6] Building Release with warnings treated as errors..." -ForegroundColor Cyan
dotnet build '.\ITDeviceManager.sln' -c Release --no-restore -warnaserror
if ($LASTEXITCODE -ne 0) { throw 'dotnet build failed.' }

Write-Host "[6/6] Running application self-tests..." -ForegroundColor Cyan
$selfTestArgs = @('run', '--project', '.\ITDeviceManager.SelfTest\ITDeviceManager.SelfTest.csproj', '-c', 'Release', '--no-build', '--')
if ($SkipDatabase) { $selfTestArgs += '--skip-db' }
& dotnet @selfTestArgs
if ($LASTEXITCODE -ne 0) { throw 'ITDeviceManager self-test failed.' }

Write-Host ''
Write-Host 'ALL TESTS PASSED' -ForegroundColor Green
