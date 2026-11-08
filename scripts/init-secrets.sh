#!/usr/bin/env bash
# Creates a local JWT key for the API and passes the same key to the Aspire AppHost.
# Existing values are kept, so the script is safe to run repeatedly.
set -euo pipefail

root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
api="$root/UI/Backend/TriadIdentity.UI.Api"
apphost="$root/UI/TriadIdentity.AppHost"

get_secret() {
    # $1 - project, $2 - secret name; prints the value or nothing
    dotnet user-secrets list --project "$1" 2>/dev/null |
        awk -v name="$2" 'index($0, name " = ") == 1 { print substr($0, length(name) + 4); exit }'
}

new_key() {
    if command -v openssl >/dev/null 2>&1; then
        openssl rand -base64 64 | tr -d '\n'
    else
        head -c 64 /dev/urandom | base64 | tr -d '\n'
    fi
}

key="$(get_secret "$api" 'Jwt:Key')"
if [ -n "$key" ]; then
    echo 'Jwt:Key already exists.'
else
    key="$(new_key)"
    dotnet user-secrets set 'Jwt:Key' "$key" --project "$api" >/dev/null
    echo 'Jwt:Key created in user-secrets.'
fi

if [ -n "$(get_secret "$apphost" 'Parameters:jwt-key')" ]; then
    echo 'Parameters:jwt-key already exists.'
else
    dotnet user-secrets set 'Parameters:jwt-key' "$key" --project "$apphost" >/dev/null
    echo 'Parameters:jwt-key created in AppHost user-secrets.'
fi
