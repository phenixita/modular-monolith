# Architettura - Vending Machine Modular Monolith

## Overview - ASCII Art

```
╔════════════════════════════════════════════════════════════════════════════╗
║                    VENDING MACHINE - MODULAR MONOLITH                      ║
╚════════════════════════════════════════════════════════════════════════════╝

                          ┌─────────────────────┐
                          │   CLI Entry Point   │
                          │   (VendingMachine   │
                          │     .Cli)           │
                          └──────────┬──────────┘
                                     │
                    ┌────────────────┼────────────────┐
                    │                │                │
            ┌───────▼────────┐ ┌─────▼──────┐ ┌──────▼─────────┐
            │   MediatR      │ │   DI       │ │  CLI Commands  │
            │   Pipeline     │ │ Container  │ │ • Inventory    │
            │ (Handlers)     │ │ (Services) │ │ • Cash         │
            └────────────────┘ └─────┬──────┘ │ • Orders       │
                    │                │        └────────────────┘
                    └────────────────┼────────────────┐
                                     │                │
          ┌──────────────────────────┼────────────────┼────────────────┐
          │                          │                │                │
    ┌─────▼─────────────┐ ┌──────────▼──────────┐ ┌──▼───────────────┐
    │  INVENTORY MODULE │ │   CASH MODULE      │ │  ORDERS MODULE   │
    └─────┬─────────────┘ └──────────┬──────────┘ └──┬───────────────┘
          │                          │                │
    ┌─────┴─────────────┐ ┌──────────┴──────────┐ ┌──┴───────────────┐
    │ Abstractions      │ │ Abstractions       │ │ Abstractions     │
    │ (Interfaces)      │ │ (Interfaces)       │ │ (Interfaces)     │
    │                   │ │                    │ │                  │
    │ • IInventory      │ │ • ICashRegister    │ │ • IOrderService  │
    │   Repository      │ │   Service          │ │                  │
    │ • IInventory      │ │ • ICashStorage     │ │ • Commands &     │
    │   Service         │ │ • Queries/Commands │ │   Queries        │
    │ • Queries/        │ │                    │ │                  │
    │   Commands        │ │                    │ │                  │
    └─────┬─────────────┘ └──────────┬──────────┘ └──┬───────────────┘
          │                          │                │
    ┌─────▼─────────────┐ ┌──────────▼──────────┐ ┌──┴───────────────┐
    │ Implementation    │ │ Implementation     │ │ Implementation   │
    │                   │ │                    │ │                  │
    │ • InventoryService│ │ • CashRegisterSvc  │ │ • OrderService   │
    │ • Handlers        │ │ • Handlers         │ │ • Handlers       │
    │ • Repositories    │ │ • Storage (In-Mem, │ │                  │
    │   (Mongo, In-Mem) │ │   PostgreSQL)      │ │                  │
    └─────┬─────────────┘ └──────────┬──────────┘ └──┬───────────────┘
          │                          │                │
          │              ┌───────────┴────────────┐   │
          │              │                        │   │
    ┌─────▼──────────┐   │  ┌──────────────────┐ │   │
    │   Storage      │   │  │  Storage Engines │ │   │
    │                │   │  │                  │ │   │
    │ • MongoDB      │   │  │ • PostgreSQL     │ │   │
    │★ In-Memory     │   │  │ • In-Memory Mocks│◀┘   │
    └────────────────┘   │  └──────────────────┘     │
                         │                            │
                         └────────────────────────────┘

╔════════════════════════════════════════════════════════════════════════════╗
║                            TEST PYRAMID                                    ║
╠════════════════════════════════════════════════════════════════════════════╣
║  L0 Tests (Unit)  ▲    Tests database logic and business rules in isolation║
║  L1 Tests (Inte)  │    Tests module integration with real/mocked storage   ║
╚════════════════════════════════════════════════════════════════════════════╝

Ogni modulo segue il pattern:
  Module.Abstractions → Interfacce pubbliche (Commands, Queries, Services)
  Module             → Implementazione (Handlers, Services, Repositories)
  Module.Tests.L0    → Unit test
  Module.Tests.L1    → Integration test
```

## Diagramma Strutturale - Mermaid

