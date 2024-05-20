# Starling Bank
Technical Test for Starling Bank

## Validate/Refresh the Access Token:
##### - In Starling Bank Developers navigate to Sandbox Customers pick required Account and refresh/generate a new access token.
#### - Replace the _"Authorization"_ variable in _"application.Development.json"_ file in the project with copied token. _(Remember to add "Bearer" prefix)_

## How to run the tests:

#### - Using an IDE, go to project called **StarlingRoundUpTests** and run/debug.
#### - Tests are present in ApiHelperTests and AccountServiceTests classes.

## How to run the application:
#### - Using an IDE, go to a class called **StarlingRoundUpChallenge** and run/debug.


## API:
* **PUT**: https://localhost:7223/Account/accounts/feed/round-up
  * #### Endpoint takes: 
    * account currency
    * round up start date
    * round up end date
    * new savings goal name
    * target balance
    * base64 encoded photo

## Assumptions made during the test
#### - User knows:
* #### the period of time they want to run round up between
* #### the currency type of the account
#### - User can only have one account of each currency type
#### - Round ups are only wanted on the default category of transactions for the account
