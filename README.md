# Starling Bank
Technical Test for Starling Bank

## Validate/Refresh the Access Token:
* #### In Starling Bank Developers navigate to Sandbox Customers pick required Account and refresh/generate a new access token.
* #### Replace the _"Authorization"_ variable in the [appsettings.Development.json](appsettings.Development.json) file with copied token. _(Remember to add "Bearer" prefix)_.

## How to run the tests:

#### Using an IDE:
* #### go to **StarlingRoundUpTests** project.
* #### tests are present in [ApiHelperTests](StarlingRoundUpChallengeTests/Helpers/ApiHelperTests.cs) and [AccountServiceTests](StarlingRoundUpChallengeTests/Services/AccountServiceTests.cs) classes. 
* #### inside class files click run to run tests.
#### Using command line:
* #### install .NET SDK. You can download it from [.NET download page](https://dotnet.microsoft.com/en-us/download/dotnet).
* #### open command line and navigate to the directory where projects are stored
* #### run the tests using dotnet test command
```console
dotnet test
```

## How to run the application:
#### Using an IDE:
* #### go to [StarlingRoundUpChallenge](StarlingRoundUpChallenge/StarlingRoundUpChallenge.csproj) project and run/debug.
* #### swagger documentation will be automatically launched to use endpoints*.
  * #### _*if swagger documentation fails to automatically launch navigate to [Swagger StarlingRoundUpChallenge](https://localhost:7223/swagger/index.html)_.
#### Using command line:
* #### install .NET SDK. You can download it from [.NET download page](https://dotnet.microsoft.com/en-us/download/dotnet).
* #### open command line and navigate to the directory where projects are stored
* #### restore project dependencies specified in the .csproj file using the dotnet restore command:
```console
dotnet restore
```
* #### build the project using the dotnet build command
```console
dotnet build
```
* #### run the project using dotnet run command
```console
dotnet run
```
* #### once project is successfully running navigate to swagger documentation to use endpoints. Found at [Swagger StarlingRoundUpChallenge](https://localhost:7223/swagger/index.html). 


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
* #### User knows:
  * #### the period of time they want to run round up between
  * #### the currency type of the account
* #### User can only have one account of each currency type
* #### Round ups are only wanted on the default category of transactions for the account
