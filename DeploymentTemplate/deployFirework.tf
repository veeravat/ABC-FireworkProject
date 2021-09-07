terraform {
  required_version = "~> 1"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~>2"
    }
    random = {
      source  = "hashicorp/random"
      version = "~>3"
    }
  }
}

provider "azurerm" {
  features {}
}


module "FireworkProject" {
  source = "./Modules/FireworkProject"
  fireworkSetting = {
    "name"     = "myfirework"
    "location" = "southeastasia"
  }
}
