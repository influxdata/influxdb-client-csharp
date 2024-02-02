#!/usr/bin/env bash

set -e

#
# Read arguments
#
while [[ $# -gt 0 ]]; do
  case $1 in
    -tfm|--dotnet-target-framework)
      DOTNET_TARGET_FRAMEWORK="$2"
      shift # Past argument
      shift # Past value
      ;;
    -codecov|--code-coverage-report)
      CODE_COVERAGE_REPORT=true
      shift # Past argument
      ;;
    *)
      echo "Unknown argument $1"
      exit 1
      ;;
  esac
done


#
# Validate arguments
#
if [[ -z "$DOTNET_TARGET_FRAMEWORK" ]]
then
  echo "Please set the .NET target framework (TFM) to use for testing with the \"--dotnet-target-framework <tfm>\" argument."
  exit 1
fi

CODE_COVERAGE_REPORT="${CODE_COVERAGE_REPORT:-false}"
TEST_PARAMS=()

if [[ "$CODE_COVERAGE_REPORT" = true ]]
then
  TEST_PARAMS=(--collect:"XPlat Code Coverage")
  echo "Running tests using $DOTNET_TARGET_FRAMEWORK with Code Coverage report..."
else
  echo "Running tests using $DOTNET_TARGET_FRAMEWORK without Code Coverage report..."
fi


#
# Generate testing certificates
#
dotnet dev-certs https

#
# Test
#
dotnet test   "Client.Core.Test/bin/Release/$DOTNET_TARGET_FRAMEWORK/InfluxDB.Client.Core.Test.dll"   --verbosity normal --logger trx "${TEST_PARAMS[@]}"
dotnet test        "Client.Test/bin/Release/$DOTNET_TARGET_FRAMEWORK/InfluxDB.Client.Test.dll"        --verbosity normal --logger trx "${TEST_PARAMS[@]}"
dotnet test "Client.Legacy.Test/bin/Release/$DOTNET_TARGET_FRAMEWORK/InfluxDB.Client.Legacy.Test.dll" --verbosity normal --logger trx "${TEST_PARAMS[@]}"
dotnet test   "Client.Linq.Test/bin/Release/$DOTNET_TARGET_FRAMEWORK/InfluxDB.Client.Linq.Test.dll"   --verbosity normal --logger trx "${TEST_PARAMS[@]}"
