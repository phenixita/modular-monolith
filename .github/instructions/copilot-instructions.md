# Copilot Instructions

## Architecture

- Source code lives under `src/` with a modular monolith

## Definition of module

- Dedicate project
- Dedicated path
- Dedicated database schema
- No reading / editing data from other modules' tables
- Communication via services, events, or shared libraries

## Developer flow

- NON NEGOTIABLE: Test driven development.
- Use red, green, refactor cycle.
- Usa la skill `code-cleanup` per mantenere il codice pulito e formattato ogni volta che finisci di lavorare e restituisci il controllo all'utente.

## Plan mode improvement

After planning remind user if updating Azure DevOps work-items or wiki is needed.

## Azure DevOps reference 

For all interactions with the MCP Server Azure DevOps or skills related to Azure DevOps, the reference project is https://dev.azure.com/micheleferracin/MFSE.
To optimize interactions with Azure DevOps, use specific agent `AzureDevOps-Docs-Helper` that has access to all Azure DevOps related tools.