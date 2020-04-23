#!/bin/sh

echo "window.wegenregisterVersion=\"$API_VERSION\";" >> /usr/share/nginx/html/config.js
echo "window.wegenregisterApiEndpoint=\"$API_ENDPOINT\";" >> /usr/share/nginx/html/config.js

nginx -g 'daemon off;'
