# Azure Foundry AI Projects

Building a working Visual Studio project where you can authenticate, select a deployment from Foundry, and interact with the model programmatically.

## Project Structure

- **AzureFoundryDeploymentList.slnx** - Solution file
- **FoundryDemo.csproj** - C# project file
- **Program.cs** - Main entry point with Azure AI Foundry client implementation
- **FoundryClient.cs** - Helper class for Azure credential management

## Requirements

- .NET 8.0 or higher
- Azure CLI with MFA login
- Azure.AI.Projects and Azure.Identity NuGet packages

## Getting Started

1. Update the endpoint URL in `Program.cs` with your Azure AI project endpoint
2. Run the application with your Azure CLI credentials
3. The application will list all available deployments from your Foundry project