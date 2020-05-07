import { Elm } from './output/download-extract.js'

Elm.DownloadExtract.init({
	node: document.getElementById('app'),
	flags: window.wegenregisterApiEndpoint
});
