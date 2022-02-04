# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

param(
    [String]$appsConfig = ".\AadApps.config.json"
)

$json = Get-Content -Path $appsConfig |Out-String|ConvertFrom-Json -AsHashtable

foreach ($app in $json.apps)
{
    $filter = "DisplayName eq '" + $app.bodyParameter.displayName + "'"
    Get-MgApplication -Filter $filter |ForEach-Object {
        Remove-MgApplication -ApplicationId $_.Id
        Write-Host "Remove $($app.bodyParameter.displayName)."
    }
}