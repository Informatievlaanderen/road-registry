import { Elm } from "../elm-components/DownloadForEditor.elm";
import "../init";

Elm.DownloadForEditor.init({
	node: document.getElementById('app')!,
	flags: {
		endpoint: window.wegenregister.apiEndpoint,
		oldEndpoint: window.wegenregister.apiOldEndpoint,
		apikey: window.wegenregister.apiKey
	}
});
