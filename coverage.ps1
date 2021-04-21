# Compute coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutput=TestResults/ /p:CoverletOutputFormat=lcov

# Use the following line just once to install the read tool
# dotnet tool install dotnet-reportgenerator-globaltool --tool-path .coverage

# Interpret data locally
.\.coverage\reportgenerator.exe -reports:.\Main.Tests\TestResults\coverage.info -targetdir:.\Main.Tests\TestResults\
Start-Process .\Main.Tests\TestResults\index.htm