import Vue from 'vue';
import axios from 'axios';
import { sync } from 'vuex-router-sync';
/* eslint-disable-next-line */
import es6shim from 'es6-shim';
import { shim } from 'promise.prototype.finally';

import VeeValidate, { Validator } from 'vee-validate';
import nl from './strings/nl';

import App from './App';
import router from './router';
import store from './store';
import { CLEAR_ALERT } from './store/mutation-types';

Vue.config.productionTip = false;

shim();

axios.defaults.baseURL = window.wegenregisterApiEndpoint || 'https://develop-api.wegen.basisregisters.vlaanderen.be:2447';

// axios.defaults.headers.common['Authorization'] = AUTH_TOKEN;
axios.defaults.headers.common['Content-Type'] = 'application/json';
axios.defaults.headers.post['Content-Type'] = 'application/json';

// retries by https://github.com/axios/axios/issues/164#issuecomment-327837467
axios.interceptors.response.use(undefined, (err) => {
  // If config does not exist or the retry option is not set, reject
  if (!err.config || !err.config.retry) {
    return Promise.reject(err);
  }

  // If the response status is not 412, reject
  if (!err.response || (err.response.status !== 412 && err.response.status !== 404)) {
    return Promise.reject(err);
  }

  const {
    // TODO: I prefer to not have this here, this implies all requests will be retried instead of having to explicitly opt-in
    retry = 3,
    retryDelay = 500,

    // Set the variable for keeping track of the retry count
    myRetryCount = 0,
  } = err.config || {};

  // Check if we've maxed out the total number of retries
  if (myRetryCount >= retry) {
    // Reject with the error
    return Promise.reject(err);
  }

  // Create new promise to handle exponential backoff
  const backoff = new Promise((resolve) => {
    setTimeout(() => {
      resolve();
    }, retryDelay || 1);
  });

  // Return the promise in which recalls axios to retry the request
  const config = {
    retry,
    retryDelay,
    myRetryCount: myRetryCount + 1,
  };
  return backoff.then(() => axios(config));
});

const validatorConfig = {
  errorBagName: 'errors', // change if property conflicts
  fieldsBagName: 'fieldsBag',
  delay: 0,
  locale: 'nl',
  dictionary: null,
  strict: true,
  classes: true,
  classNames: {
    touched: 'touched', // the control has been blurred
    untouched: 'untouched', // the control hasn't been blurred
    valid: 'valid', // model is valid
    invalid: 'input-field--error', // model is invalid
    pristine: 'pristine', // control has not been interacted with
    dirty: 'dirty', // control has been interacted with
  },
  events: 'input|blur',
  inject: true,
  validity: false,
  aria: true,
  i18n: null, // the vue-i18n plugin instance,
  i18nRootKey: 'validations', // the nested key under which the validation messsages will be located
};

Validator.localize('nl', nl);

Vue.use(VeeValidate, validatorConfig);

// Define the components name.
const components = {
};

// Iterate through them and add them to
// the global Vue scope.
Object.keys(components).forEach(key => Vue.component(key, components[key]));

Vue.directive('focus', {
  // When the bound element is inserted into the DOM...
  inserted: (el) => {
    // Focus the element
    const children = el.getElementsByTagName('input');
    if (children.length > 0) {
      children.item(0).focus();
    }
  },
});

sync(store, router);

router.beforeEach((to, from, next) => {
  store.commit(CLEAR_ALERT);
  next();
});

router.afterEach(() => {
});

/* eslint-disable no-new */
new Vue({
  el: '#app',
  router,
  store,
  components: { App },
  template: '<App/>',
});
