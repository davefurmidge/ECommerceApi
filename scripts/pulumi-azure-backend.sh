#!/bin/bash

# Bash variables used in the script
random=$(shuf -i 100-1000 -n 1)
location="North Europe"
rgName="rg-pulumi-kv"
kvName="kv-iacstate-eunorth-$random"

az group create -n "$rgName" -l "$location"

# # Configure the Azure Blob Storage that will contain the state
# az storage account create -g "$rgName" -n "$saName" -l "$location" --sku Standard_LRS

# # Set environment variables needed to write on the storage account
# AZURE_STORAGE_KEY=$(az storage account keys list -n "$saName" -g "$rgName" -o tsv --query '[0].value')
# AZURE_STORAGE_ACCOUNT=$saName
# az storage container create -n iacstate

# Configure the Key Vault that will be used to encrypt the sensitive data
az keyvault create \
  --name "$kvName" \
  --resource-group "$rgName" \
  --location "$location" \
  --enabled-for-template-deployment true \
  --sku "standard"
if ! az keyvault key show --vault-name "$kvName" \
      --name pulumi; then
  az keyvault key create \
    --name "$kvName" \
    --vault-name kvpulumi \
    --kty "RSA" \
    --protection "software" \
    --size 2048
fi


# az ad user show --id "dave@furmidge.co.uk" --query objectId -o tsv

# vaultId=$(az keyvault create -g "$rgName" -n "$kvName" --enable-rbac-authorization true --query "id")
myUserId=$(az ad signed-in-user show --query "objectId" -o tsv)
# az role assignment create --scope "$vaultId" --role "Key Vault Crypto Officer" --assignee "$myUserId"
# az keyvault key create -n encryptionState --vault-name "$kvName"
az keyvault set-policy \
  --name "$kvName" \
  --object-id "$myUserId" \
  --key-permissions get list encrypt decrypt
az keyvault set-policy \
  --name "$kvName" \
  --object-id "$myUserId" \
  --key-permissions get list encrypt decrypt
# Use az cli to authenticate to key vault instead of using environment variables
export AZURE_KEYVAULT_AUTH_VIA_CLI="true"

# Indicate pulumi to use the newly created azure blob storage as a backend
pulumi login azblob://iacstate

# Create and use a folder to store the infrastructure code
mkdir infra && cd infra

# Create a new Pulumi project using the azure blob storage as the backend and the keyvault as the encryption provider
pulumi new azure-csharp -n AzureStorageBackend -s dev -y --secrets-provider="azurekeyvault://$kvName.vault.azure.net/keys/encryptionState"

# Deploy the infrastructure
pulumi up -y