# Copilot Instructions

This is modular monolith project.
What you find here are shared practices shared between all modules.
For specific references see:
- [Cash module](./vendingmachine-cash.instructions.md)
- [Inventory module](./vendingmachine-inventory.instructions.md)


## Architecture

- Source code lives under `src/`.

## Definition of module

- Dedicate project
- Dedicated path
- Dedicated database schema
- No reading / editing data from other modules' tables
- Communication via services, events, or shared libraries

## Testing giudelines

## Testing approach

Test are organized in levels.

- L0: in-memory unit tests, no external dependencies, fast execution, ideal for TDD. Execution under 100ms. 
- L1: integration tests with minimal infrastructure (e.g.: real database engine). Use this to test sql commands, schema, indexes, etc. Execution under 1s.

RULE: Leverage the lowest test level that can verify a given behavior.

If something is testable with in-memory implementations, do not use infrastructure tests.

Example: business logic, parameter validation, etc. should be testable with in-memory implementations.

Move to L1 test to verify database interactions, tipycally database query, file system access, network calls, etc.

## Developer flow

1. NON NEGOTIABLE: Test driven development.
2. Create the simplest test that can validate a behavior.
   1. Create proper test assemblies if missing (Module.Test.LO, L1)
3. Make the test fail
4. Implement test
5. Repeat from 2 until all behaviors are implemented.

At the end leverage skill `code-cleanup`.

## Plan mode improvement

After planning remind user if updating Azure DevOps work-items or wiki is needed.

## Azure DevOps reference 

For all interactions with the MCP Server Azure DevOps or skills related to Azure DevOps, the reference project is https://dev.azure.com/micheleferracin/MFSE.
To optimize interactions with Azure DevOps, use specific agent `AzureDevOps-Docs-Helper` that has access to all Azure DevOps related tools.