# Creates a local JWT key for this project. An existing key is kept.
$project = Join-Path $PSScriptRoot '..\UI\Backend\TriadIdentity.UI.Api'

$existing = dotnet user-secrets list --project $project
if ($existing -match '^Jwt:Key\s*=') {
    Write-Host 'Jwt:Key already exists, nothing to do.'
    exit 0
}

$bytes = New-Object byte[] 64
$rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
$rng.GetBytes($bytes)
$rng.Dispose()

dotnet user-secrets set 'Jwt:Key' ([Convert]::ToBase64String($bytes)) --project $project | Out-Null
Write-Host 'Jwt:Key created in user-secrets.'