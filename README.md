# serverless-dotnet-api

## Prerequisites:
- .NET 5
- C# Extension for VSCode (MSBuild required for building - might require Visual Studio)

## How to run:
Two options:
- use ```.NET Core Launch (web-api)``` task in VSCode, then use Postman for testing. 

Base path should be ```https://localhost:5001```, so test something like ```https://localhost:5001/WeatherForecast```.

    Note: SSL certificate check must be disabled in Postman, otherwise this won't work.
- use ```.NET Core Launch (web)``` task in VSCode, which will open ```https://localhost:6001/swagger``` in browser

## Source
```dotnet new webapi```

## File structure
- ```Properties``` folder is used by Visual Studio for launching