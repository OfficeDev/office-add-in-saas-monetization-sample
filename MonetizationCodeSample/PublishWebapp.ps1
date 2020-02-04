# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

param(
    [Parameter(Mandatory)]
    [String]$ResourceGroupName,
	[Parameter(Mandatory)]
    [String]$webAppSiteName = (Read-Host -Prompt "Enter webAppSiteName value in ARMParameters.json"),
	[Parameter(Mandatory)]
    [String]$webApiSiteName = (Read-Host -Prompt "Enter webApiSiteName value in ARMParameters.json"),
	[Parameter(Mandatory)]
    [String]$outlookAddInWebSiteName = (Read-Host -Prompt "Enter outlookAddInWebSiteName value in ARMParameters.json"),
	[Parameter(Mandatory)]
    [String]$resourceMockWebSiteName = (Read-Host -Prompt "Enter resourceMockWebSiteName value in ARMParameters.json")
)

$currentPath = Get-Location

if (Test-Path .\output){
    Remove-item .\output -Recurse -Force -Confirm:$false
}
#publish AppSourceMock
Write-Host "Building AppSourceMock Site..." -ForegroundColor "Green"
dotnet publish .\AppSourceMockWebApp\AppSourceMockWebApp.csproj -c Release -o .\output\AppSourceMockWebApp >> log.txt

#publish OutlookWebAddIn
Write-Host "Building OutlookWebAddIn..." -ForegroundColor "Green"
dotnet publish .\OutlookWebAddInWeb\OutlookWebAddInWeb.csproj -c Release -o .\output\OutlookWebAddInWeb >> log.txt

#publish WebApi
Write-Host "Building WebApi..." -ForegroundColor "Green"
dotnet publish .\SaaSSampleWebApi\SaaSSampleWebApi.csproj -c Release -o .\output\SaaSSampleWebApi >> log.txt

#publish WebApp
Write-Host "Building WebApp..." -ForegroundColor "Green"
dotnet publish .\SaaSSampleWebApp\SaaSSampleWebApp.csproj -c Release -o .\output\SaaSSampleWebApp >> log.txt

Write-Host "Packing AppSourceMock..." -ForegroundColor "Green"
Compress-Archive ".\output\AppSourceMockWebApp\*" -DestinationPath ".\output\AppSourceMockWebApp.zip"
Write-Host "Packing OutlookWebAddIn..." -ForegroundColor "Green"
Compress-Archive ".\output\OutlookWebAddInWeb\*" -DestinationPath ".\output\OutlookWebAddInWeb.zip"
Write-Host "Packing SaaSSampleWebApi..." -ForegroundColor "Green"
Compress-Archive ".\output\SaaSSampleWebApi\*" -DestinationPath ".\output\SaaSSampleWebApi.zip"
Write-Host "Packing SaaSSampleWebApp..." -ForegroundColor "Green"
Compress-Archive ".\output\SaaSSampleWebApp\*" -DestinationPath ".\output\SaaSSampleWebApp.zip"

try {
     Write-Host "Publishing AppSourceMock..." -ForegroundColor "Green"
     Publish-AzWebApp -ResourceGroupName $ResourceGroupName -Name $resourceMockWebSiteName -ArchivePath "$currentPath\output\AppSourceMockWebApp.zip" -Confirm:$false -Force >> log.txt
     Write-Host "Publishing OutlookAddin..." -ForegroundColor "Green"
     Publish-AzWebApp -ResourceGroupName $ResourceGroupName -Name $outlookAddInWebSiteName -ArchivePath "$currentPath\output\OutlookWebAddInWeb.zip" -Confirm:$false -Force >> log.txt
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

