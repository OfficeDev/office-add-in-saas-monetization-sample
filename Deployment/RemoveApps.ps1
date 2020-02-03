# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

param(
    [Parameter(Mandatory)]
    [String]$config = (Read-Host -Prompt "Enter the config file")
)


$json = Get-Content -Path $config |Out-String|ConvertFrom-Json -AsHashtable

foreach ($app in $json.apps)
{
    #$app.bodyParameter
    Get-MgApplication -top 400|Where-Object {$_.DisplayName -eq $app.bodyParameter.displayName} |ForEach-Object {
        Remove-Application -ApplicationId $_.Id
    }
    Write-Host "Remove $($app.bodyParameter.displayName)."
}