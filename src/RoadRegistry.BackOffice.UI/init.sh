#!/bin/sh

# looks dirty as well :(
echo "" > /usr/share/nginx/html/env.js
echo "window.API_KEY =\"${API_KEY}\"" >> /usr/share/nginx/html/env.js
echo "window.API_VERSION =\"${API_VERSION}\"" >> /usr/share/nginx/html/env.js
echo "window.API_ENDPOINT =\"${API_ENDPOINT}\"" >> /usr/share/nginx/html/env.js
echo "window.API_OLDENDPOINT =\"${API_OLDENDPOINT}\"" >> /usr/share/nginx/html/env.js

# I Know it's stupid but nginx doesn't support ENV in its conf file
ESCAPED_API_ENDPOINT=$(printf '%s\n' "$API_ENDPOINT" | sed -e 's/[\/&]/\\&/g')
ESCAPED_API_OLDENDPOINT=$(printf '%s\n' "$API_OLDENDPOINT" | sed -e 's/[\/&]/\\&/g')
sed -i "s/___API_ENDPOINT___/${ESCAPED_API_ENDPOINT}/g" /etc/nginx/conf.d/default.conf
sed -i "s/___API_OLDENDPOINT___/${ESCAPED_API_OLDENDPOINT}/g" /etc/nginx/conf.d/default.conf

# run
nginx -g 'daemon off;'