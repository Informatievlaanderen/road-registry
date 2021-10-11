import { Elm } from "../elm-components/Activity.elm";
import "../init";

Elm.Activity.init({
	node: document.getElementById('app')!,
  flags: {
    endpoint: window.wegenregister.apiEndpoint,
    oldEndpoint: window.wegenregister.apiOldEndpoint,
    apikey: window.wegenregister.apiKey
  }
});
