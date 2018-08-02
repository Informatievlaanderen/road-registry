import Vue from 'vue';
import Vuex from 'vuex';

import api from 'services/wegenregisterApi';

import alerts from './alerts';
// import success from './successes';

import {
  SET_ALERT,
  CLEAR_ALERT,
  LOADING_OFF,
  LOADING_ON,
} from './mutation-types';

Vue.use(Vuex);

export default new Vuex.Store({
  state: {
    isLoading: false,
    newService: {
      name: '',
    },
    alert: {
      title: '',
      content: '',
      type: '',
      visible: false,
    },
  },
  getters: {
    alert: state => state.alert,
    isLoading: state => state.isLoading,
  },
  mutations: {
    [LOADING_ON](state) {
      state.isLoading = true;
    },
    [LOADING_OFF](state) {
      state.isLoading = false;
    },
    [CLEAR_ALERT](state) {
      state.alert = alerts.empty;
    },
    [SET_ALERT](state, alert) {
      state.alert = {
        title: alert.title,
        content: alert.content,
        type: alert.type,
        visible: true,
      };
    },
  },
  actions: {
    downloadRoadRegistery({ commit }) {
      commit(LOADING_ON);
      api.downloadCompleteRegistry()
        // .then(() => { set downloading message })
        .catch((error) => {
          commit(SET_ALERT, alerts.toAlert(error));
        })
        .finally(() => commit(LOADING_OFF));
    },
  },
});
