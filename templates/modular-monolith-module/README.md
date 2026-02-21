# Template `mm-module`

Template .NET per generare lo scheletro di un nuovo modulo in un monolite modulare, allineato alla struttura di questa codebase:

- `*.Abstractions`
- modulo applicativo con MediatR (`DI`, `Service`, `Query/Handler`)
- `*.Infrastructure`
- test `L0` e `L1`

## Installazione locale

Dalla root repo:

```bash
dotnet new install ./templates/modular-monolith-module
```

## Generazione modulo

Esempio: modulo `Payments` con namespace root `VendingMachine`.

```bash
dotnet new mm-module -n Payments --RootNamespace VendingMachine
```

Output atteso in `src/`:

- `VendingMachine.Payments.Abstractions`
- `VendingMachine.Payments`
- `VendingMachine.Payments.Infrastructure`
- `VendingMachine.Payments.Tests.L0`
- `VendingMachine.Payments.Tests.L1`

## Integrazione nel monolite

1. Aggiungi i nuovi progetti alla solution (`dotnet sln add ...`).
2. Registra il modulo in composition root (`services.AddPaymentsModule()`).
3. Registra servizio applicativo e adapter infrastrutturali in DI.
4. Espandi use case, contratti e test del modulo.
