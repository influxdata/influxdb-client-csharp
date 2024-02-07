#!/usr/bin/env bash

set -e

#
# NuGet repository
#
DEFAULT_BONITOO_NUGET_URL="https://apitea.com/nexus/service/local/nuget/bonitoo-nuget/"
BONITOO_NUGET_URL="${BONITOO_NUGET_URL:-$DEFAULT_BONITOO_NUGET_URL}"

#
# Deploy to Preview NuGet repository
#
dotnet nuget push ./NuGetPackages/InfluxDB.Client.*.nupkg -s ${BONITOO_NUGET_URL} -k ${BONITOO_SNAPSHOT_APIKEY} -sk ${BONITOO_SNAPSHOT_APIKEY}