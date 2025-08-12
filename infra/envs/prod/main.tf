# ----------------------------
# PROD configuration (starter)
# ----------------------------

# Core
prefix      = "summarizer"         # used for naming if your module supports it
environment = "prod"
location    = "centralus"          # change to match your preferred region

# Resource Group
resource_group_name = "rg-summarizer-prod"

# App Service Plan (Linux)
app_service_plan_name     = "asp-summarizer-prod"
app_service_plan_tier     = "S1"    # bump to "P1v3" for more CPU/mem if needed
app_service_plan_size     = "S1"
app_service_plan_capacity = 1
app_service_plan_os       = "Linux"

# Web App (.NET 8)
web_app_name               = "app-summarizer-prod"   # must be globally unique on *.azurewebsites.net
web_app_stack              = "DOTNETCORE|8.0"
web_app_always_on          = true
web_app_health_check_path  = "/healthz"

# Application Insights
app_insights_name              = "appi-summarizer-prod"
app_insights_retention_in_days = 30

# Azure AI (Cognitive Services) – multi-service account
cognitive_account_name = "cog-summarizer-prod"
cognitive_account_kind = "CognitiveServices"   # multi-service (includes Text Analytics)
cognitive_account_sku  = "S0"
cognitive_location     = "centralus"           # keep same region as the app when possible

# Tags
tags = {
  env         = "prod"
  system      = "ai-summarizer"
  owner       = "april.northcutt"
  cost_center = "portfolio"
}

# App settings (no secrets here—inject via Key Vault or pipeline variables)
settings = {
  ASPNETCORE_ENVIRONMENT = "Production"
  # APPLICATIONINSIGHTS_CONNECTION_STRING  -> inject via DevOps variables or Key Vault
  # COGNITIVE_ENDPOINT / COGNITIVE_KEY     -> inject via DevOps variables or Key Vault
}
