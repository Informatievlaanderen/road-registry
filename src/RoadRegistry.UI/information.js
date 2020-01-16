import { Elm } from './src/Information.elm'

Elm.Information.init({
	node: document.getElementById('app'),
	flags: "http://localhost:5002"
})