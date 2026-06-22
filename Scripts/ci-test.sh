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
TRX2JUNIT_VERSION="2.1.0"
TEST_PARAMS=()

if [[ "$CODE_COVERAGE_REPORT" = true ]]
then
  TEST_PARAMS=(--collect:"XPlat Code Coverage")
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
dotnet restore
dotnet build Client.Core.Test/Client.Core.Test.csproj --no-restore --framework="${NET_TARGET_VERSION}"
dotnet build Client.Test/Client.Test.csproj --no-restore --framework="${NET_TARGET_VERSION}"
dotnet build Client.Legacy.Test/Client.Legacy.Test.csproj --no-restore --framework="${NET_TARGET_VERSION}"
dotnet biild Client.Linq.Test/Client.Linq.Test.csproj --no-restore --framework="${NET_TARGET_VERSION}"

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
