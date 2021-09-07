
resource "azurerm_app_service" "API" {
  name                = "web-${var.fireworkSetting.name}-api-${random_id.uniqID.id}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  app_service_plan_id = azurerm_app_service_plan.asp.id
  site_config {
    dotnet_framework_version = "v5.0"
    always_on                = true
    http2_enabled            = true
    websockets_enabled       = true
    cors {
      allowed_origins = [
        lower("https://web-${var.fireworkSetting.name}-view-${random_id.uniqID.id}.azurewebsites.net"),
        lower("https://fn-${var.fireworkSetting.name}-${random_id.uniqID.id}.azurewebsites.net")

      ]
      support_credentials = false
    }
  }

  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY" = azurerm_application_insights.AI.instrumentation_key
    "ASPNETCORE_ENVIRONMENT"         = "Development"
  }

  connection_string {
    name  = "SQLServer"
    type  = "Custom"
    value = local.sqlconn
  }

  connection_string {
    name  = "ServicesBus"
    type  = "Custom"
    value = local.SBConn
  }

  connection_string {
    name  = "SignalR"
    type  = "Custom"
    value = local.SRConn
  }

  connection_string {
    name  = "RedisCache"
    type  = "Custom"
    value = local.redisConn
  }
}


resource "azurerm_app_service" "FireworkView" {
  name                = "web-${var.fireworkSetting.name}-view-${random_id.uniqID.id}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  app_service_plan_id = azurerm_app_service_plan.asp.id

  site_config {
    dotnet_framework_version = "v5.0"
    always_on                = true
    http2_enabled            = true
    websockets_enabled       = true
    default_documents        = ["index.html"]

  }

  app_settings = {
    "APPINSIGHTS_INSTRUMENTATIONKEY" = azurerm_application_insights.AI.instrumentation_key
  }


  connection_string {
    name  = "RedisCache"
    type  = "Custom"
    value = local.SRHub
  }
}


resource "azurerm_function_app" "fn" {
  name                       = "fn-${var.fireworkSetting.name}-${random_id.uniqID.id}"
  location                   = azurerm_resource_group.rg.location
  resource_group_name        = azurerm_resource_group.rg.name
  app_service_plan_id        = azurerm_app_service_plan.fnplan.id
  storage_account_access_key = azurerm_storage_account.storage.primary_access_key
  storage_account_name       = azurerm_storage_account.storage.name
  version                    = "~3"
  app_settings = {
    "firework_SERVICEBUS"                   = local.SBConn
    "WEBSITE_RUN_FROM_PACKAGE"              = "1"
    "FUNCTIONS_WORKER_RUNTIME"              = "dotnet-isolated"
    "SQLServer"                             = local.sqlconn
    "APPINSIGHTS_INSTRUMENTATIONKEY"        = azurerm_application_insights.AI.instrumentation_key
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = "InstrumentationKey=${azurerm_application_insights.AI.instrumentation_key};IngestionEndpoint=https://southeastasia-1.in.applicationinsights.azure.com/"
    "<Replace with api url>"                             = lower("https://web-${var.fireworkSetting.name}-api-${random_id.uniqID.id}.azurewebsites.net/Firework/signalR")
  }

}
