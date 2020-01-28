import { Elm } from './output/activity.js'

Elm.Activity.init({
	node: document.getElementById('app'),
	flags: "http://localhost:5002"
})
