As a thirsty user I want to insert money into the vending machine so that I can buy drinks later.

## Acceptance criteria

Scenario Outline: insert money

Given the vending machine is available
And the current balance is <initial_balance>
When I insert <amount> into the vending machine
Then the balance is <expected_balance>

Examples:
  | initial_balance | amount | expected_balance |
  | 0               | 2 €    | 2 €              |
  | 2 €             | 1 €    | 3 €              |
  | 5 €             | 5 €    | 10 €             |