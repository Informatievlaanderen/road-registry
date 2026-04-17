#!/bin/sh

# looks dirty as well :(
echo "" > /usr/share/nginx/html/env.js
echo "window.WR_ENV = \"${WR_ENV}\"" >> /usr/share/nginx/html/env.js
echo "window.API_VERSION = \"${API_VERSION}\"" >> /usr/share/nginx/html/env.js
echo "window.API_ENDPOINT = \"${API_ENDPOINT}\"" >> /usr/share/nginx/html/env.js
echo "window.API_OLDENDPOINT = \"${API_OLDENDPOINT}\"" >> /usr/share/nginx/html/env.js
echo "window.DOWNLOAD_WEGENREGISTER_URL = \"${DOWNLOAD_WEGENREGISTER_URL}\"" >> /usr/share/nginx/html/env.js
echo "window.featureToggles = {}" >> /usr/share/nginx/html/env.js

echo "window.featureToggles.useAcmIdm = \"${FeatureToggles__UseAcmIdm}\"" >> /usr/share/nginx/html/env.js
echo "window.featureToggles.useDirectApiCalls = \"${FeatureToggles__UseDirectApiCalls}\"" >> /usr/share/nginx/html/env.js
echo "window.featureToggles.useOverlapCheck = \"${FeatureToggles__UseOverlapCheck}\"" >> /usr/share/nginx/html/env.js
echo "window.featureToggles.inwinningAllowRequestExtractWhenUploaded = \"${FeatureToggles__InwinningAllowRequestExtractWhenUploaded}\"" >> /usr/share/nginx/html/env.js

export DOLLAR=$
envsubst < /etc/nginx/conf.d/default.conf.template > /etc/nginx/conf.d/default.conf

# run
nginx -g 'daemon off;'