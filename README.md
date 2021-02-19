# serverless-dotnet-api
Starter for REST API project:
- dotnet
- dynamodb (also available locally)
- swagger

## Prerequisites:
- Node.js (required to install Serverless framework)
- AWS CLI (run ```aws configure```)
- Serverless framework ```npm install serverless -g```
- Java JDK with JAVA_HOME system env set to C:\Program Files\Java\jdk-X.Y.Z (for running dynamodb locally)
- .NET 5
- C# Extension for VSCode (MSBuild required for building - might require Visual Studio)

## How to run:
- ```npm install``` (required for local dynamodb)
- ```sls dynamodb install``` (required for local dynamodb)
- ```sls dynamodb start``` (then visit http://localhost:8000/shell)
- ```cd .dynamodb```
- ```java -Djava.library.path=./DynamoDBLocal_lib -jar DynamoDBLocal.jar -sharedDb```
- Open a new terminal to run the next command(s):
- If table already exists, delete it: ```aws dynamodb delete-table --table-name ProductReview --endpoint-url http://localhost:8000```
- ```aws dynamodb create-table --cli-input-json file://create-table-productreview.json --endpoint-url http://localhost:8000```
- ```aws dynamodb list-tables --endpoint-url http://localhost:8000```
Two options:
- use ```.NET Core Launch (web-api)``` task in VSCode, then use Postman for testing. 

Base path should be ```https://localhost:5001```, so test something like ```https://localhost:5001/WeatherForecast```.

    Note: SSL certificate check must be disabled in Postman, otherwise this won't work.
- use ```.NET Core Launch (web)``` task in VSCode, which will open ```https://localhost:6001/swagger``` in browser

## History (resouces used)
```dotnet new webapi```
Added DynamoDB as per http://blog.romanpavlov.me/net-core-app-with-aws-dynamo-db and its github repo.
http://blog.romanpavlov.me/net-core-app-with-aws-dynamo-db/
https://www.codeproject.com/Articles/5273030/ASP-NET-Core-Web-API-plus-DynamoDB-Locally

## File structure
- ```Properties``` folder is used by Visual Studio for launching