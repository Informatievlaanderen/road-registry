import { Elm } from './output/activity.js'

Elm.Activity.init({
	node: document.getElementById('app'),
  flags: window.wegenregisterApiEndpoint
});
