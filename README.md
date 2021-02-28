# ServerlessDotnetApi
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
### For offline testing:
- ```npm install``` (required for local dynamodb)
- ```sls dynamodb install``` (required for local dynamodb)
- ```sls dynamodb start``` (then visit http://localhost:8000/shell)
- ```cd .dynamodb```
- ```java -Djava.library.path=./DynamoDBLocal_lib -jar DynamoDBLocal.jar -sharedDb```
- Open a new terminal to run the next command(s):
- If table already exists, delete it: ```aws dynamodb delete-table --table-name ProductReview --endpoint-url http://localhost:8000```
- ```aws dynamodb create-table --cli-input-json file://create-table-productreview.json --endpoint-url http://localhost:8000```
- ```aws dynamodb list-tables --endpoint-url http://localhost:8000```
- ```dotnet tool install --global Amazon.Lambda.Tools --version 3.0.1``` Required to package the lambda function into zip.
- run ```.\build.ps1``` every time the code changes
Two options:
- use ```.NET Core Launch (web-api)``` task in VSCode, then use Postman for testing. 

Base path should be ```https://localhost:5001```, so test something like ```https://localhost:5001/WeatherForecast```.

    Note: SSL certificate check must be disabled in Postman, otherwise this won't work.
- use ```.NET Core Launch (web)``` task in VSCode, which will open ```https://localhost:6001/swagger``` in browser
### For online testing:
- ```.\build.ps1```
- ```serverless deploy -v```
- Make sure to manually add an item in DynamoDb table using AWS Console, the app returns error if table is empty!!!

## Todo:
### Use granular dynamodb permissions in serverless.yml file, instead of dynamodb.*
### Authentification

## History (resouces used)
```dotnet new webapi```
Added DynamoDB and project structure from http://blog.romanpavlov.me/net-core-app-with-aws-dynamo-db and its github repo: https://github.com/roman-pavlov/dynamo-db-demo
Learned how to use local dynamodb from:
https://www.codeproject.com/Articles/5273030/ASP-NET-Core-Web-API-plus-DynamoDB-Locally
Since this project has a different structure than ```dotnet new serverless.AspNetCoreWebAPI```, we have to update serverless.yml to take this into account. Used:
https://dev.to/schwamster/deploy-a-net-core-web-api-with-aws-lambda-and-the-serverless-framework-3762
https://nodogmablog.bryanhogan.net/2020/07/dynamodb-reading-and-writing-data-with-net-core-part-1/

https://referbruv.com/blog/posts/deploying-an-aspnet-core-api-into-an-aws-lambda-function

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