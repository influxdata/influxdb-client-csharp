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

sed -i '/<TargetFrameworks>netcoreapp2.2;netcoreapp3.1<\/TargetFrameworks>/c\<TargetFramework>'"${NET_TEST_VERSION}"'<\/TargetFramework>' Client.Core.Test/Client.Core.Test.csproj
sed -i '/<TargetFrameworks>netcoreapp2.2;netcoreapp3.1<\/TargetFrameworks>/c\<TargetFramework>'"${NET_TEST_VERSION}"'<\/TargetFramework>' Client.Test/Client.Test.csproj
sed -i '/<TargetFrameworks>netcoreapp2.2;netcoreapp3.1<\/TargetFrameworks>/c\<TargetFramework>'"${NET_TEST_VERSION}"'<\/TargetFramework>' Client.Legacy.Test/Client.Legacy.Test.csproj
sed -i '/<TargetFrameworks>netcoreapp2.2;netcoreapp3.1<\/TargetFrameworks>/c\<TargetFramework>'"${NET_TEST_VERSION}"'<\/TargetFramework>' Client.Linq.Test/Client.Linq.Test.csproj
sed -i '/<TargetFrameworks>netcoreapp2.2;netcoreapp3.1<\/TargetFrameworks>/c\<TargetFramework>'"${NET_TEST_VERSION}"'<\/TargetFramework>' Examples/Examples.csproj

sed -i '/<TargetFrameworks>netstandard2.0;netstandard2.1<\/TargetFrameworks>/c\<TargetFramework>'"${NET_TARGET_VERSION}"'<\/TargetFramework>' Client.Core/Client.Core.csproj
sed -i '/<TargetFrameworks>netstandard2.0;netstandard2.1<\/TargetFrameworks>/c\<TargetFramework>'"${NET_TARGET_VERSION}"'<\/TargetFramework>' Client/Client.csproj
sed -i '/<TargetFrameworks>netstandard2.0;netstandard2.1<\/TargetFrameworks>/c\<TargetFramework>'"${NET_TARGET_VERSION}"'<\/TargetFramework>' Client.Legacy/Client.Legacy.csproj
sed -i '/<TargetFrameworks>netstandard2.0;netstandard2.1<\/TargetFrameworks>/c\<TargetFramework>'"${NET_TARGET_VERSION}"'<\/TargetFramework>' Client.Linq/Client.Linq.csproj

TRX2JUNIT_VERSION=""
BUILD_PARAMS=""
TEST_PARAMS=""

if [[ "$CODE_COVERAGE_REPORT" = true ]]
then
  TRX2JUNIT_VERSION="1.5.0"
  BUILD_PARAMS="/p:ContinuousIntegrationBuild=true"
  TEST_PARAMS="/p:CollectCoverage=true /p:CoverletOutputFormat=opencover"
else
  TRX2JUNIT_VERSION="1.3.2"
fi

#
# Install testing tools
#
dotnet tool install --tool-path="./trx2junit/" trx2junit --version ${TRX2JUNIT_VERSION}

#
# Build
#
dotnet restore
dotnet build --no-restore ${BUILD_PARAMS}

#
# Test
#
dotnet test Client.Core.Test/Client.Core.Test.csproj --no-build --verbosity normal --logger trx ${TEST_PARAMS}
dotnet test Client.Test/Client.Test.csproj --no-build --verbosity normal --logger trx ${TEST_PARAMS}
dotnet test Client.Legacy.Test/Client.Legacy.Test.csproj --no-build --verbosity normal --logger trx ${TEST_PARAMS}
dotnet test Client.Linq.Test/Client.Linq.Test.csproj --no-build --verbosity normal --logger trx ${TEST_PARAMS}

#
# Convert test results to Junit format
#
./trx2junit/trx2junit ./**/TestResults/*.trx
