output "app_service_url" {
  description = "Default URL of the deployed App Service"
  value = "https://${azurerm_linux_web_app.app.default_hostname}"
}