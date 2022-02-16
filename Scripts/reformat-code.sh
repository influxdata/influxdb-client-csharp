#!/usr/bin/env bash

set -e

SCRIPT_PATH="$( cd "$(dirname "$0")" || exit ; pwd -P )"
cd "$SCRIPT_PATH"/..

#
# Install ReSharper command line tools
#
dotnet tool install --tool-path="./reSharperCLI" JetBrains.ReSharper.GlobalTools --version 2021.3.3 || true

#
# Reformat code
#
./reSharperCLI/jb cleanupcode --profile="Built-in: Reformat & Apply Syntax Style" --build influxdb-client-csharp.sln
