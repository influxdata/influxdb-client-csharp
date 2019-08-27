#!/usr/bin/env bash

set -e

#
# Install testing tools
#
dotnet tool install --tool-path="./Coverlet/" coverlet.console
dotnet tool install --tool-path="./trx2junit/" trx2junit

#
# Build
#
dotnet restore
dotnet build

#
# Test
#
./Coverlet/coverlet Client.Legacy.Test/bin/Debug/netcoreapp2.2/Client.Legacy.Test.dll --target "dotnet" --targetargs "test Client.Legacy.Test/Client.Legacy.Test.csproj --no-build  --logger trx" --format opencover --output "./Client.Legacy.Test/"
./Coverlet/coverlet Client.Test/bin/Debug/netcoreapp2.2/Client.Test.dll --target "dotnet"  --targetargs "test Client.Test/Client.Test.csproj --no-build --logger trx" --format opencover --output "./Client.Test/"

#
# Convert test results to Junit format
#
./trx2junit/trx2junit ./**/TestResults/*.trx
