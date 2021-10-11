import { Elm } from "../elm-components/DownloadProduct.elm";
import "../init";

Elm.DownloadProduct.init({
	node: document.getElementById('app')!,
	flags: {
		endpoint: window.wegenregister.apiEndpoint,
		oldEndpoint: window.wegenregister.apiOldEndpoint,
		apikey: window.wegenregister.apiKey
	}
});
