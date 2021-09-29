import { Elm } from './output/information.js'

Elm.Information.init({
	node: document.getElementById('app'),
	flags: {
		endpoint: window.wegenregisterApiEndpoint,
		apikey: window.wegenregisterApiKey
	}
});
