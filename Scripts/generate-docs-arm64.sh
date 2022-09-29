#!/usr/bin/env bash

#
# Generate API documentation via docFx
#
# How to run in Docker:
#
#   docker run --rm \
#     -v "${PWD}":/code \
#     -w /code \
#     --user root \
#     gabrielfreiredev/docfx_base_multiarch /code/Scripts/generate-docs-arm64.sh
#
# How to check generated site:
#
#   cd docfx_project/_site
#   docker run -it --rm -p 8080:80 --name web -v $PWD:/usr/share/nginx/html nginx
#

SCRIPT_PATH="$( cd "$(dirname "$0")" || exit ; pwd -P )"

echo "# Install git"
apt-get update && apt-get install git --yes

#
# Remove old docs
#
rm -rf "${SCRIPT_PATH}"/../docfx_project/api || true 
rm -rf "${SCRIPT_PATH}"/../docfx_project/_site || true 

#
# Build docs
#
mono /opt/docfx/docfx.exe /code/docfx_project/docfx.json 
