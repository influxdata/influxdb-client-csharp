#!/usr/bin/env bash

set -e

#
# Nuget repository
#
DEFAULT_BONITOO_NUGET_URL="https://apitea.com/nexus/service/local/nuget/bonitoo-nuget/"
BONITOO_NUGET_URL="${BONITOO_NUGET_URL:-$DEFAULT_BONITOO_NUGET_URL}"

#
# Deploy to Preview repository
#
#dotnet pack Client.Core/Client.Core.csproj -c Debug -p:Version=5.1.0-dev.$CIRCLE_BUILD_NUM
#dotnet pack Client.Legacy/Client.Legacy.csproj -c Debug -p:Version=5.1.0-dev.$CIRCLE_BUILD_NUM
#dotnet pack Client.Linq/Client.Linq.csproj -c Debug -p:Version=5.1.0-dev.$CIRCLE_BUILD_NUM
#dotnet pack Client/Client.csproj -c Debug -p:Version=5.1.0-dev.$CIRCLE_BUILD_NUM

dotnet pack Client.Core/Client.Core.csproj -c Debug -p:VersionSuffix=dev.$CIRCLE_BUILD_NUM
dotnet pack Client.Legacy/Client.Legacy.csproj -c Debug -p:VersionSuffix=dev.$CIRCLE_BUILD_NUM
dotnet pack Client.Linq/Client.Linq.csproj -c Debug -p:VersionSuffix=dev.$CIRCLE_BUILD_NUM
dotnet pack Client/Client.csproj -c Debug -p:VersionSuffix=dev.$CIRCLE_BUILD_NUM

dotnet pack Client --version-suffix=dev.$CIRCLE_BUILD_NUM

dotnet nuget push ./Client.Core/bin/Debug/InfluxDB.Client.Core.*-dev.$CIRCLE_BUILD_NUM.nupkg -s ${BONITOO_NUGET_URL} -k ${BONITOO_SNAPSHOT_APIKEY} -sk ${BONITOO_SNAPSHOT_APIKEY}
dotnet nuget push ./Client.Legacy/bin/Debug/InfluxDB.Client.Flux.*-dev.$CIRCLE_BUILD_NUM.nupkg -s ${BONITOO_NUGET_URL} -k ${BONITOO_SNAPSHOT_APIKEY} -sk ${BONITOO_SNAPSHOT_APIKEY}
dotnet nuget push ./Client/bin/Debug/InfluxDB.Client.*-dev.$CIRCLE_BUILD_NUM.nupkg -s ${BONITOO_NUGET_URL} -k ${BONITOO_SNAPSHOT_APIKEY} -sk ${BONITOO_SNAPSHOT_APIKEY}
dotnet nuget push ./Client.Linq/bin/Debug/InfluxDB.Client.Linq.*-dev.$CIRCLE_BUILD_NUM.nupkg -s ${BONITOO_NUGET_URL} -k ${BONITOO_SNAPSHOT_APIKEY} -sk ${BONITOO_SNAPSHOT_APIKEY}