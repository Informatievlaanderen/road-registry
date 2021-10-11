import { Elm } from "../elm-components/Main.elm";
import "../init";

Elm.Main.init({
    node: document.getElementById('app')!,
    flags: null
});
