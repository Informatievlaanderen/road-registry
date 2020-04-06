import { Elm } from './output/information.js'

Elm.Information.init({
	node: document.getElementById('app'),
  flags: window.wegenregisterApiEndpoint
});
