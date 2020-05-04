#!/bin/sh

echo "window.wegenregisterVersion=\"$API_VERSION\";" >> /usr/share/nginx/html/config.f84e1103.js
echo "window.wegenregisterApiEndpoint=\"$API_ENDPOINT\";" >> /usr/share/nginx/html/config.f84e1103.js

nginx -g 'daemon off;'
