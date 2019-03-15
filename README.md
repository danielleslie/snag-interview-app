# Overview

This application is for the purposes of the interview at Snag.  It completes the process of intaking an application and comparing it against a stored result set of quesitons and answers in a database and then writes the output to another database.

# Architecture

  - One application is an very small REST API Microservice built with .NET Core using Swagger Codegen - this can be run locally
  - The second application is an Azure Function App (2.x Function running .NET Core) that has an event hub trigger function - this is hosted in Azure
  - There are two Cosmos DBs hosted in Azure
