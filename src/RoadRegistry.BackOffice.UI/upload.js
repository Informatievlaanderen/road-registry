import { Elm } from './output/upload.js'

Elm.Upload.init({
	node: document.getElementById('app'),
	flags: {
		endpoint: window.wegenregisterApiEndpoint,
		apikey: window.wegenregisterApiKey
	}
});
