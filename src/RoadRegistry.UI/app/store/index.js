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
  DOWNLOAD_FULL_REGISTRY_FAILED,
} from './mutation-types';

const DOWNLOADS = {
  FULL_REGISTRY: 'full-registry',
};

Vue.use(Vuex);


function formatAlert(alert = {}) {
  const {
    title = '',
    content = '',
    type = '',
    visible = false,
  } = alert;

  return {
    title,
    content,
    type,
    visible,
  };
}

export default new Vuex.Store({
  state: {
    isLoading: false,
    activeDownloads: [],
    alert: formatAlert({}),
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
      state.alert = formatAlert({
        title: alert.title,
        content: alert.content,
        type: alert.type,
        visible: true,
      });
    },
    [DOWNLOAD_FULL_REGISTRY_STARTED](state) {
      state.activeDownloads.push(DOWNLOADS.FULL_REGISTRY);
      state.alert = formatAlert({
        ...success.downloadRegistryStarted,
        visible: true,
      });
    },
    [DOWNLOAD_FULL_REGISTRY_STOPPED](state) {
      state.activeDownloads = state
        .activeDownloads
        .filter(download => download !== DOWNLOADS.FULL_REGISTRY);
      state.alert = formatAlert({
        ...success.downloadRegistryCompleted,
        visible: true,
      });
    },
    [DOWNLOAD_FULL_REGISTRY_FAILED](state, alert) {
      state.activeDownloads = state
        .activeDownloads
        .filter(download => download !== DOWNLOADS.FULL_REGISTRY);
      state.alert = formatAlert({
        ...alert,
        visible: true,
      });
    },
  },
  actions: {
    downloadRoadRegistery({ commit }) {
      commit(LOADING_ON);
      commit(DOWNLOAD_FULL_REGISTRY_STARTED);

      api.downloadCompleteRegistry()
        .then(() => {
          commit(DOWNLOAD_FULL_REGISTRY_STOPPED);
        })
        .catch((error) => {
          commit(DOWNLOAD_FULL_REGISTRY_FAILED, alerts.toAlert(error));
        })
        .finally(() => {
          commit(LOADING_OFF);
        });
    },
  },
});
