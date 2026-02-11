As a maintenance user I want to load and unload beverages into the vending machine so that I can manage the inventory.

## Acceptance criteria

Scenario Outline: load beverages

Given the vending machine is available
And the current stock for product <product_code> is <initial_stock>
When I load <quantity> units of product <product_code>
Then the stock for product <product_code> is <expected_stock>

Examples:
  | product_code | initial_stock | quantity | expected_stock |
  | COLA         | 0             | 5        | 5              |
  | COLA         | 5             | 3        | 8              |
  | WATER        | 10            | 10       | 20             |

Scenario Outline: unload beverages

Given the vending machine is available
And the current stock for product <product_code> is <initial_stock>
When I unload <quantity> units of product <product_code>
Then the stock for product <product_code> is <expected_stock>

Examples:
  | product_code | initial_stock | quantity | expected_stock |
  | COLA         | 10            | 2        | 8              |
  | COLA         | 5             | 5        | 0              |
  | WATER        | 20            | 15       | 5              |
