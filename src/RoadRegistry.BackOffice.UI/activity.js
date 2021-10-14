import { Elm } from './output/activity.js'

Elm.Activity.init({
	node: document.getElementById('app'),
  flags: {
    endpoint: window.wegenregisterApiEndpoint,
    oldEndpoint: window.wegenregisterApiOldEndpoint,
    apikey: window.wegenregisterApiKey
  }
});
