# dfc-api-lmi-import
## Introduction

This function app is used to retrieve Job Profile information from the National Careers Service API. The Job Profiles each contain a SOC value, which is subsequently used to retrieve LMI data from the LMI API.
The LMI API is provided by http://api.lmiforall.org.uk/.
This function app collates the Job Profile / LMI data and stores in a Cosmos database to subsequent processing.

## Getting Started

This is a self-contained Visual Studio 2019 solution containing a number of projects (azure function app with associated unit test project).

### Installing

Clone the project and open the solution in Visual Studio 2019.

## List of dependencies

|Item	|Purpose|
|-------|-------|
DFC.Compui.Cosmos|Cosmos repository nuget|

## Local Config Files

Once you have cloned the public repo you need to remove the -template part from the configuration file names listed below.

| Location | Filename | Rename to |
|-------|-------|-------|
| DFC.Api.Lmi.Import |local.settings-template.json | local.settings.json |

## Configuring to run locally

The project contains a "local.settings-template.json" file which contains appsettings for the function app project. To use this file, copy it to "local.settings.json" and edit and replace the configuration item values with values suitable for your environment.

This app uses the LMI API to retrieve LMI data. This app also uses the Job Profiles API to retrieve Job Profile data. To make use of it you will require an APIM API key for that service.

## App Settings

| App setting | Value |
|-------|-------|
LmiImportTimerTriggerSchedule | * * * * * * |
ApiSuffix | dev | 
|Configuration__CosmosDbConnections__LmiImport__AccessKey|__CosmosAccessKey__|
|Configuration__CosmosDbConnections__LmiImport__EndpointUrl|__CosmosEndpoint__|
|Configuration__CosmosDbConnections__LmiImport__DatabaseId|dfc-api-lmi-import|
|Configuration__CosmosDbConnections__LmiImport__CollectionId|lmi-for-all-data|
|Configuration__CosmosDbConnections__LmiImport__PartitionKey|/PartitionKey|
JobProfileApiClientOptions__BaseAddress | Job Profiles summary API endpoint |
JobProfileApiClientOptions__Version | v1 |
JobProfileApiClientOptions__ApiKey | APIM key for PP JobProfiles api |
EventGridClientOptions__TopicEndpoint | Event grid topic endpoint |
EventGridClientOptions__SubjectPrefix | Message subject prefix |
EventGridClientOptions__TopicKey | Event grid topic key |
EventGridClientOptions__ApiEndpoint | Content API endpoint |
LmiApiClientOptions__BaseAddress | https://api.lmiforall.org.uk/api/v1/ |
LmiApiClientOptions__MinYear | 2021 |
LmiApiClientOptions__MaxYear | 2027 |

## Running locally

To run this product locally, you will need to configure the list of dependencies, once configured and the configuration files updated, it should be F5 to run and debug locally.

To run the project, start the function app. Once running, await the timer to start the timer trigger.

## Deployments

This function app will be deployed as an individual stand-alone deployment.

## Built With

* Microsoft Visual Studio 2019
* .Net Core 3.1

## References

Please refer to https://github.com/SkillsFundingAgency/dfc-digital for additional instructions on configuring individual components like Cosmos.
