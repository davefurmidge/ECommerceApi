# PowerShell variables used in the script

$random=Get-Random -Minimum 100 -Maximum 1000
$location="North Europe"
$rgName="rg-iacstate-eunorth-$random"
$saName="stiacstate$random"
$kvName="kv-iacstate-eunorth-$random"

az group create -n $rgName -l $location

# Configure the Azure Blob Storage that will contain the state
az storage account create -g $rgName -n $saName -l $location --sku Standard_LRS

# Set environment variables needed to write on the storage account
$env:AZURE_STORAGE_KEY=$(az storage account keys list -n $saName -g $rgName -o tsv --query '[0].value')
$env:AZURE_STORAGE_ACCOUNT=$saName
az storage container create -n iacstate

# Configure the Key Vault that will be used to encrypt the sensitive data
$vaultId=az keyvault create -g $rgName -n $kvName --enable-rbac-authorization true --query "id"
$myUserId=az ad signed-in-user show --query "objectId" -o tsv
az role assignment create --scope $vaultId --role "Key Vault Crypto Officer" --assignee $myUserId
az keyvault key create -n encryptionState --vault-name $kvName

# Use az cli to authenticate to key vault instead of using environment variables
$env:AZURE_KEYVAULT_AUTH_VIA_CLI="true"

# Indicate pulumi to use the newly created azure blob storage as a backend
pulumi login azblob://iacstate

# Create and use a folder to store the infrastructure code
mkdir infra;cd infra;

# Create a new Pulumi project using the azure blob storage as the backend and the keyvault as the encryption provider
pulumi new azure-csharp -n AzureStorageBackend -s dev -y --secrets-provider="azurekeyvault://$kvName.vault.azure.net/keys/encryptionState"

# Deploy the infrastructure
pulumi up -y