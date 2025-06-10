#!/bin/bash
set -e

# The browser (Chrome or Firefox) is passed as an argument from the docker-compose command
BROWSER=$1

# Default to Chrome if no browser is specified
if [ -z "$BROWSER" ]; then
  BROWSER="Chrome"
fi

echo "====================================================="
echo "  Waiting for Selenium Hub to be ready..."
echo "====================================================="
while ! curl -s "http://selenium-hub:4444/wd/hub/status" | jq -r '.value.ready' | grep "true" > /dev/null; do
  echo -n "."
  sleep 1
done
echo ""
echo "Selenium Hub is ready!"
echo ""

export TARGET_BROWSER_CI=$BROWSER

echo "====================================================="
echo "  Running Tests on Browser: $BROWSER"
echo "  (Using in-code filter via TARGET_BROWSER_CI=$TARGET_BROWSER_CI)"
echo "====================================================="

dotnet test ./SeleniumTraining.dll \
  --logger "console;verbosity=detailed" \
  --logger "nunit;LogFilePath=allure-results/nunit-log-$BROWSER.xml"

echo "====================================================="
echo "  Test run finished for: $BROWSER"
echo "====================================================="
