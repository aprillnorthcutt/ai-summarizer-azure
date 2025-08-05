provider "azurerm" {
  features {}
}

resource "azurerm_resource_group" "rg" {
  name     = "rg-summarizer-dev"
  location = var.location
  tags     = var.tags
}

resource "azurerm_service_plan" "app_plan" {
  name                = "asp-summarizer-dev"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  os_type             = "Linux" # or "Windows"
  sku_name            = "B1"
  tags                = var.tags
}


resource "azurerm_linux_web_app" "web_app" {
  name                = "summarizer-api-dev"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  service_plan_id     = azurerm_service_plan.app_plan.id

  site_config {
    application_stack {
      dotnet_version = "8.0"
    }
  }

  app_settings = {
    "WEBSITE_RUN_FROM_PACKAGE" = "0"
  }

  tags = var.tags
}

