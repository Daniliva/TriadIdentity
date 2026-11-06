# Creates a local JWT key for the API and passes the same key to the Aspire AppHost.
# Existing values are kept, so the script is safe to run repeatedly.
$api = Join-Path $PSScriptRoot '..\UI\Backend\TriadIdentity.UI.Api'
$appHost = Join-Path $PSScriptRoot '..\UI\TriadIdentity.AppHost'

function Get-Secret($project, $name) {
    foreach ($line in @(dotnet user-secrets list --project $project)) {
        $parts = "$line" -split ' = ', 2
        if ($parts.Count -eq 2 -and $parts[0] -eq $name) { return $parts[1] }
    }
    return $null
}

$key = Get-Secret $api 'Jwt:Key'
if ($key) {
    Write-Host 'Jwt:Key already exists.'
} else {
    $bytes = New-Object byte[] 64
    $rng = [System.Security.Cryptography.RandomNumberGenerator]::Create()
    $rng.GetBytes($bytes)
    $rng.Dispose()
    $key = [Convert]::ToBase64String($bytes)
    dotnet user-secrets set 'Jwt:Key' $key --project $api | Out-Null
    Write-Host 'Jwt:Key created in user-secrets.'
}

if (Get-Secret $appHost 'Parameters:jwt-key') {
    Write-Host 'Parameters:jwt-key already exists.'
} else {
    dotnet user-secrets set 'Parameters:jwt-key' $key --project $appHost | Out-Null
    Write-Host 'Parameters:jwt-key created in AppHost user-secrets.'
}