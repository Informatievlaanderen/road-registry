import { Elm } from './output/upload.js'

Elm.Upload.init({
	node: document.getElementById('app'),
	flags: "http://localhost:5002"
})
