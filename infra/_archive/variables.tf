variable "resource_group_name" {
  description = "Name of the resource group"
  type        = string
}

variable "location" {
  description = "Azure region for resource deployment"
  type        = string
  default     = "West US 3"
}

variable "app_service_plan_name" {
  description = "Name of the App Service plan"
  type        = string
}

variable "app_service_name" {
  description = "Name of the App Service"
  type        = string
}

variable "tags" {
  description = "Tags to apply to resources"
  type        = map(string)
}