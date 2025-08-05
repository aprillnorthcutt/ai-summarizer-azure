
variable "subscription_id" {
  type = string
}

variable "tenant_id" {
  type = string
}

variable "tags" {
  type = map(string)
  default = {
    environment = "dev"
    owner       = "april.northcutt"
    project     = "ai-summarizer"
  }
}

variable "location" {
  type        = string
  description = "Azure region where resources will be deployed"
}
