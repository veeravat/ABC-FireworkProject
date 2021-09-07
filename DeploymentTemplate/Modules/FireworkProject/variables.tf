resource "random_id" "uniqID" {
  byte_length = 8
}

resource "random_password" "password" {
  length           = 16
  special          = true
  override_special = "_%@"
}

variable "fireworkSetting" {
  type        = map(string)
  description = "Resource Group Description"
  default = {
    name     = "Firework"
    location = "southeastasia"
  }
}

variable "AppServiceSize" {
  type        = string
  description = "AppService size[S1,S2,S3]"
  default     = "S1"
}
