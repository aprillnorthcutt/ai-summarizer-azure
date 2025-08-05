output "resource_group_name" {
  description = "Name of the resource group"
  value       = azurerm_resource_group.rg.name
}

output "app_service_plan_name" {
  description = "Name of the App Service Plan"
  value       = azurerm_service_plan.app_plan.name
}

output "app_service_name" {
  value = azurerm_linux_web_app.web_app.name
}

output "app_service_default_hostname" {
  value = azurerm_linux_web_app.web_app.default_hostname
}

output "app_service_url" {
  value = "https://${azurerm_linux_web_app.web_app.default_hostname}"
}