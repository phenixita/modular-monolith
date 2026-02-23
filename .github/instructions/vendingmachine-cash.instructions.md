---
description: Use when working on cash domain implementations (ICashStorage), Postgres/InMemory cash storage, or cash persistence for balance.
applyTo: 'src/VendingMachine.Cash.Infrastructure/**'
---

# VendingMachine.Cash Standards (Hard Rules)

## Naming and Structure
- Use PascalCase for public types and methods; file name must match the primary type.
- Storage implementations must be sealed classes in the VendingMachine.Cash namespace.
- Implement ICashStorage from VendingMachine.Cash.Abstractions.

## Balance Safety
- Validate balance inputs; if balance < 0, throw ArgumentOutOfRangeException.

## Persistence Behavior
- Persistent storage implementations must call EnsureCreated inside GetBalance and SetBalance.
- EnsureCreated must create schema/table if missing and seed a default balance row.
- Use parameterized queries for database writes.

## Construction
- Validate required connection strings in constructors and store them in a private readonly field.
