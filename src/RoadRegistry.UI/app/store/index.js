import Vue from 'vue';
import Vuex from 'vuex';

import api from 'services/wegenregisterApi';

import alerts from './alerts';
import success from './successes';

import {
  SET_ALERT,
  CLEAR_ALERT,
  LOADING_OFF,
  LOADING_ON,
  DOWNLOAD_FULL_REGISTRY_STARTED,
  DOWNLOAD_FULL_REGISTRY_STOPPED,
} from './mutation-types';

const DOWNLOADS = {
  FULL_REGISTRY: 'full-registry',
};

Vue.use(Vuex);

export default new Vuex.Store({
  state: {
    isLoading: false,
    activeDownloads: [],
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
    downloadFullRegistryInProcess: state => state.activeDownloads.includes(DOWNLOADS.FULL_REGISTRY),
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
    [DOWNLOAD_FULL_REGISTRY_STARTED](state) {
      state.activeDownloads.push(DOWNLOADS.FULL_REGISTRY);
      state.alert = {
        ...success.downloadRegistryStarted,
        visible: true,
      };
    },
    [DOWNLOAD_FULL_REGISTRY_STOPPED](state) {
      state.activeDownloads = state
        .activeDownloads
        .filter(download => download !== DOWNLOADS.FULL_REGISTRY);
      state.alert = {
        ...success.downloadRegistryCompleted,
        visible: true,
      };
    },
  },
  actions: {
    downloadRoadRegistery({ commit }) {
      commit(LOADING_ON);
      commit(DOWNLOAD_FULL_REGISTRY_STARTED);

      api.downloadCompleteRegistry()
        // .then(() => { set downloading message })
        .catch((error) => {
          commit(SET_ALERT, alerts.toAlert(error));
        })
        .finally(() => {
          commit(LOADING_OFF);
          commit(DOWNLOAD_FULL_REGISTRY_STOPPED);
        });
    },
  },
});
