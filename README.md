# Modular Monolith - Module Assembly Philosophy

Questo repository adotta una filosofia di montaggio moduli dove l'host applicativo resta semplice:

- l'host decide il contesto (produzione o test)
- il modulo espone API DI esplicite e auto-consistenti
- il modulo incapsula i dettagli infrastrutturali
- la configurazione runtime arriva dall'ambiente (es. connection string)

## Obiettivo

Ridurre la conoscenza infrastrutturale nell'host e mantenere i moduli indipendenti dal tipo di esecuzione.

## Diagramma Mermaid

```mermaid
flowchart LR
    subgraph ENV[Execution Environment]
        VARS[Environment Variables\npostgres.connectionString]
        PURPOSE{Execution Purpose}
    end

    subgraph HOST[Host Application]
        BOOT[Program / Composition Root]
    end

    subgraph MODS[Business Modules]
        CASH[Cash Module\nAddCashRegisterModule\nAddCashRegisterModuleForTests]
        INV[Inventory Module\nAddInventoryModule\nAddInventoryModuleForTests]
        ORD[Orders Module\nAddOrdersModule\nAddOrdersModuleForTests]
        REP[Reporting Module\nAddReportingModule\nAddReportingModuleForTests]
    end

    subgraph IMPL[Infrastructure Wiring Hidden Inside Modules]
        PG[Postgres Repositories]
        MEM[InMemory Repositories]
        UOW[IUnitOfWork\nPostgresUnitOfWork / NoOpUnitOfWork]
    end

    VARS --> BOOT
    PURPOSE --> BOOT

    BOOT --> CASH
    BOOT --> INV
    BOOT --> ORD
    BOOT --> REP

    PURPOSE -->|Production| PG
    PURPOSE -->|Tests| MEM

    CASH --> PG
    CASH --> MEM
    INV --> PG
    INV --> MEM
    REP --> PG
    REP --> MEM
    ORD --> UOW

    PG --> UOW
    MEM --> UOW
```

## Come leggere il diagramma

- l'host non costruisce manualmente repository e servizi di dominio
- l'host chiama solo AddXxxModule (produzione) oppure AddXxxModuleForTests (test)
- i moduli registrano internamente dipendenze coerenti con lo scenario
- la source of truth runtime resta la configurazione ambiente

## Esempio pratico

### Produzione

- host legge la connection string dall'ambiente
- host configura la persistenza reale
- host monta i moduli con AddXxxModule

### Test

- host monta i moduli con AddXxxModuleForTests
- i moduli usano in-memory repository
- la transazionalità usa NoOpUnitOfWork dove necessario

## Benefici

- API DI dei moduli esplicite e leggibili
- host semplificato
- minore coupling tra host e dettagli infrastrutturali
- setup test consistente e ripetibile
