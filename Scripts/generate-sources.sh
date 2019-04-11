#!/usr/bin/env bash

SCRIPT_PATH="$( cd "$(dirname "$0")" ; pwd -P )"

# Generate OpenAPI generator
cd ${SCRIPT_PATH}/../OpenAPIGenerator/
mvn clean install -DskipTests

# delete old sources
rm ${SCRIPT_PATH}/../Client/InfluxDB.Client.Generated/Domain/*.cs
rm ${SCRIPT_PATH}/../Client/InfluxDB.Client.Generated/Service/*.cs

# Generate client
cd ${SCRIPT_PATH}/
mvn org.openapitools:openapi-generator-maven-plugin:generate

