# Introduction
This repo is to help familiarize ourselves with the coding related to "Developing Solutions for Microsoft Azure" which is the Azure AZ 204 exam.

## Newbie Alert
Know at least the basics for Azure such as what is an Azure Subscription, Resource Group etc before starting this!

# Setup
This will guide us to setup the pre-reqs for our local development environment. Resources created in Azure require a resource group. First off, we should create a resource group. 

```
$location = "<Define location such as centralus>"
$rg = "<Define resource group name here such as az204>"
az group create -n $rg -l  $location
```

## Newbie Alert
You might be wondering what are the commands. This is also known as the Azure CLI. To install azure cli, download from: https://aka.ms/azurecli.

# Topics
[App Service](AppService\ReadMe.md)
[Service Bus](ServiceBus\ReadMe.md)
