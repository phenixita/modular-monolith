# Copilot Instructions

## Architecture
- Source code lives under `src/` with a modular monolith split into `VendingMachine.Cash`, `VendingMachine.Inventory`, `VendingMachine.Orders`, and `VendingMachine.Cli` (see `src/VendingMachine.slnx`).
- The CLI is the composition root: `Program.cs` wires DI + MediatR and seeds data via `UpsertProductCommand`/`SetStockCommand`.
- Cross-module workflows are MediatR requests; handlers live beside their records in `CashRequests.cs`, `InventoryRequests.cs`, and `PurchaseProductCommand.cs`.
- Inventory persistence uses `IInventoryRepository` with `MongoInventoryRepository` (normalizes product codes to `ToUpperInvariant()`); orders persist via `IOrderRepository` + `PostgresOrderRepository`.
- Domain-only flows use in-memory types (`CashRegister`, `InventoryCatalog`, `StockRoom`, `OrderService`) for L0 tests.

## Workflows
- Build/test: `dotnet build .\src\VendingMachine.slnx` and `dotnet test .\src\VendingMachine.slnx`.
- Integration tests (L1) are opt-in: set `RUN_L1_TESTS=true` and run Mongo/Postgres via `.\src\docker-compose.yml`.
- Run the CLI: `dotnet run --project .\src\VendingMachine.Cli\VendingMachine.Cli.csproj`.

## Conventions
- Keep product codes case-insensitive; reuse normalization behavior in `MongoInventoryRepository` and dictionary usage (`StringComparer.OrdinalIgnoreCase`).
- When recording orders, use `OrderResult` factory helpers and persist `OrderRecord` with UTC timestamps.
- Tests live inside each project folder (e.g., `VendingMachine.Orders.Tests.L0`, `VendingMachine.Orders.Tests.L1`) rather than separate test projects.
