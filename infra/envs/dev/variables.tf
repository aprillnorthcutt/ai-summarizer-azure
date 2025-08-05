variable "tags" {
  type = map(string)
  default = {
    environment = "dev"
    owner       = "april.northcutt"
    project     = "ai-summarizer"
  }
}

variable "location" {
  description = "Azure Region"
  type        = string
}

variable "resource_group_name" {
  description = "Name of the Resource Group"
  type        = string
}

variable "app_service_plan_name" {
  description = "App Service Plan name"
  type        = string
}

variable "app_service_name" {
  description = "App Service name"
  type        = string
}