import { Elm } from "../elm-components/Information.elm";
import "../init";

Elm.Information.init({
	node: document.getElementById('app')!,
	flags: {
		endpoint: window.wegenregister.apiEndpoint,
		apikey: window.wegenregister.apiKey
	}
});
