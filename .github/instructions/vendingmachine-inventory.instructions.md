---
description: Rules and coding standards for VendingMachine.Inventory module
applyTo: 'src/VendingMachine.Inventory/**'
---

# VendingMachine.Inventory Standards (Hard Rules)

## Naming and Structure
- Use PascalCase for public types and methods; file name must match the primary type.
- Keep feature operations in their dedicated folders (e.g., CreateProduct, UpdateProduct, Stock, RemoveStock, ListProducts).
- Service interface remains in VendingMachine.Inventory.Abstractions; implementation stays in VendingMachine.Inventory.

## Logging
- All Inventory service methods must wrap work with LoggingHelper.ExecuteWithLoggingAsync.
- Use operation names in the format "InventoryService.<MethodName>".
- Log parameters via the optional "parameters" object when inputs are meaningful for diagnostics.

## DI and MediatR
- Use MediatR commands/queries for service operations; do not embed business logic directly in InventoryService.
- Register the module with AddVendingMachineInventoryModule; do not bypass it in production code.

## Testing Style
- Use xUnit with [Trait("Level", "L0")] for in-memory unit tests and [Trait("Level", "L1")] for infrastructure tests.
- L0 tests use InMemoryInventoryRepository; L1 tests use MongoInventoryRepository with InfrastructureFixture.
- Follow Arrange/Act/Assert sections; keep test names descriptive and action-oriented.