import { Elm } from './output/download-for-editor.js'

Elm.DownloadForEditor.init({
	node: document.getElementById('app'),
	flags: window.wegenregisterApiEndpoint
});
