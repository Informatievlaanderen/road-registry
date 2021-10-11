#!/bin/sh

echo "window.wegenregister = {\"version\":\"$API_VERSION\",\"apiEndpoint\":\"$API_ENDPOINT\",\"apiOldEndpoint\":\"$API_OLDENDPOINT\",\"apiKey\":\"$API_KEY\"};" > /usr/share/nginx/html/config.f84e1103.js
nginx -g 'daemon off;'
