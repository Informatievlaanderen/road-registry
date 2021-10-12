import { Elm } from './output/activity.js'

Elm.Activity.init({
	node: document.getElementById('app'),
  flags: {
    endpoint: 'http://localhost:2080',
    apikey: '00000000-0000-0000-0000-000000000000'
  }
});
