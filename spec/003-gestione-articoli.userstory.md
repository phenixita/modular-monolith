As a maintenance user I want to manage product prices so that I can keep the product catalog up to date.

## Acceptance criteria

Scenario Outline: create product with valid price

Given the vending machine is available
When I create a product with code <product_code> and price <price>
Then the product <product_code> is created with price <price>

Examples:
  | product_code | price |
  | COLA         | 1.50  |
  | WATER        | 1.00  |
  | JUICE        | 2.00  |

Scenario Outline: create product with invalid price

Given the vending machine is available
When I create a product with code <product_code> and price <price>
Then the creation is rejected with validation error

Examples:
  | product_code | price |
  | COLA         | 0     |
  | WATER        | -1.00 |
  | JUICE        | -0.50 |

Scenario Outline: update product with valid price

Given the vending machine is available
And the product <product_code> exists with price <initial_price>
When I update the product <product_code> with price <new_price>
Then the product <product_code> has price <new_price>

Examples:
  | product_code | initial_price | new_price |
  | COLA         | 1.50          | 1.75      |
  | WATER        | 1.00          | 0.90      |
  | JUICE        | 2.00          | 2.50      |

Scenario Outline: update product with invalid price

Given the vending machine is available
And the product <product_code> exists with price <initial_price>
When I update the product <product_code> with price <new_price>
Then the update is rejected with validation error
And the product <product_code> has price <initial_price>

Examples:
  | product_code | initial_price | new_price |
  | COLA         | 1.50          | 0         |
  | WATER        | 1.00          | -1.00     |
  | JUICE        | 2.00          | -0.50     |

Scenario Outline: read product

Given the vending machine is available
And the product <product_code> exists with price <price>
When I read the product <product_code>
Then I get the product with code <product_code> and price <price>

Examples:
  | product_code | price |
  | COLA         | 1.50  |
  | WATER        | 1.00  |
  | JUICE        | 2.00  |

Scenario Outline: delete product

Given the vending machine is available
And the product <product_code> exists with price <price>
When I delete the product <product_code>
Then the product <product_code> does not exist

Examples:
  | product_code | price |
  | COLA         | 1.50  |
  | WATER        | 1.00  |
  | JUICE        | 2.00  |