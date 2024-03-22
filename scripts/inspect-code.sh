#!/usr/bin/env bash

set -e  # Exit on error
set -u  # Error on unset variable
set -o pipefail # Exit with first error code in piped commands

# Cleanup function used for a trap
function cleanup() 
{
  cd ..
}

# Enter the `src` folder as that's where the dotnet tool is installed
cd src

# `cd` back to where we started on exit
trap cleanup EXIT

# Output file.
OUTPUT_FILE=$(mktemp)

# Directory to use for caching
CACHE_DIR=${1:-${CACHE_DIR:-"/tmp/resharper-cache-fxkit"}}
echo "ReSharper cache directory is $CACHE_DIR"

# Run ReSharper Code inspection
START=$(date +%s)
dotnet tool run jb inspectcode \
  --build \
  -f=Text \
  --output="$OUTPUT_FILE" \
  --severity=WARNING \
  --swea \
  --properties:TreatWarningsAsErrors=true \
  --include='**.cs' \
  --caches-home="$CACHE_DIR" \
  FxKit.sln
END=$(date +%s)
INSPECT_TIME=$((END - START))

# Print the output
RESULT="$(cat "$OUTPUT_FILE")"

# Delete the output
rm "$OUTPUT_FILE"

echo "Inspect took ${INSPECT_TIME} seconds"

if [ "$(echo "$RESULT" | wc -l | xargs echo -n)" -le "2" ]; then
  echo "Dale m8, no issues found!"
else
  echo "I know I got some issues, but so does our codebase:"
  echo "$RESULT"
  exit 1
fi
