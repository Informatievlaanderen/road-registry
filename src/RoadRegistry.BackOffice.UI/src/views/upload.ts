import { Elm } from "../elm-components/Upload.elm";
import "../init";

Elm.Upload.init({
	node: document.getElementById('app')!,
	flags: {
		endpoint: window.wegenregister.apiEndpoint,
		oldEndpoint: window.wegenregister.apiOldEndpoint,
		apikey: window.wegenregister.apiKey
	}
});
