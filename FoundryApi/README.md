# FoundryApi (Clean Architecture)

ASP.NET Core Web API that talks to Azure AI Foundry. Sibling to the **AzureFoundryDeploymentList** solution; projects are under this folder and added to the repo solution.

## Structure

| Project | Role |
|---------|------|
| **FoundryApi.Domain** | Entities, value objects; no dependencies |
| **FoundryApi.Application** | Ports (interfaces), DTOs, use cases; references Domain only |
| **FoundryApi.Infrastructure** | Azure AI Foundry + Identity implementations; references Application |
| **FoundryApi.Web** | Controllers, DI, config; references Application + Infrastructure |

## Build & run

From repo root:

```bash
dotnet build FoundryApi\FoundryApi.Web\FoundryApi.Web.csproj
dotnet run --project FoundryApi\FoundryApi.Web\FoundryApi.Web.csproj
```

Or open `AzureFoundryDeploymentList.slnx` in Visual Studio and set **FoundryApi.Web** as startup project.

## Next steps

- Add Application ports (e.g. `IDeploymentProvider`, `IChatCompletionService`) and use cases
- Implement them in Infrastructure using existing Foundry client pattern from repo root
- Add Deployments and Chat controllers in Web
