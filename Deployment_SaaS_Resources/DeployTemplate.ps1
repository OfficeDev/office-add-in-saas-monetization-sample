# Copyright (c) Microsoft Corporation.
# Licensed under the MIT License.

param(
  [Parameter(Mandatory)]
  [String]$resourceGroupName = (Read-Host -Prompt "Enter the Resource Group name")
)

#central us contains all required resources
$location = "centralus"

try {
  Write-Host "Creating resource group..." -ForegroundColor Green
  $resourceGroup = New-AzResourceGroup -Name $resourceGroupName -Location $location
  
  Write-Host "Deploying resource..." -ForegroundColor Green
  $deployedResult =New-AzResourceGroupDeployment -ResourceGroupName $resourceGroupName `
    -TemplateFile ".\ARMTemplate.json" `
    -TemplateParameterFile ".\ARMParameters.json"
  Write-Host "Completed!"  -ForegroundColor Green
}
catch {
  Write-Host $_.Exception.Message
  Write-Host $_.Exception.ItemName
}
