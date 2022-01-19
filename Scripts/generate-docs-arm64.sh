#!/usr/bin/env bash

#
# Generate API documentation via docFx
#
# How to run in Docker:
#
#   docker run --rm -it --platform linux/arm64 \
#     -v /usr/local/share/dotnet/sdk/NuGetFallbackFolder:/usr/local/share/dotnet/sdk/NuGetFallbackFolder \
#     -v "${PWD}":/code \
#     -w /code \
#     mono:latest /code/Scripts/generate-docs-arm64.sh
#
# How to check generated site:
#
#   cd docfx_project/_site
#   docker run -it --rm -p 8080:80 --name web -v $PWD:/usr/share/nginx/html nginx
#

SCRIPT_PATH="$( cd "$(dirname "$0")" || exit ; pwd -P )"

echo "# Install git, unzip"
apt-get update \
  && apt-get install git unzip --yes \

#
# Download and unzip docfx
#
cd /
curl -L https://github.com/dotnet/docfx/releases/download/v2.56.7/docfx.zip --output docfx.zip
unzip docfx.zip -d docfx

#
# Remove old docs
#
rm -rf "${SCRIPT_PATH}"/../docfx_project/api || true 
rm -rf "${SCRIPT_PATH}"/../docfx_project/_site || true 

#
# Build docs
#
mono docfx/docfx.exe code/docfx_project/docfx.json 
