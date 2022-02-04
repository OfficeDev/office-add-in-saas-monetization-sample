# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

param(
    [String]$armConfig = "..\Deployment_SaaS_Resources\ARMParameters.json",
    [Parameter(Mandatory)]
    [String]$ResourceGroupName
)

$armConfigJson = Get-Content -Path $armConfig |Out-String|ConvertFrom-Json
$webAppSiteName = $armConfigJson.parameters.webAppSiteName.value
$webApiSiteName = $armConfigJson.parameters.webApiSiteName.value
$resourceMockWebSiteName = $armConfigJson.parameters.resourceMockWebSiteName.value

$currentPath = Get-Location

if (Test-Path .\output){
    Remove-item .\output -Recurse -Force -Confirm:$false
}

#publish AppSourceMock
Write-Host "Building AppSourceMock Site..." -ForegroundColor "Green"
dotnet publish .\AppSourceMockWebApp\AppSourceMockWebApp.csproj -c Release -o .\output\AppSourceMockWebApp >> log.txt

#publish WebApi
Write-Host "Building WebApi..." -ForegroundColor "Green"
dotnet publish .\SaaSSampleWebApi\SaaSSampleWebApi.csproj -c Release -o .\output\SaaSSampleWebApi >> log.txt

#publish WebApp
Write-Host "Building WebApp..." -ForegroundColor "Green"
dotnet publish .\SaaSSampleWebApp\SaaSSampleWebApp.csproj -c Release -o .\output\SaaSSampleWebApp >> log.txt

Write-Host "Packing AppSourceMock..." -ForegroundColor "Green"
Compress-Archive ".\output\AppSourceMockWebApp\*" -DestinationPath ".\output\AppSourceMockWebApp.zip"
Write-Host "Packing SaaSSampleWebApi..." -ForegroundColor "Green"
Compress-Archive ".\output\SaaSSampleWebApi\*" -DestinationPath ".\output\SaaSSampleWebApi.zip"
Write-Host "Packing SaaSSampleWebApp..." -ForegroundColor "Green"
Compress-Archive ".\output\SaaSSampleWebApp\*" -DestinationPath ".\output\SaaSSampleWebApp.zip"
try {
    Write-Host "Publishing AppSourceMock..." -ForegroundColor "Green"
    Publish-AzWebApp -ResourceGroupName $ResourceGroupName -Name $resourceMockWebSiteName -ArchivePath "$currentPath\output\AppSourceMockWebApp.zip" -Confirm:$false -Force >> log.txt
    Write-Host "Publishing WebApi..." -ForegroundColor "Green"
    Publish-AzWebApp -ResourceGroupName $ResourceGroupName -Name $webApiSiteName -ArchivePath "$currentPath\output\SaaSSampleWebApi.zip" -Confirm:$false -Force >> log.txt
    Write-Host "Publishing WebApp..." -ForegroundColor "Green"
    Publish-AzWebApp -ResourceGroupName $ResourceGroupName -Name $webAppSiteName -ArchivePath "$currentPath\output\SaaSSampleWebApp.zip" -Confirm:$false -Force >> log.txt
    Write-Host "Completed!" -ForegroundColor "Green"
}
catch {
    Write-Host $_.Exception.Message
    Write-Host $_.Exception.ItemName
}

