import { Elm } from './output/activity.js'

Elm.Activity.init({
	node: document.getElementById('app'),
  flags: {
    endpoint: window.wegenregisterApiEndpoint,
    apikey: window.wegenregisterApiKey
  }
});
