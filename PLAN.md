# Plan: ASP.NET Core API + Angular Chat UI (Clean Architecture)

Two-project setup using **Clean Architecture**: backend talks to **Azure AI Foundry**, frontend is a modern **Angular** chat UI. Dependencies point inward; domain and use cases stay framework-agnostic.

---

## 1. Architecture Overview

```
┌─────────────────────┐         ┌──────────────────────────┐         ┌─────────────────┐
│   Angular App       │  HTTP   │  ASP.NET Core Web API    │  SDK    │  Azure AI       │
│   (Clean layers)    │ ──────► │  (Clean Architecture)   │ ──────► │  Foundry        │
└─────────────────────┘         └──────────────────────────┘         └─────────────────┘
```

- **API**: Clean Architecture (Domain → Application → Infrastructure → Web). Owns Azure AI Foundry; no secrets in the browser.
- **Angular**: Clean-style layers (Core → Data → Presentation). Calls API only.

---

## 2. Backend: ASP.NET Core Web API (Clean Architecture)

### Dependency rule
- **Domain**: no dependencies. Entities, value objects, domain exceptions.
- **Application**: depends on Domain only. Use cases, DTOs, interfaces (ports).
- **Infrastructure**: depends on Application. Implements ports (Azure AI Foundry, Azure Identity).
- **Web**: depends on Application (and registers Infrastructure in DI). Controllers, middleware, config.

No project references from inner layers to outer (e.g. Domain never references Application or Infrastructure).

### Layer structure (projects)

```
src/FoundryApi/
├── FoundryApi.Domain/           # Class library, no refs
├── FoundryApi.Application/     # Class library, refs Domain only
├── FoundryApi.Infrastructure/   # Class library, refs Application (and Domain transitively)
└── FoundryApi.Web/              # Web API, refs Application + Infrastructure
```

### 2.1 Domain
- **Purpose**: Core business concepts; zero external dependencies.
- **Contents**:
  - Entities or value objects if needed (e.g. `ChatMessage`, `DeploymentInfo` as domain models).
  - Or keep thin: only shared enums/errors; DTOs can live in Application.
- **References**: None.

### 2.2 Application (use cases / ports)
- **Purpose**: Orchestration and contracts; no HTTP, no Azure SDK.
- **Contents**:
  - **Interfaces (ports)**: e.g. `IDeploymentProvider`, `IChatCompletionService`.
  - **DTOs**: `DeploymentDto`, `ChatRequest`, `ChatResponse`.
  - **Use cases / application services**: e.g. `GetDeploymentsQuery`, `SendChatCommand` (or a single `ChatApplicationService` that uses the ports).
- **References**: Domain only.

### 2.3 Infrastructure
- **Purpose**: Implement Application ports using Azure AI Foundry and Azure Identity.
- **Contents**:
  - `FoundryDeploymentProvider` implementing `IDeploymentProvider` (wraps `AIProjectClient.Deployments`).
  - `FoundryChatCompletionService` implementing `IChatCompletionService` (calls Foundry chat/completions).
  - Azure credential (e.g. `DefaultAzureCredential`) and endpoint config; reuse pattern from existing `FoundryClient.cs`.
- **Packages**: `Azure.AI.Projects`, `Azure.Identity`.
- **References**: Application (and Domain transitively). No reference to Web.

### 2.4 Web (presentation)
- **Purpose**: HTTP surface and composition root.
- **Contents**:
  - Controllers: `DeploymentsController`, `ChatController` (call Application use cases/services only).
  - `Program.cs`: register Application services + Infrastructure implementations, config, CORS.
  - No business logic; no direct use of Azure SDK in controllers.
- **References**: Application, Infrastructure (for DI registration only; controllers depend on interfaces from Application).

### API surface (unchanged)

| Endpoint              | Method | Purpose                          |
|-----------------------|--------|----------------------------------|
| `/api/deployments`    | GET    | List Foundry deployments        |
| `/api/chat`           | POST   | Send message(s), return reply   |
| (optional) `/api/chat/stream` | POST | Streaming response for chat UI |

### Config
- **appsettings** (Web): Azure AI project endpoint; CORS origins (e.g. `http://localhost:4200`).
- **Auth**: Azure credentials in Infrastructure; optional API key/JWT in Web later.

---

## 3. Frontend: Angular App (Clean Architecture style)

### Dependency rule
- **Core**: models and interfaces; no Angular or HTTP (or minimal Angular types only).
- **Data**: implements core interfaces; talks to API (HTTP).
- **Presentation**: components and pages; uses Core + Data (injected services).

Data and Presentation depend on Core; Presentation may use Data. Core does not depend on Data or Presentation.

### Layer structure (folders under `src/app`)

