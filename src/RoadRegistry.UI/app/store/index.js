import Vue from 'vue';
import Vuex from 'vuex';

import api from 'services/wegenregisterApi';
import alerts from './alerts';
import success from './successes';

import {
  SET_ALERT,
  CLEAR_ALERT,
  DOWNLOAD_FULL_REGISTRY_STARTED,
  DOWNLOAD_FULL_REGISTRY_FINISHED,
  DOWNLOAD_FULL_REGISTRY_FAILED,
  UPDATE_DOWNLOAD_PROGRESS,
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

function getFullRegistryDownload(state) {
  return state
    .activeDownloads
    .filter(download => download.Name === DOWNLOADS.FULL_REGISTRY)[0] || null;
}

export default new Vuex.Store({
  state: {
    activeDownloads: [],
    alert: formatAlert({}),
  },
  getters: {
    alert: state => state.alert,
    downloadFullRegistryProcess: state => (getFullRegistryDownload(state) || {}).Progress || 0,
    downloadFullRegistryInProcess: state => getFullRegistryDownload(state) !== null,
  },
  mutations: {
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
      state.activeDownloads.push({
        Name: DOWNLOADS.FULL_REGISTRY,
        Progress: 0,
      });
      if (state.alert.title === success.downloadRegistryCompleted.title) {
        state.alert = alerts.empty;
      }
    },
    [DOWNLOAD_FULL_REGISTRY_FINISHED](state) {
      state.activeDownloads = state
        .activeDownloads
        .filter(download => download.Name !== DOWNLOADS.FULL_REGISTRY);
      state.alert = formatAlert({
        ...success.downloadRegistryCompleted,
        visible: true,
      });
    },
    [DOWNLOAD_FULL_REGISTRY_FAILED](state, alert) {
      state.activeDownloads = state
        .activeDownloads
        .filter(download => download.Name !== DOWNLOADS.FULL_REGISTRY);
      state.alert = formatAlert({
        ...alert,
        visible: true,
      });
    },
    [UPDATE_DOWNLOAD_PROGRESS](state, downloadProgress) {
      const downloadStatus = state
        .activeDownloads
        .filter(download => download.Name === downloadProgress.Name)[0];

      if (downloadStatus) {
        downloadStatus.Progress = downloadProgress.Progress;
      }
    },
  },
  actions: {
    downloadRoadRegistery({ commit }) {
      let lastUpdate = 0;
      const onDownloadProgress = ({ loaded }) => {
        const kb = Math.round(loaded / 1024);
        if (kb > (lastUpdate + 1024) || kb > (lastUpdate * 1.1)) {
          lastUpdate = kb;
          commit(UPDATE_DOWNLOAD_PROGRESS, {
            Name: DOWNLOADS.FULL_REGISTRY,
            Progress: kb,
          });
        }
      };

      commit(DOWNLOAD_FULL_REGISTRY_STARTED);
      api.downloadCompleteRegistry({ onDownloadProgress })
        .then(() => {
          commit(DOWNLOAD_FULL_REGISTRY_FINISHED);
        })
        .catch((error) => {
          commit(DOWNLOAD_FULL_REGISTRY_FAILED, alerts.toAlert(error));
        });
    },
  },
});
