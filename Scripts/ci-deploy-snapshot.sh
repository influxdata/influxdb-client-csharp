#!/usr/bin/env bash

set -e

#
# Lookup to Client Version
#
dotnet tool install --tool-path="./DotnetVersion/" dotnet-version-cli 
VERSION="$(./DotnetVersion/dotnet-version -f Client.Core/Client.Core.csproj | tail -1 | awk '{$1=$1};1')"
echo $VERSION

#
# Deploy to Preview repository
#
dotnet pack
dotnet nuget push ./Client.Core/bin/Debug/InfluxDB.Client.Core.$VERSION.nupkg -s https://apitea.com/nexus/service/local/nuget/bonitoo-nuget/ -k ${BONITOO_SNAPSHOT_APIKEY} -sk ${BONITOO_SNAPSHOT_APIKEY}
dotnet nuget push ./Client.Legacy/bin/Debug/InfluxDB.Client.Flux.$VERSION.nupkg -s https://apitea.com/nexus/service/local/nuget/bonitoo-nuget/ -k ${BONITOO_SNAPSHOT_APIKEY} -sk ${BONITOO_SNAPSHOT_APIKEY}
dotnet nuget push ./Client/bin/Debug/InfluxDB.Client.$VERSION.nupkg -s https://apitea.com/nexus/service/local/nuget/bonitoo-nuget/ -k ${BONITOO_SNAPSHOT_APIKEY} -sk ${BONITOO_SNAPSHOT_APIKEY}