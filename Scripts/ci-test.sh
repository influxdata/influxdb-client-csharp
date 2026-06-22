#!/usr/bin/env bash

set -e

if [[ "$*" == *true* ]]
then
    CODE_COVERAGE_REPORT=true
else
    CODE_COVERAGE_REPORT=false 
fi

echo "Configuration: $*, Coverage Report: $CODE_COVERAGE_REPORT"

#
# Prepare compatible version
#
NET_TEST_VERSION=$(dotnet --version | awk -F. '{printf "netcoreapp"$1"."$2;}')
echo "$NET_TEST_VERSION"

DEFAULT_NET_TARGET_VERSION="netstandard2.1"
NET_TARGET_VERSION="${NET_TARGET_VERSION:-$DEFAULT_NET_TARGET_VERSION}"
TRX2JUNIT_VERSION=""
TEST_PARAMS=()

if [[ "$CODE_COVERAGE_REPORT" = true ]]
then
  TEST_PARAMS=(--collect:"XPlat Code Coverage")
  TRX2JUNIT_VERSION="1.6.0"
else
  TRX2JUNIT_VERSION="1.3.2"
fi

if [[ "$NET_TEST_VERSION" = "netcoreapp6.0" || "$NET_TEST_VERSION" = "netcoreapp7.0" || "$NET_TEST_VERSION" = "netcoreapp8.0" ]]
then
  TRX2JUNIT_VERSION="2.1.0"
fi

if [[ "$NET_TEST_VERSION" != "netcoreapp8.0" ]]
then
  dotnet sln remove Examples/ExampleBlazor/ExampleBlazor.csproj
fi

#
# Generate testing certificates
#
dotnet dev-certs https

#
# Install testing tools
#
dotnet tool install --tool-path="./trx2junit/" trx2junit --version ${TRX2JUNIT_VERSION}

#
# Build
#
dotnet build Client.Core.Test/Client.Core.Test.csproj --framework="${NET_TARGET_VERSION}"
dotnet build Client.Test/Client.Test.csproj --framework="${NET_TARGET_VERSION}"
dotnet build Client.Legacy.Test/Client.Legacy.Test.csproj --framework="${NET_TARGET_VERSION}"
dotnet build Client.Linq.Test/Client.Linq.Test.csproj --framework="${NET_TARGET_VERSION}"

#
# Test
#
dotnet test Client.Core.Test/Client.Core.Test.csproj --no-build --verbosity normal --framework="${NET_TARGET_VERSION}" --logger trx "${TEST_PARAMS[@]}"
dotnet test Client.Test/Client.Test.csproj --no-build --verbosity normal --framework="${NET_TARGET_VERSION}" --logger trx "${TEST_PARAMS[@]}"
dotnet test Client.Legacy.Test/Client.Legacy.Test.csproj --no-build --verbosity normal --framework="${NET_TARGET_VERSION}" --logger trx "${TEST_PARAMS[@]}"
dotnet test Client.Linq.Test/Client.Linq.Test.csproj --no-build --verbosity normal --framework="${NET_TARGET_VERSION}" --logger trx "${TEST_PARAMS[@]}"

#
# Convert test results to Junit format
#
./trx2junit/trx2junit ./**/TestResults/*.trx
