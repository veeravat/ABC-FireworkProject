output "fireworkSetting" {
  value = var.fireworkSetting.name
}
resource "azurerm_mssql_server" "sqlSV" {
  name                          = lower("sql-${var.fireworkSetting.name}-${random_id.uniqID.id}")
  resource_group_name           = azurerm_resource_group.rg.name
  location                      = azurerm_resource_group.rg.location
  version                       = "12.0"
  administrator_login           = "<Replace with connection string>"
  administrator_login_password  = random_password.password.result
  minimum_tls_version           = "1.2"
  public_network_access_enabled = true

}

resource "azurerm_mssql_firewall_rule" "fwRule" {
  name             = "allowAzure"
  server_id        = azurerm_mssql_server.sqlSV.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}

resource "azurerm_mssql_database" "sqlDB" {
  name        = "db-${var.fireworkSetting.name}"
  server_id   = azurerm_mssql_server.sqlSV.id
  collation   = "SQL_Latin1_General_CP1_CI_AS"
  sku_name    = "Basic"
}


resource "azurerm_redis_cache" "redis" {
  name                = "redis-${var.fireworkSetting.name}-${random_id.uniqID.id}"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  family              = "C"
  capacity            = 1
  sku_name            = "Standard"
  enable_non_ssl_port = false
  minimum_tls_version = "1.2"
}

resource "azurerm_servicebus_namespace" "sb" {
  name                = "sb-${var.fireworkSetting.name}-${random_id.uniqID.id}"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  sku                 = "Standard"
}

resource "azurerm_servicebus_namespace_authorization_rule" "sbKey" {
  namespace_name      = azurerm_servicebus_namespace.sb.name
  name                = "fireworkPolicy"
  resource_group_name = azurerm_resource_group.rg.name
  listen              = true
  send                = true
  manage              = false
}

resource "azurerm_servicebus_queue" "queue" {
  name                = "firework"
  resource_group_name = azurerm_resource_group.rg.name
  namespace_name      = azurerm_servicebus_namespace.sb.name
  max_delivery_count  = 10
}

resource "azurerm_signalr_service" "SignalR" {
  name                = "sr-${var.fireworkSetting.name}-${random_id.uniqID.id}"
  resource_group_name = azurerm_resource_group.rg.name
  location            = azurerm_resource_group.rg.location
  sku {
    capacity = 1
    name     = "Standard_S1"
  }
}
locals {
  sqlconn   = "Server=tcp:${azurerm_mssql_server.sqlSV.fully_qualified_domain_name},1433;Initial Catalog=${azurerm_mssql_database.sqlDB.name};Persist Security Info=False;User ID=<Replace with connection string>;Password=${random_password.password.result};MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
  redisConn = azurerm_redis_cache.redis.primary_connection_string
  SBConn    = azurerm_servicebus_namespace_authorization_rule.sbKey.primary_connection_string
  SRConn    = azurerm_signalr_service.SignalR.primary_connection_string
  SRHub     = "https://${azurerm_app_service.API.default_site_hostname}/fireworkhub"
}
