---
description: Rules and coding standards for VendingMachine.Persistence module - unified transaction management and database connections
applyTo: 'src/VendingMachine.Persistence/**'
---

# VendingMachine.Persistence Standards (Hard Rules)

## Purpose
Provides unified transaction management and database connection handling across all domains (Cash, Inventory, Orders) to ensure ACID compliance for multi-domain operations.

## Naming and Structure
- Use PascalCase for public types and methods; file name must match the primary type.
- Keep implementations sealed unless explicitly designed for inheritance.
- Abstractions remain in VendingMachine.Persistence.Abstractions namespace.
- Implementations stay in VendingMachine.Persistence namespace.

## Transaction Management
- **Default Isolation Level**: Serializable (configurable via TransactionManager constructor).
- All database operations MUST use async/await for uniformity.
- Use `ITransactionManager.ExecuteInTransactionAsync` for operations requiring transaction scope.
- Handlers executing cross-domain operations MUST wrap logic in a transaction.
- Single-domain operations MAY use transactions for consistency but are not required.

## Connection Management
- Use `IDbConnectionFactory` to create connections; NEVER create connections directly.
- Connection strings MUST be injected via `IConfiguration`.
- Configuration key for PostgreSQL: `postgres:connectionString`.
- Factory implementations MUST validate connection strings in constructor.
- Connections are opened asynchronously via `CreateConnectionAsync`.

## Unit of Work Pattern
- `IUnitOfWork` represents a transaction scope with connection and transaction references.
- Always dispose unit of work properly (use `await using`).
- Commit explicitly via `CommitAsync` on success.
- Rollback automatically on exception; explicit `RollbackAsync` is optional.

## DI Registration
- Register persistence services using `AddPersistence()` extension method.
- `IDbConnectionFactory` registered as Singleton (stateless, configuration-based).
- `ITransactionManager` registered as Scoped (per-request lifecycle).

## Error Handling
- Let exceptions propagate; transaction will auto-rollback.
- Validate all constructor parameters with ArgumentNullException.ThrowIfNull.
- Throw InvalidOperationException for missing or invalid configuration.

## Testing Approach
- Unit tests (L0): Mock `IDbConnectionFactory` and `ITransactionManager`.
- Integration tests (L1): Use real PostgreSQL database with test containers.
- Transaction tests MUST verify commit/rollback behavior.
- Cross-domain transaction tests MUST ensure atomicity across multiple operations.

## Example Usage

```csharp
// In a handler requiring transaction
public async Task<OrderReceipt> Handle(PlaceOrderCommand request, CancellationToken cancellationToken)
{
    return await _transactionManager.ExecuteInTransactionAsync(
        async (unitOfWork, ct) =>
        {
            // Execute multiple operations within transaction
            await ChargeCustomer(unitOfWork, amount, ct);
            await RemoveStock(unitOfWork, code, ct);
            // Transaction commits automatically on success
            return new OrderReceipt(/*...*/);
        },
        cancellationToken);
}
```

## Constraints
- No migration support (greenfield approach).
- Currently PostgreSQL-only; extend for other databases if needed.
- Async-only API; no synchronous methods.
