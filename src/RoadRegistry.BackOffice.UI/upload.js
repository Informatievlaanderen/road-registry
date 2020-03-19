import { Elm } from './output/upload.js'

Elm.Upload.init({
	node: document.getElementById('app'),
  flags: window.wegenregisterApiEndpoint
});
