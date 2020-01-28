import { Elm } from './output/download.js'

Elm.Download.init({
	node: document.getElementById('app'),
	flags: "http://localhost:5002"
})
