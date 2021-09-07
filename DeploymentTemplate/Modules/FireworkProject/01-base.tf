resource "azurerm_resource_group" "rg" {
  name     = "RG-${var.fireworkSetting.name}"
  location = var.fireworkSetting.location
}

resource "azurerm_app_service_plan" "asp" {
  name                = "ASP-${var.fireworkSetting.name}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  sku {
    tier = "Standard"
    size = var.AppServiceSize
  }
}

resource "azurerm_app_service_plan" "fnplan" {
  name                = "FN-${var.fireworkSetting.name}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  kind                = "FunctionApp"
  sku {
    tier = "Dynamic"
    size = "Y1"
  }
}

resource "azurerm_application_insights" "AI" {
  name                = "AI-${var.fireworkSetting.name}"
  location            = azurerm_resource_group.rg.location
  resource_group_name = azurerm_resource_group.rg.name
  application_type    = "web"
}
locals {
  storagename = join("", [var.fireworkSetting.name, random_id.uniqID.id])
}
resource "azurerm_storage_account" "storage" {
  name                     = lower(local.storagename)
  location                 = azurerm_resource_group.rg.location
  resource_group_name      = azurerm_resource_group.rg.name
  account_replication_type = "LRS"
  account_tier             = "Standard"
}
