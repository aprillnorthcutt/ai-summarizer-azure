variable "tags" {
  type = map(string)
  default = {
    environment = "dev"
    owner       = "april.northcutt"
    project     = "ai-summarizer"
  }
}

variable "app_service_name" {
  type = string
}

variable "resource_group_name" {
  type = string
}

variable "tags" {
  type = map(string)
}

variable "location" {
  type = string
}
