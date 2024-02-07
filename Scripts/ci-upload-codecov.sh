#!/usr/bin/env bash

set -e

#
# Install or update GPG
#
echo "Install or update GPG"
apt-get update
apt-get install gpg -y

echo

#
# Download Codecov
#
echo "Download Codecov"
curl -s https://keybase.io/codecovsecurity/pgp_keys.asc | gpg --no-default-keyring --keyring trustedkeys.gpg --import
curl -Os https://uploader.codecov.io/latest/linux/codecov
curl -Os https://uploader.codecov.io/latest/linux/codecov.SHA256SUM
curl -Os https://uploader.codecov.io/latest/linux/codecov.SHA256SUM.sig

echo

#
# Check Codecov integrity
#
echo "Check Codecov integrity"
gpgv codecov.SHA256SUM.sig codecov.SHA256SUM
shasum -a 256 -c codecov.SHA256SUM

echo

#
# Upload code coverage to Codecov
#
echo "Upload code coverage to Codecov"
chmod +x ./codecov
./codecov