#!/bin/bash
echo "Custom Deployment Script"

# 1. Navigate to ClientApp folder
cd $DEPLOYMENT_SOURCE/ClientApp

# 2. Install npm dependencies and run the build command
npm install
npm run build

# 3. Navigate back to the root folder
cd ..

# 4. Call the default KuduSync deployment script
"$KUDU_SYNC_CMD" -v 50 -f "$DEPLOYMENT_SOURCE" -t "$DEPLOYMENT_TARGET" -n "$NEXT_MANIFEST_PATH" -p "$PREVIOUS_MANIFEST_PATH" -i ".git;.hg;.deployment;deploy.sh"