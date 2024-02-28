#####
# Configure the following environment variables to suit your needs.
#####
# The resource group used by Terraform to store its remote state.
RESOURCE_GROUP_NAME=rg-pulumi-state
# The location of the resource group. For example `eastus`.
LOCATION=northeurope
# The storage account (inside the resource group) used by Terraform to store its remote state.
TF_STORAGE_ACCOUNT=st$RANDOM$RANDOM$RANDOM$RANDOM
# The container name (inside the storage account) used by Terraform to store its remote state.
CONTAINER_NAME=state
#####
# Execute the following commands to set up GitOps.
#####
# Create a new Azure Resource Group
az group create --name $RESOURCE_GROUP_NAME --location $LOCATION
# Create the Storage Account
az storage account create --resource-group $RESOURCE_GROUP_NAME --name $TF_STORAGE_ACCOUNT --sku Standard_LRS --allow-blob-public-access false --encryption-services blob
# Get the Storage Account key
ACCOUNT_KEY=$(az storage account keys list --resource-group $RESOURCE_GROUP_NAME --account-name $TF_STORAGE_ACCOUNT --query '[0].value' -o tsv)
# Create a Blob Container in the Storage Account
az storage container create --name $CONTAINER_NAME --account-name $TF_STORAGE_ACCOUNT --account-key $ACCOUNT_KEY
# Create a Virtual Network
# VNET=vnet-$TF_STORAGE_ACCOUNT
# SUBNET=snet-$TF_STORAGE_ACCOUNT
# az network vnet create --resource-group $RESOURCE_GROUP_NAME --name $VNET --subnet-name $SUBNET
# az network vnet subnet update --resource-group $RESOURCE_GROUP_NAME --name $SUBNET --vnet-name $VNET --service-endpoints "Microsoft.Storage"
# # Secure the storage account in the Virtual Network
# az storage account network-rule add  --resource-group $RESOURCE_GROUP_NAME --account-name $TF_STORAGE_ACCOUNT --vnet-name $VNET --subnet $SUBNET
# az storage account update  --resource-group $RESOURCE_GROUP_NAME --name $TF_STORAGE_ACCOUNT --default-action Deny --bypass None
# Get the subscription ID
SUBSCRIPTION_ID=$(az account show --query id --output tsv --only-show-errors)
# Create a service principal
az ad sp create-for-rbac --role="Contributor" --scopes="/subscriptions/$SUBSCRIPTION_ID" --name "furmidgeuk-spn" --sdk-auth --only-show-errors

# # Get the current GitHub remote repository
# REMOTE_REPO=$(git config --get remote.origin.url)
# # Set the two GitHub secrets
# gh secret set AZURE_CREDENTIALS -b"$SERVICE_PRINCIPAL" -R $REMOTE_REPO && gh secret set TF_STORAGE_ACCOUNT -b"$TF_STORAGE_ACCOUNT" -R $REMOTE_REPO