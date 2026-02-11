# US001 - Acquisto Base Bevanda

## User Story
Come cliente della vending machine,
Voglio inserire denaro e selezionare una bevanda,
Così da ritirare il prodotto scelto e vedere il credito aggiornato correttamente.

## Acceptance Criteria (Gherkin)
```gherkin
Feature: Acquisto base bevanda
  Come cliente della vending machine
  Voglio acquistare una bevanda con credito disponibile
  Così da ricevere il prodotto e aggiornare il saldo

  Scenario: Acquisto completato con credito sufficiente
    Given la vending machine contiene la bevanda "COLA" al prezzo di 1.50 EUR
    And il cliente inserisce 2.00 EUR
    When il cliente seleziona la bevanda "COLA"
    Then la vending machine eroga la bevanda "COLA"
    And il credito residuo del cliente è 0.50 EUR
    And il totale scalato dal credito è 1.50 EUR
```
