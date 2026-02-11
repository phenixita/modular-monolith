Come utente della vending machine, voglio acquistare una bevanda in modo da ottenere il prodotto desiderato usando il credito disponibile.

## Acceptance Criteria

Scenario Outline: Acquisto bevanda con credito sufficiente
Given la vending machine è disponibile
And esiste un prodotto con codice <product_code> e prezzo <price>
And il credito disponibile è pari a <balance>
And lo stock per <product_code> è pari a <stock>
When seleziono la bevanda con codice <product_code>
Then l'acquisto va a buon fine
And il credito residuo è pari a <remaining_balance>
And lo stock per <product_code> diventa <remaining_stock>
And viene erogata una sola bevanda
Examples:
  | product_code | price | balance | stock | remaining_balance | remaining_stock |
  | COLA         | 1.50  | 2.00    | 5     | 0.50              | 4               |
  | WATER        | 1.00  | 1.00    | 3     | 0.00              | 2               |
  | JUICE        | 2.00  | 3.00    | 1     | 1.00              | 0               |

Scenario Outline: Acquisto rifiutato per credito insufficiente
Given la vending machine è disponibile
And esiste un prodotto con codice <product_code> e prezzo <price>
And il credito disponibile è pari a <balance>
And lo stock per <product_code> è pari a <stock>
When seleziono la bevanda con codice <product_code>
Then l'acquisto viene rifiutato per credito insufficiente
And il credito residuo resta <balance>
And lo stock per <product_code> resta <stock>
And nessuna bevanda viene erogata
Examples:
  | product_code | price | balance | stock |
  | COLA         | 1.50  | 1.00    | 5     |
  | WATER        | 1.00  | 0.50    | 2     |
  | JUICE        | 2.00  | 1.99    | 1     |

Scenario Outline: Acquisto rifiutato per prodotto non disponibile
Given la vending machine è disponibile
And esiste un prodotto con codice <product_code> e prezzo <price>
And il credito disponibile è pari a <balance>
And lo stock per <product_code> è pari a <stock>
When seleziono la bevanda con codice <product_code>
Then l'acquisto viene rifiutato per prodotto non disponibile
And il credito residuo resta <balance>
And lo stock per <product_code> resta <stock>
And nessuna bevanda viene erogata
Examples:
  | product_code | price | balance | stock |
  | COLA         | 1.50  | 2.00    | 0     |
  | WATER        | 1.00  | 5.00    | 0     |
  | JUICE        | 2.00  | 2.00    | 0     |
 