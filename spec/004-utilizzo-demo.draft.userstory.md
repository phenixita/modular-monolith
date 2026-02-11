Come operatore di manutenzione, voglio usare la CLI per gestire catalogo, stock e cassa in un flusso realistico in modo da amministrare la vending machine nel quotidiano.

## Acceptance Criteria

Scenario Outline: avvio su installazione pulita e creazione catalogo via CLI

Given la vending machine e' disponibile
And il catalogo e' vuoto
When eseguo il comando <create_product_command> per il prodotto <product_code> con prezzo <price>
Then il prodotto <product_code> esiste con prezzo <price>

Examples:
	| product_code | price | create_product_command                          |
	| COLA         | 2.00  | vm inventory product create COLA 2.00           |
	| WATER        | 1.00  | vm inventory product create WATER 1.00          |

Scenario Outline: carico iniziale dello stock via CLI

Given la vending machine e' disponibile
And il prodotto <product_code> esiste in catalogo
And lo stock iniziale per <product_code> e' <initial_stock>
When eseguo il comando <load_command> per caricare <quantity> unita'
Then lo stock per <product_code> e' <expected_stock>

Examples:
	| product_code | initial_stock | quantity | expected_stock | load_command                            |
	| COLA         | 0             | 5        | 5              | vm inventory stock load COLA 5          |
	| WATER        | 0             | 10       | 10             | vm inventory stock load WATER 10        |

Scenario Outline: inserimento monete e lettura saldo via CLI

Given la vending machine e' disponibile
And il saldo iniziale e' <initial_balance>
When eseguo il comando <insert_command> per inserire <amount>
And eseguo il comando <balance_command> per leggere il saldo
Then il saldo e' <expected_balance>

Examples:
	| initial_balance | amount | expected_balance | insert_command     | balance_command |
	| 0.00            | 2.00   | 2.00             | vm cash insert 2.00 | vm cash balance |
	| 2.00            | 1.00   | 3.00             | vm cash insert 1.00 | vm cash balance |

Scenario Outline: rimborso del credito residuo via CLI

Given la vending machine e' disponibile
And il saldo iniziale e' <initial_balance>
When eseguo il comando <refund_command> per rimborsare il credito
Then il saldo e' 0.00
And il credito rimborsato e' <refunded_amount>

Examples:
	| initial_balance | refunded_amount | refund_command  |
	| 1.50            | 1.50            | vm cash refund  |
	| 3.00            | 3.00            | vm cash refund  |