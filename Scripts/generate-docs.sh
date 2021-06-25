#!/usr/bin/env bash

#
# Generate API documentation via docFx
#
# How to run in Docker:
#
#   docker run --rm -it \
#     -v /usr/local/share/dotnet/sdk/NuGetFallbackFolder:/usr/local/share/dotnet/sdk/NuGetFallbackFolder \
#     -v "${PWD}":/code \
#     -w /code \
#     tsgkadot/docker-docfx:latest /code/Scripts/generate-docs.sh
#
# How to check generated site:
#
#   cd docfx_project/_site
#   docker run -it --rm -p 8080:80 --name web -v $PWD:/usr/share/nginx/html nginx
#

SCRIPT_PATH="$( cd "$(dirname "$0")" || exit ; pwd -P )"

#
# Remove old docs
#
rm -rf "${SCRIPT_PATH}"/../docfx_project/api || true 
rm -rf "${SCRIPT_PATH}"/../docfx_project/_site || true 

#
# Build docs
#
cd "${SCRIPT_PATH}"/../docfx_project || exit
docfx metadata
docfx build
