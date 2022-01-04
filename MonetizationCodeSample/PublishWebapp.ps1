# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

param(
    [String]$armConfig = "..\Deployment\ARMParameters.json",
    [Parameter(Mandatory)]
    [String]$ResourceGroupName
)

$armConfigJson = Get-Content -Path $armConfig |Out-String|ConvertFrom-Json
$webAppSiteName = $armConfigJson.parameters.webAppSiteName.value
$webApiSiteName = $armConfigJson.parameters.webApiSiteName.value
$outlookAddInWebSiteName = $armConfigJson.parameters.outlookAddInWebSiteName.value
$wordAddInWebSiteName = $armConfigJson.parameters.wordAddInWebSiteName.value
$excelAddInWebSiteName = $armConfigJson.parameters.excelAddInWebSiteName.value
$powerpointAddInWebSiteName = $armConfigJson.parameters.powerpointAddInWebSiteName.value
$resourceMockWebSiteName = $armConfigJson.parameters.resourceMockWebSiteName.value

$currentPath = Get-Location

if (Test-Path .\output){
    Remove-item .\output -Recurse -Force -Confirm:$false
}


#publish AppSourceMock
Write-Host "Building AppSourceMock Site..." -ForegroundColor "Green"
dotnet publish .\AppSourceMockWebApp\AppSourceMockWebApp.csproj -c Release -o .\output\AppSourceMockWebApp >> log.txt

#publish OutlookAddInWeb
Write-Host "Building OutlookAddInWeb..." -ForegroundColor "Green"
dotnet publish .\OutlookAddInWeb\OutlookAddInWeb.csproj -c Release -o .\output\OutlookAddInWeb >> log.txt

#publish WordAddInWeb
Write-Host "Building WordAddInWeb..." -ForegroundColor "Green"
dotnet publish .\WordAddInWeb\WordAddInWeb.csproj -c Release -o .\output\WordAddInWeb >> log.txt

#publish ExcelAddInWeb
Write-Host "Building ExcelAddInWeb..." -ForegroundColor "Green"
dotnet publish .\ExcelAddInWeb\ExcelAddInWeb.csproj -c Release -o .\output\ExcelAddInWeb >> log.txt

#publish PowerPointAddInWeb
Write-Host "Building PowerPointAddInWeb..." -ForegroundColor "Green"
dotnet publish .\PowerPointAddInWeb\PowerPointAddInWeb.csproj -c Release -o .\output\PowerPointAddInWeb >> log.txt

#publish WebApi
Write-Host "Building WebApi..." -ForegroundColor "Green"
dotnet publish .\SaaSSampleWebApi\SaaSSampleWebApi.csproj -c Release -o .\output\SaaSSampleWebApi >> log.txt

#publish WebApp
Write-Host "Building WebApp..." -ForegroundColor "Green"
dotnet publish .\SaaSSampleWebApp\SaaSSampleWebApp.csproj -c Release -o .\output\SaaSSampleWebApp >> log.txt

Write-Host "Packing AppSourceMock..." -ForegroundColor "Green"
Compress-Archive ".\output\AppSourceMockWebApp\*" -DestinationPath ".\output\AppSourceMockWebApp.zip"
Write-Host "Packing OutlookAddInWeb..." -ForegroundColor "Green"
Compress-Archive ".\output\OutlookAddInWeb\*" -DestinationPath ".\output\OutlookAddInWeb.zip"
Write-Host "Packing WordAddInWeb..." -ForegroundColor "Green"
Compress-Archive ".\output\WordAddInWeb\*" -DestinationPath ".\output\WordAddInWeb.zip"
Write-Host "Packing ExcelAddInWeb..." -ForegroundColor "Green"
Compress-Archive ".\output\ExcelAddInWeb\*" -DestinationPath ".\output\ExcelAddInWeb.zip"
Write-Host "Packing PowerPointAddInWeb..." -ForegroundColor "Green"
Compress-Archive ".\output\PowerPointAddInWeb\*" -DestinationPath ".\output\PowerPointAddInWeb.zip"
Write-Host "Packing SaaSSampleWebApi..." -ForegroundColor "Green"
Compress-Archive ".\output\SaaSSampleWebApi\*" -DestinationPath ".\output\SaaSSampleWebApi.zip"
Write-Host "Packing SaaSSampleWebApp..." -ForegroundColor "Green"
Compress-Archive ".\output\SaaSSampleWebApp\*" -DestinationPath ".\output\SaaSSampleWebApp.zip"

try {
    Write-Host "Publishing AppSourceMock..." -ForegroundColor "Green"
    Publish-AzWebApp -ResourceGroupName $ResourceGroupName -Name $resourceMockWebSiteName -ArchivePath "$currentPath\output\AppSourceMockWebApp.zip" -Confirm:$false -Force >> log.txt
    Write-Host "Publishing OutlookAddInWeb..." -ForegroundColor "Green"
    Publish-AzWebApp -ResourceGroupName $ResourceGroupName -Name $outlookAddInWebSiteName -ArchivePath "$currentPath\output\OutlookAddInWeb.zip" -Confirm:$false -Force >> log.txt
    Write-Host "Publishing WordAddInWeb..." -ForegroundColor "Green"
    Publish-AzWebApp -ResourceGroupName $ResourceGroupName -Name $wordAddInWebSiteName -ArchivePath "$currentPath\output\WordAddInWeb.zip" -Confirm:$false -Force >> log.txt
    Write-Host "Publishing ExcelAddInWeb..." -ForegroundColor "Green"
    Publish-AzWebApp -ResourceGroupName $ResourceGroupName -Name $excelAddInWebSiteName -ArchivePath "$currentPath\output\ExcelAddInWeb.zip" -Confirm:$false -Force >> log.txt
    Write-Host "Publishing PowerPointAddInWeb..." -ForegroundColor "Green"
    Publish-AzWebApp -ResourceGroupName $ResourceGroupName -Name $powerpointAddInWebSiteName -ArchivePath "$currentPath\output\PowerPointAddInWeb.zip" -Confirm:$false -Force >> log.txt
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

