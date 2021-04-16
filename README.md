# ServerlessDotnetApi
Starter for REST API project:
- [x] Should use .NET
- [x] Should use DynamoDB
- [x] Should use The serverless framework
- [x] Should generate Swagger documentation automatically on new controller
- [x] Should be testable offline
- [x] Should allow JWT authentification
- [ ] Should allow federated authentification
- [x] Should allow role-based authorization
- [ ] Should have 100% unit test coverage
- [ ] Should allow logging
- [ ] Should allow query parameters (?year=2015&product_name=test)
- [ ] Schema validation

## Prerequisites:
- Node.js (required to install Serverless framework)
- AWS CLI (run ```aws configure```)
- Serverless framework ```npm install serverless -g```
- Docker (for running dynamodb locally)
- .NET 5
- C# Extension for VSCode (MSBuild required for building - might require Visual Studio)

## How to run:
### For offline testing:
- ```docker-compose up``` (required for local dynamodb, tables must be manually created)
- Run VSCode task (which depends on a build step)
- Open a new terminal to run the next command(s):
- If table already exists, delete it: ```aws dynamodb delete-table --table-name ProductReview --endpoint-url http://localhost:8000```
- ```aws dynamodb create-table --cli-input-json file://DbScripts/create-table-productreview.json --endpoint-url http://localhost:8000```
- ```aws dynamodb create-table --cli-input-json file://DbScripts/create-table-user.json --endpoint-url http://localhost:8000```
- ```aws dynamodb list-tables --endpoint-url http://localhost:8000```
- ```aws dynamodb scan --table-name User --endpoint-url http://localhost:8000```
- ```aws dynamodb update-item --table-name User --key '{"Id": {"S": '<GUID>'}}' --attribute-updates '{"Role": {"Value": {"N": '1'} --endpoint-url http://localhost:8000```
- ```aws dynamodb delete-item --table-name User --key '{"Id": {"S": '<GUID>'}}' --endpoint-url http://localhost:8000```
- Use register path to post a new user, then modify its role from ```http://localhost:8001/``` web interface, or use ```aws dynamodb put-item --table-name User --item file://DbScripts/admin-user.json --endpoint-url http://localhost:8000``` to add (admin/pass) credentials
- ```dotnet tool install --global Amazon.Lambda.Tools --version 3.0.1``` Required to package the lambda function into zip.
- run ```.\build.ps1``` every time the code changes
Two options:
- use ```Run app``` task in VSCode, then use Postman for testing. 

Base path should be ```https://localhost:6001```, so test something like ```https://localhost:6001/WeatherForecast```.

    Note: SSL certificate check must be disabled in Postman, otherwise this won't work.
### For online testing:
- ```.\build.ps1```
- ```serverless deploy -v```
- Make sure to manually add an item in DynamoDb table using AWS Console, the app returns error if table is empty!!!
### Testing in Postman
- POST to ```https://localhost:6001/Users/auth/register``` the following message as Body/raw:
```json
{
  "firstName": "fn",
  "lastName": "ln",
  "username": "user",
  "password": "pass"
}
```

- Make sure in Headers you have ```Content-Type``` set as ```application/json```

- Make sure ```No auth``` is selected in ```Authorization``` tab!

- POST to ```https://localhost:6001/Users/auth/authenticate``` the following message as Body/raw:
```json
{
  "username": "user",
  "password": "pass"
}
```
- Make sure in Headers you have ```Content-Type``` set as ```application/json```

- Make sure ```No auth``` is selected in ```Authorization``` tab!

- Note the token received, which will be used in the next requests.

- GET from ```https://localhost:6001/Users/user``` with Body set to None.
Make sure in Headers you have ```Content-Type``` set as ```application/json```

Make sure ```Bearer token``` is selected in ```Authorization```. Copy + paste the token!

## Todo:
- Use granular dynamodb permissions in serverless.yml file, instead of dynamodb.*
- Federated authentification
- Validate REST data (schema)
- Confirmation email
- Repository is tightly coupled with DynamoDBContext. 
- Lock icon for authorization requiring services
- User - should replace username with user id


## Decisions
- JWT Should contain just user id, because it is the only one not changing (otherwise the token may contain a deleted user and would still work for 7 days)
- Will not use Entity Framework for NoSql Databases (ORM-Object relational mapping, relational means SQL)
- Will not create a global secondary index just for registration and authentification, due to the unnecessary cost overhead. Is better to just scan the entire table instead.
- Tried out "aaronshaf/dynamodb-admin" Docker image and "dynamodb-admin" npm package by the same author, both had critical issues with modifying records in the interface. Decided to use plain old AWS CLI.

## Questions
- Admin and User in the same db?
- (Dynamodb) How to generate id when user is written to db?

## History (resouces used)
```dotnet new webapi```

Added DynamoDB and project structure from http://blog.romanpavlov.me/net-core-app-with-aws-dynamo-db and its github repo https://github.com/roman-pavlov/dynamo-db-demo

Learned how to use local dynamodb from https://www.codeproject.com/Articles/5273030/ASP-NET-Core-Web-API-plus-DynamoDB-Locally

Since this project has a different structure than ```dotnet new serverless.AspNetCoreWebAPI```, we have to update serverless.yml to take this into account. 
Used:
https://dev.to/schwamster/deploy-a-net-core-web-api-with-aws-lambda-and-the-serverless-framework-3762
https://nodogmablog.bryanhogan.net/2020/07/dynamodb-reading-and-writing-data-with-net-core-part-1/

https://referbruv.com/blog/posts/deploying-an-aspnet-core-api-into-an-aws-lambda-function

Added JWT auth from https://github.com/cornflourblue/aspnet-core-3-registration-login-api https://jasonwatmore.com/post/2019/10/14/aspnet-core-3-simple-api-for-authentication-registration-and-user-management

https://docs.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test

To add facebook auth from https://docs.microsoft.com/en-us/aspnet/core/security/authentication/social/facebook-logins?view=aspnetcore-5.0

RPG Game: https://github.com/patrickgod/dotnet-rpg

## File structure
- ```Properties``` folder is used by Visual Studio for launching
- ```appsettings.[Development.]json``` File used for AWS + Dynamodb configuration. Is read in Startup.cs. The "Development" environment variable is set by VSCode's task.
- ```LambdaEntryPoint.cs``` - Entry point for AWS Lambda
- ```Program.cs``` - Entry point for offline testing

## Troubleshooting
- ```AmazonServiceException: Unable to get IAM security credentials from EC2 Instance Metadata Service.``` - recheck ```.aws/credentials``` file (delete [default])
- ```Internal server error``` or other error while running. Open Lambda in AWS Console, click Test and setup a test input with:
```json
{
  "path": "/weatherforecast",
  "httpMethod": "GET"
}
```
or
```json
{
  "body": "{\"productName\":\"test1\",\"rank\":99,\"review\":\"test2\",\"reviewOn\":\"2021-02-25T21:20:38.633Z\"}",
  "path": "/productreviews/15",
  "httpMethod": "POST",
  "headers": {
    "Accept": "application/json",
    "Content-Type": "application/json"
  },
  "isBase64Encoded": false
}
```