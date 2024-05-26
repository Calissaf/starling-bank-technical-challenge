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
**POST**: https://localhost:7223/Account/accounts/feed/round-up
#### Parameters


| Query Param             | Type     | Description                                                 |
|-------------------------|----------|-------------------------------------------------------------|
| accountCurrency         | enum     | Currency type for the account user wants to run roundups on |
| minTransactionTimestamp | dateTime | Start timestamp for roundUp period                          |
| maxTransactionTimestamp | dateTime | End timestamp for roundUp period                            |

#### Request Body

```json5
{
  "savingsGoalName": "my savings goal", // name of the savings goal to create for the round ups
  "target": {
    "currency": "GBP", // currency type of savings goal
    "minorUnits": 10000 // target balance for the savings goal in pence
  },
  "base64EncodedPhoto": "string" // custom image to display in starling app for savings goal
}
```

#### Response Body

````json5
{
  "savingsGoalUid": "77887788-7788-7788-7788-778877887788", // Uid of savings goal created for account round ups
  "balance": {
    "currency": "GBP", // currency type of savings goal
    "minorUnits": 500 // current balance of savings goal 
  },
  "success": true // success status of api call
}
````

## Assumptions made during the test
#### - User knows:
* #### the period of time they want to run round up between
* #### the currency type of the account
#### - User can only have one account of each currency type
#### - Round ups are only wanted on the default category of transactions for the account
