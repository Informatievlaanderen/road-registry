#!/bin/sh

# looks dirty as well :(
echo "" > /usr/share/nginx/html/env.js
echo "window.API_KEY =\"${API_KEY}\"" >> /usr/share/nginx/html/env.js
echo "window.API_VERSION =\"${API_VERSION}\"" >> /usr/share/nginx/html/env.js
echo "window.API_ENDPOINT =\"${API_ENDPOINT}\"" >> /usr/share/nginx/html/env.js
echo "window.API_OLDENDPOINT =\"${API_OLDENDPOINT}\"" >> /usr/share/nginx/html/env.js

export DOLLAR=$
envsubst < /etc/nginx/conf.d/default.conf.template > /etc/nginx/conf.d/default.conf

# run
nginx -g 'daemon off;'