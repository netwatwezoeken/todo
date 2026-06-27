Feature: create user

@integration 
Scenario: A user adds a todo
    Given user navigates to the main page
    When they fill in 'jos@netwatwezoeken.nl' as email
    And they fill in password 'T0psecret!'
    And they click Create
    Then user is looged in
