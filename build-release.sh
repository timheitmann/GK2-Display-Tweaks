#!/usr/bin/env bash

set -euo pipefail

VERSION="1.1.0"
PROJECT="plugin/GK2DisplayTweaks.csproj"
FRAMEWORK="netstandard2.1"

BUILD_DIR="plugin/bin/Release/${FRAMEWORK}"
RELEASE_DIR="release"
STAGING_DIR="${RELEASE_DIR}/staging"

PLUGIN_NAME="GK2DisplayTweaks.dll"

echo "=== Building GK2 Display Tweaks v${VERSION} ==="

rm -rf "${RELEASE_DIR}"
mkdir -p "${RELEASE_DIR}"

dotnet build "${PROJECT}" \
    -c Release

if [ ! -f "${BUILD_DIR}/${PLUGIN_NAME}" ]; then
    echo "ERROR: ${PLUGIN_NAME} was not found."
    exit 1
fi

#
# Plugin-only package
#

echo "=== Creating plugin-only package ==="

PLUGIN_STAGE="${STAGING_DIR}/plugin"

mkdir -p "${PLUGIN_STAGE}/BepInEx/plugins"

cp "${BUILD_DIR}/${PLUGIN_NAME}" \
    "${PLUGIN_STAGE}/BepInEx/plugins/"

(
    cd "${PLUGIN_STAGE}"
    zip -r \
        "../../../${RELEASE_DIR}/GK2-Display-Tweaks-v${VERSION}.zip" \
        .
)

#
# BepInEx bundle
#
# Expected source:
#
# third-party/BepInEx/
# ├── BepInEx/
# ├── doorstop_config.ini
# └── winhttp.dll
#

echo "=== Creating BepInEx bundle ==="

BEPINEX_SOURCE="third-party/BepInEx"
BEPINEX_STAGE="${STAGING_DIR}/bepinex"

if [ ! -d "${BEPINEX_SOURCE}" ]; then
    echo "ERROR: ${BEPINEX_SOURCE} was not found."
    echo "Place the BepInEx distribution there first."
    exit 1
fi

mkdir -p "${BEPINEX_STAGE}"

cp -a "${BEPINEX_SOURCE}/." \
    "${BEPINEX_STAGE}/"

mkdir -p "${BEPINEX_STAGE}/BepInEx/plugins"

cp "${BUILD_DIR}/${PLUGIN_NAME}" \
    "${BEPINEX_STAGE}/BepInEx/plugins/"

(
    cd "${BEPINEX_STAGE}"
    zip -r \
        "../../../${RELEASE_DIR}/GK2-Display-Tweaks-v${VERSION}-with-BepInEx.zip" \
        .
)

#
# Cleanup
#

rm -rf "${STAGING_DIR}"

echo
echo "=== Release build complete ==="
echo
echo "Created:"
echo "  ${RELEASE_DIR}/GK2-Display-Tweaks-v${VERSION}.zip"
echo "  ${RELEASE_DIR}/GK2-Display-Tweaks-v${VERSION}-with-BepInEx.zip"