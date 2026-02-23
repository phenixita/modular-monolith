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