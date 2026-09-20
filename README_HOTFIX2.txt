ITDeviceManager V1.2.6 - Release Hotfix 2

Fix:
- Removes dependency on PowerShell Get-FileHash.
- SHA-256 is now calculated directly with System.Security.Cryptography.SHA256.
- Compatible with Windows PowerShell / PowerShell environments where Get-FileHash is unavailable.

Apply:
1. Copy this package over the project root.
2. Replace scripts\release.ps1.
3. Run again: .\release.bat 1.2.6

The previous Release v1.2.6 commit can remain. The script will continue and create another small commit only for this release-script hotfix if needed.
