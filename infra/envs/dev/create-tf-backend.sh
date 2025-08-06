#!/bin/bash
# One-time setup script to create the Azure backend for Terraform state

RESOURCE_GROUP="rg-tf-backend"
STORAGE_ACCOUNT="tfbackendstorage"
CONTAINER_NAME="tfstate"
LOCATION="eastus"  # Change if needed

echo "Creating resource group..."
az group create --name $RESOURCE_GROUP --location $LOCATION

echo "Creating storage account..."
az storage account create   --name $STORAGE_ACCOUNT   --resource-group $RESOURCE_GROUP   --sku Standard_LRS   --encryption-services blob

echo "Getting storage account key..."
ACCOUNT_KEY=$(az storage account keys list --resource-group $RESOURCE_GROUP --account-name $STORAGE_ACCOUNT --query '[0].value' -o tsv)

echo "Creating blob container for Terraform state..."
az storage container create   --name $CONTAINER_NAME   --account-name $STORAGE_ACCOUNT   --account-key $ACCOUNT_KEY

echo "✅ Backend setup complete. Now update your Terraform directory with backend.tf and run:"
echo "   terraform init"