```
src/FoundryChat/src/app/
├── core/                    # Domain + application contracts
│   ├── models/              # ChatMessage, Deployment, etc.
│   ├── interfaces/          # IDeploymentService, IChatService
│   └── ...
├── data/                    # Data access / API
│   ├── services/            # DeploymentApiService, ChatApiService (implement core interfaces)
│   ├── http/                # API base URL, interceptors if needed
│   └── ...
└── presentation/            # UI
    ├── chat/                # chat page, message list, input
    ├── layout/              # shell, header
    └── shared/              # shared UI components
```

### 3.1 Core
- **Purpose**: App concepts and contracts; framework-agnostic where possible.
- **Contents**:
  - **Models**: `ChatMessage`, `Deployment`, `ChatRequest`, `ChatResponse` (or mirror API DTOs).
  - **Interfaces**: `IDeploymentService` (e.g. `getDeployments(): Observable<Deployment[]>`), `IChatService` (e.g. `send(request): Observable<ChatResponse>`).
- **References**: No dependency on `data/` or `presentation/`; only RxJS/types if needed.

### 3.2 Data
- **Purpose**: Implement Core interfaces; single place for HTTP and API shape.
- **Contents**:
  - **Services**: `DeploymentApiService`, `ChatApiService` implementing Core interfaces, calling `HttpClient` with API base URL (from environment).
  - **Providers**: Provide `IDeploymentService` → `DeploymentApiService`, `IChatService` → `ChatApiService` (injectable in Presentation).
- **References**: Core; Angular `HttpClient`, `environment`.

### 3.3 Presentation
- **Purpose**: Chat UI and user interaction.
- **Contents**:
  - **Chat**: container/smart component (uses `IChatService`, `IDeploymentService`), message list, input, send; optional deployment selector.
  - **Layout**: app shell, header.
  - **Shared**: buttons, loading, error display.
- **References**: Core (models, interfaces); injects Data services (via Core interfaces). No direct `HttpClient` in presentation.

### Features
- Chat view: message list (user vs assistant), input, send.
- Optional: deployment selector (from `IDeploymentService.getDeployments()`).
- Environment: API base URL (e.g. `http://localhost:5000`).
- Optional: streaming later when API exposes `/api/chat/stream`.

---

## 4. Repo / Solution Layout (Clean Architecture)

**Monorepo with Clean layers**

```
d:\deepak-Azure-AI-Projects\
├── src/
│   ├── FoundryApi/
│   │   ├── FoundryApi.Domain/
│   │   │   └── FoundryApi.Domain.csproj
│   │   ├── FoundryApi.Application/
│   │   │   └── FoundryApi.Application.csproj
│   │   ├── FoundryApi.Infrastructure/
│   │   │   └── FoundryApi.Infrastructure.csproj
│   │   └── FoundryApi.Web/
│   │       ├── FoundryApi.Web.csproj
│   │       ├── Program.cs
│   │       └── appsettings.json
│   └── FoundryChat/                # Angular
│       ├── src/app/
│       │   ├── core/
│       │   ├── data/
│       │   └── presentation/
│       ├── angular.json
│       └── package.json
├── AzureFoundryDeploymentList.slnx # Include Domain, Application, Infrastructure, Web
└── PLAN.md (this file)
```

**Alternative**: Sibling folders `FoundryApi/` and `FoundryChat/` with the same inner structure.

---

## 5. Implementation Order (Clean Architecture)

1. **API – Domain**
   - Create `FoundryApi.Domain`; add minimal entities/DTOs or leave thin.

2. **API – Application**
   - Create `FoundryApi.Application`; define `IDeploymentProvider`, `IChatCompletionService`, DTOs, and use case(s) (e.g. get deployments, send chat).

3. **API – Infrastructure**
   - Create `FoundryApi.Infrastructure`; implement ports with Azure AI Foundry (reuse existing client/credential pattern); register in DI extension.

4. **API – Web**
   - Create `FoundryApi.Web`; add controllers that call Application only; wire Application + Infrastructure in `Program.cs`; CORS and config.

5. **Angular – Core**
   - Create `core/`: models and `IDeploymentService`, `IChatService`.

6. **Angular – Data**
   - Create `data/`: API services implementing core interfaces; environment for base URL.

7. **Angular – Presentation**
   - Create `presentation/`: chat page, message list, input; inject services via Core interfaces.

8. **Polish**
   - Optional: streaming (API + Angular), deployment selector, API auth.

---

## 6. Quick Reference

| Item              | API (Clean)                 | Angular (Clean)              |
|-------------------|-----------------------------|-----------------------------|
| Inner layer       | Domain                      | Core (models, interfaces)   |
| Use cases         | Application (ports + DTOs)  | —                           |
| External impl     | Infrastructure              | Data (API services)         |
| Entry point       | Web (controllers, DI)       | Presentation (components)   |
| Port (dev)        | e.g. 5000 / 5001            | 4200                        |
| AI Foundry        | Infrastructure only         | No (API only)               |

---

Next step: create the four .NET projects (Domain, Application, Infrastructure, Web) and implement one use case (e.g. list deployments) end-to-end, then scaffold Angular with `core/`, `data/`, and `presentation/` and a minimal chat that calls the API.
