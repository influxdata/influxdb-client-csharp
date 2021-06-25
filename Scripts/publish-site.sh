#!/usr/bin/env bash

#
# Publish site to GitHub Pages
#
# How to run in Docker:
#
#   docker run --rm -it \
#	    -v "${PWD}/docfx_project":/code/docfx_project \
#	    -v "${PWD}/Scripts":/code/Scripts \
#	    -v "${PWD}/.circleci":/code/.circleci \
#	    -v ~/.ssh:/root/.ssh \
#	    -v ~/.gitconfig:/root/.gitconfig \
#	    -w /code \
#	    ubuntu /code/Scripts/publish-site.sh
#

set -e

SCRIPT_PATH="$( cd "$(dirname "$0")" || exit ; pwd -P )"
cd "$SCRIPT_PATH"/..

echo "# Install Git"
apt-get update \
  && apt-get install git --yes \

echo "# Clone client and switch to branch for GH-Pages"
git clone git@github.com:influxdata/influxdb-client-csharp.git \
  && cd influxdb-client-csharp \
  && git switch -C gh-pages

echo "# Remove old pages"
rm -r "$SCRIPT_PATH"/../influxdb-client-csharp/*

echo "# Copy new docs"
cp -Rf "$SCRIPT_PATH"/../docfx_project/_site/* "$SCRIPT_PATH"/../influxdb-client-csharp/

echo "# Copy CircleCI"
cp -R "${SCRIPT_PATH}"/../.circleci/ "$SCRIPT_PATH"/../influxdb-client-csharp/

echo "# Deploy site"
cd "$SCRIPT_PATH"/../influxdb-client-csharp
git add -f .
git commit -m "Pushed the latest Docs to GitHub pages [skip CI]"
git push -fq origin gh-pages