#!/usr/bin/env bash

set -e

if [[ "$*" == *true* ]]
then
    CODE_COVERAGE_REPORT=true
else
    CODE_COVERAGE_REPORT=false 
fi

echo "Test configuration: $*, generate coverage report: $CODE_COVERAGE_REPORT"

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

#
# Install testing tools
#
dotnet tool install --tool-path="./Coverlet/" coverlet.console --version 1.7.2
dotnet tool install --tool-path="./trx2junit/" trx2junit --version 1.3.2

#
# Build
#
dotnet restore
dotnet build

#
# Test
#
./Coverlet/coverlet Client.Legacy.Test/bin/Debug/"$NET_VERSION"/Client.Legacy.Test.dll --target "dotnet" --targetargs "test Client.Legacy.Test/Client.Legacy.Test.csproj --no-build  --logger trx" --format opencover --output "./Client.Legacy.Test/"
./Coverlet/coverlet Client.Test/bin/Debug/"$NET_VERSION"/Client.Test.dll --target "dotnet"  --targetargs "test Client.Test/Client.Test.csproj --no-build --logger trx" --format opencover --output "./Client.Test/"
./Coverlet/coverlet Client.Linq.Test/bin/Debug/"$NET_VERSION"/Client.Linq.Test.dll --target "dotnet" --targetargs "test Client.Linq.Test/Client.Linq.Test.csproj --no-build  --logger trx" --format opencover --output "./Client.Linq.Test/"

#
# Convert test results to Junit format
#
./trx2junit/trx2junit ./**/TestResults/*.trx
