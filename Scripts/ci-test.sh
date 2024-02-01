#!/usr/bin/env bash

set -e

#
# Parameters with default value
#
CODE_COVERAGE_REPORT=false
DOTNET_RUNTIME=dotnet
DOTNET_RUNTIME_VERSIONS=()
TRX2JUNIT_VERSION="2.0.4"
TEST_PARAMS=()

#
# Read command line arguments
#
while [[ $# -gt 0 ]]; do
  case $1 in
    --code-coverage-report)
      CODE_COVERAGE_REPORT=true
      shift # Past argument
      ;;
    --dotnet-runtime)
      DOTNET_RUNTIME="$2"
      shift # Past argument
      shift # Past value
      ;;
    --dotnet-runtime-versions)
      IFS=',' read -a DOTNET_RUNTIME_VERSIONS <<< "$2" # Split runtime versions separated by ";"
      shift # Past argument
      shift # Past value
      ;;
    *)
      echo "Unknown option $1"
      exit 1
      ;;
  esac
done

echo "Code Coverage Report: $CODE_COVERAGE_REPORT"
echo ".NET Runtime to install: $DOTNET_RUNTIME"
echo ".NET Runtime version(s) to install: ${DOTNET_RUNTIME_VERSIONS[@]}"

#
# Install missing .NET runtimes (required to run tests)
#
if [[ "$DOTNET_RUNTIME" != "" && "${DOTNET_RUNTIME_VERSIONS[@]}" != "" ]]
then
  wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
  chmod +x ./dotnet-install.sh

  for version in "${DOTNET_RUNTIME_VERSIONS[@]}"
  do
    ./dotnet-install.sh --channel $version --runtime $DOTNET_RUNTIME --install-dir /usr/share/dotnet
  done

  apt-get install libssl1.1 -y
fi

#
# Display information about the .NET environment
#
dotnet --info

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
dotnet build --no-restore

#
# Test
#
if [[ "$CODE_COVERAGE_REPORT" = true ]]
then
  TEST_PARAMS=(--collect:"XPlat Code Coverage")
fi

export DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

dotnet test Client.Core.Test/Client.Core.Test.csproj --no-build --verbosity normal --logger trx "${TEST_PARAMS[@]}"
dotnet test Client.Test/Client.Test.csproj --no-build --verbosity normal --logger trx "${TEST_PARAMS[@]}"
dotnet test Client.Legacy.Test/Client.Legacy.Test.csproj --no-build --verbosity normal --logger trx "${TEST_PARAMS[@]}"
dotnet test Client.Linq.Test/Client.Linq.Test.csproj --no-build --verbosity normal --logger trx "${TEST_PARAMS[@]}"
