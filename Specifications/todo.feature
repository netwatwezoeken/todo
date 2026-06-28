Feature: todo logic

@unit 
Scenario: A new todo is not completed
    Given a todo item
    Then the item is no complete
    
@unit 
Scenario: Completing a todo item
    Given a todo item
    When the todo is complete
    Then the item is complete