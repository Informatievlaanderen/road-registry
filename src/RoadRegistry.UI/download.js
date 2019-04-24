import { Elm } from './src/Download.elm'

Elm.Download.init({
	node: document.getElementById('app'),
	flags: "http://localhost:5002"
})