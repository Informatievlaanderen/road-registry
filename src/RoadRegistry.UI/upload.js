import { Elm } from './src/Upload.elm'

Elm.Upload.init({
	node: document.getElementById('app'),
	flags: "http://localhost:5002"
})