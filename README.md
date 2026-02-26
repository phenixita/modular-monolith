# Modular Monolith - Module Assembly Philosophy

Questo repository adotta una filosofia di montaggio moduli dove l'host applicativo resta semplice:

- l'host decide il contesto (produzione o test)
- il modulo espone API DI esplicite e auto-consistenti
- il modulo incapsula i dettagli infrastrutturali
- la configurazione runtime arriva dall'ambiente (es. connection string)

## Obiettivo

Ridurre la conoscenza infrastrutturale nell'host e mantenere i moduli indipendenti dal tipo di esecuzione.

## Diagramma Mermaid (Produzione)

```mermaid
flowchart TD
    ENV[Environment Variables\npostgres.connectionString] --> API[Host: VendingMachine.Api\nProgram / Composition Root]

    API --> CASH[AddCashRegisterModule]
    API --> INV[AddInventoryModule]
    API --> ORD[AddOrdersModule]
    API --> REP[AddReportingModule]

    CASH --> READY[Moduli pronti in produzione]
    INV --> READY
    ORD --> READY
    REP --> READY
```

## Diagramma Mermaid (Test L0)

```mermaid
flowchart TD
    TESTHOST[Host Runtime Test L0\nTest Project / ServiceCollection] --> MOD[Modulo sotto test\nAddXxxModuleForTests]
    MOD --> L0READY[Test L0 pronti con wiring in-memory]
```

## Come leggere il diagramma

- l'host API legge la configurazione dall'ambiente
- l'host monta ogni modulo chiamando solo il metodo di produzione
- il risultato è un'app pronta, con moduli caricati e indipendenti dall'host

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