```mermaid
graph TB
    subgraph CLI["🖥️ CLI Layer"]
        CliApp["CliApp<br/>(Entry Point)"]
        Commands["CLI Commands<br/>• Inventory<br/>• Cash<br/>• Orders"]
    end

    subgraph DI["⚙️ DI Container"]
        MediatR["MediatR<br/>Pipeline"]
        Services["Service Registration<br/>• InventoryService<br/>• CashRegisterService<br/>• OrderService"]
    end

    subgraph InventoryModule["📦 INVENTORY MODULE"]
        subgraph InventoryAbs["Abstractions"]
            IInventoryRepo["IInventoryRepository"]
            IInventoryService["IInventoryService"]
            InventoryQueries["Queries<br/>• GetProductByCode<br/>• GetStock"]
            InventoryCommands["Commands<br/>• CreateProduct<br/>• AddStock<br/>• DeleteProduct"]
        end

        subgraph InventoryImpl["Implementation"]
            InventoryService["InventoryService"]
            InventoryHandlers["Handlers<br/>• CreateProductHandler<br/>• AddStockHandler<br/>• GetStockHandler"]
            MongoRepo["MongoInventoryRepository"]
            InMemoryInvRepo["InMemoryInventoryRepository"]
        end
    end

    subgraph CashModule["💰 CASH MODULE"]
        subgraph CashAbs["Abstractions"]
            ICashStorage["ICashStorage"]
            ICashRegister["ICashRegisterService"]
            CashQueries["Queries<br/>• GetBalance"]
            CashCommands["Commands<br/>• InsertCash<br/>• ChargeCash<br/>• RefundAll"]
        end

        subgraph CashImpl["Implementation"]
            CashRegisterService["CashRegisterService"]
            CashHandlers["Handlers<br/>• InsertCashHandler<br/>• ChargeCashHandler<br/>• RefundAllHandler"]
            PostgresStorage["PostgresCashStorage"]
            InMemoryCashStorage["InMemoryCashStorage"]
        end
    end

    subgraph OrdersModule["📋 ORDERS MODULE"]
        subgraph OrdersAbs["Abstractions"]
            IOrderService["IOrderService"]
            OrdersQueries["Queries & Commands"]
        end

        subgraph OrdersImpl["Implementation"]
            OrderService["OrderService"]
            OrderHandlers["Handlers"]
        end
    end

    subgraph Storage["🗄️ Data Storage"]
        MongoDB[("MongoDB<br/>(Inventory)")]
        PostgreSQL[("PostgreSQL<br/>(Cash)")]
        InMemory[("In-Memory<br/>(Tests/Demo)")]
    end

    subgraph Tests["✅ Test Layers"]
        L0["L0: Unit Tests<br/>• CashRegisterTests<br/>• InventoryServiceTests"]
        L1["L1: Integration Tests<br/>• InfrastructureTests<br/>with real storage"]
    end

    %% CLI connections
    CliApp --> Commands
    Commands --> MediatR
    MediatR --> Services

    %% Services to modules
    Services --> InventoryService
    Services --> CashRegisterService
    Services --> OrderService

    %% Inventory module internal
    IInventoryRepo --> MongoRepo
    IInventoryRepo --> InMemoryInvRepo
    IInventoryService --> InventoryService
    InventoryService --> InventoryHandlers
    InventoryHandlers --> IInventoryRepo
    InventoryHandlers --> IInventoryService
    InventoryQueries --> InventoryHandlers
    InventoryCommands --> InventoryHandlers

    %% Cash module internal
    ICashStorage --> PostgresStorage
    ICashStorage --> InMemoryCashStorage
    ICashRegister --> CashRegisterService
    CashRegisterService --> CashHandlers
    CashHandlers --> ICashStorage
    CashHandlers --> ICashRegister
    CashQueries --> CashHandlers
    CashCommands --> CashHandlers

    %% Orders module internal
    IOrderService --> OrderService
    OrderService --> OrderHandlers
    OrdersQueries --> OrderHandlers
    OrdersCommands --> OrderHandlers

    %% Storage connections
    MongoRepo --> MongoDB
    PostgresStorage --> PostgreSQL
    InMemoryInvRepo --> InMemory
    InMemoryCashStorage --> InMemory

    %% Tests to modules
    L0 -.-> InventoryService
    L0 -.-> CashRegisterService
    L1 -.-> MongoDB
    L1 -.-> PostgreSQL

    style CLI fill:#e1f5ff
    style DI fill:#fff3e0
    style InventoryModule fill:#f3e5f5
    style CashModule fill:#e8f5e9
    style OrdersModule fill:#fce4ec
    style Storage fill:#ede7f6
    style Tests fill:#c8e6c9
    style InventoryAbs fill:#f8bbd0
    style InventoryImpl fill:#f48fb1
    style CashAbs fill:#a5d6a7
    style CashImpl fill:#81c784
    style OrdersAbs fill:#f8bbd0
    style OrdersImpl fill:#f48fb1
```

