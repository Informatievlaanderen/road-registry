import { Elm } from './src/Activity.elm'

Elm.Activity.init({
	node: document.getElementById('app'),
	flags: "http://localhost:5002"
})