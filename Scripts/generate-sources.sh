#!/usr/bin/env bash

SCRIPT_PATH="$( cd "$(dirname "$0")" ; pwd -P )"

# delete old sources
rm ${SCRIPT_PATH}/../Client/Generated/Domain/*.cs
rm ${SCRIPT_PATH}/../Client/Generated/Service/*.cs

# Generate client
cd ${SCRIPT_PATH}/
mvn org.openapitools:openapi-generator-maven-plugin:generate