## Flusso di Comunicazione

```mermaid
sequenceDiagram
    actor User as Utente CLI
    participant CLI as CLI App
    participant MediatR as MediatR<br/>Pipeline
    participant Handler as Handler<br/>(es. AddStockHandler)
    participant Service as Service<br/>(InventoryService)
    participant Repo as Repository<br/>(MongoDB)
    participant Storage as MongoDB<br/>Database

    User->>CLI: dotnet vm inventory add-stock
    CLI->>MediatR: Dispatch(AddStockCommand)
    MediatR->>Handler: Handle(Command)
    Handler->>Service: AddStock(...)
    Service->>Repo: SaveAsync(...)
    Repo->>Storage: Insert Document
    Storage-->>Repo: ✓ Success
    Repo-->>Service: void
    Service-->>Handler: void
    Handler-->>MediatR: Unit (Result)
    MediatR-->>CLI: Response
    CLI-->>User: ✓ Stock aggiunto

    rect rgb(200, 220, 255)
    Note over Handler,Repo: All'interno del modulo<br/>(testabile in L0 con mock)
    end

    rect rgb(220, 255, 200)
    Note over Repo,Storage: Storage layer<br/>(testabile in L1 con real DB)
    end
```

## Anatomia di un Modulo

```mermaid
graph LR
    Contracts["📋 Contracts<br/>(Module.Abstractions)"]
    Implementation["⚙️ Implementation<br/>(Module)"]
    UnitTests["✓ Unit Tests<br/>(Module.Tests.L0)"]
    IntegrationTests["✓ Integration Tests<br/>(Module.Tests.L1)"]

    Contracts -->|implements| Implementation
    Implementation -->|tested by| UnitTests
    Implementation -->|tested by| IntegrationTests
    Implementation -->|uses| Contracts

    style Contracts fill:#e3f2fd
    style Implementation fill:#f3e5f5
    style UnitTests fill:#c8e6c9
    style IntegrationTests fill:#a5d6a7
```

## Caratteristiche Principali

- 🎯 **3 Moduli Indipendenti**: Cash, Inventory, Orders
- 🔌 **MediatR**: Pattern CQRS (Commands, Queries, Handlers)
- 💾 **Storage Polimorfo**: 
  - MongoDB per Inventory
  - PostgreSQL per Cash
  - In-Memory per test/demo
- 🏗️ **Separazione Netta**: 
  - Abstractions = contratti pubblici (interfacce)
  - Implementation = dettagli privati (logica)
- ✅ **Test Stratificati**: 
  - L0: Unit test (business logic in isolamento)
  - L1: Integration test (con storage reale)
- 🔄 **Dipendenze Centrali**: CLI App → DI Container → Servizi → Moduli

## Directory Structure

```
src/
├── VendingMachine.Cli/                 # CLI Entry Point
│   ├── CliApp.cs                       # Main CLI orchestration
│   ├── CliServiceProviderFactory.cs    # DI configuration
│   └── Commands/                       # CLI command definitions
│
├── VendingMachine.Inventory/           # Inventory Module
├── VendingMachine.Inventory.Abstractions/  # Inventory contracts
├── VendingMachine.Inventory.Tests.L0/  # Inventory unit tests
├── VendingMachine.Inventory.Tests.L1/  # Inventory integration tests
│
├── VendingMachine.Cash/                # Cash Module
├── VendingMachine.Cash.Abstractions/   # Cash contracts
├── VendingMachine.Cash.Tests.L0/       # Cash unit tests
├── VendingMachine.Cash.Tests.L1/       # Cash integration tests
│
├── VendingMachine.Orders/              # Orders Module
├── VendingMachine.Orders.Abstractions/ # Orders contracts
├── VendingMachine.Orders.Tests.L0/     # Orders unit tests
└── VendingMachine.Orders.Tests.L1/     # Orders integration tests
```
