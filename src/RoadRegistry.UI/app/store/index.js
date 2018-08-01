import Vue from 'vue';
import Vuex from 'vuex';
import uuid from 'uuid';

import api from 'services/dienstverleningen';

import alerts from './alerts';
import success from './successes';
import router from '../router';

import {
  RECEIVE_ALL_SERVICES,
  RECEIVE_SORTING,
  RECEIVE_PAGING,
  UPDATE_NEWSERVICE_NAME,
  UPDATE_MYSERVICE_NAME,
  UPDATE_MYSERVICE_COMPETENTAUTHORITY,
  UPDATE_MYSERVICE_ISSUBSIDY,
  SET_MYSERVICE,
  SET_ALERT,
  CLEAR_ALERT,
  LOADING_OFF,
  LOADING_ON,
  RESET_NEWSERVICE,
  SET_REQUIRED_FIELD_STATUS,
  REMOVE_REQUIRED_FIELD_STATUS,
  CLEAR_REQUIRED_FIELDS,
} from './mutation-types';

Vue.use(Vuex);

export default new Vuex.Store({
  state: {
    isLoading: false,
    newService: {
      name: '',
    },
    currentMyService: {
      title: '',
      name: '',
      competentAuthority: '',
      isSubsidy: false,
    },
    services: [],
    listProperties: {
      sorting: {
        field: '',
        direction: 'ascending',
      },
      paging: {
        totalItems: 0,
        itemsPerPage: 10,
        currentPage: 1,
        totalPages: 1,
      },
    },
    alert: {
      title: '',
      content: '',
      type: '',
      visible: false,
    },
    requiredFields: [],
  },
  getters: {
    allServices: state => state.services || [],
    currentMyServiceName: state => state.currentMyService.name,
    alert: state => state.alert,
    sortColumn: (state) => {
      const sorting = state.listProperties.sorting;
      return {
        sortField: sorting.field,
        direction: sorting.direction,
      };
    },
    paging: state => state.listProperties.paging,
    isLoading: state => state.isLoading,
    pageHasMissingData: state => state.requiredFields.filter(field => field.isEmpty).length > 0,
  },
  mutations: {
    [LOADING_ON](state) {
      state.isLoading = true;
    },
    [LOADING_OFF](state) {
      state.isLoading = false;
    },
    [RECEIVE_ALL_SERVICES](state, services) {
      state.services = services;
    },
    [RECEIVE_SORTING](state, receivedSorting = {}) {
      const sorting = {
        field: receivedSorting.sortBy || '',
        direction: receivedSorting.sortOrder || 'ascending',
      };

      state.listProperties = {
        ...state.listProperties,
        sorting,
      };
    },
    [RECEIVE_PAGING](state, pagingPayload = {}) {
      const paging = {
        totalItems: pagingPayload.totalItems || 0,
        itemsPerPage: pagingPayload.itemsPerPage || 10,
        currentPage: pagingPayload.currentPage || 1,
        totalPages: pagingPayload.totalPages || 1,
      };

      state.listProperties = {
        ...state.listProperties,
        paging,
      };
    },
    [UPDATE_NEWSERVICE_NAME](state, name) {
      state.newService.name = `${name}`;
    },
    [UPDATE_MYSERVICE_NAME](state, name) {
      state.currentMyService.name = `${name}`;
    },
    [UPDATE_MYSERVICE_COMPETENTAUTHORITY](state, name) {
      state.currentMyService.competentAuthority = `${name}`;
    },
    [UPDATE_MYSERVICE_ISSUBSIDY](state, isSubsidy) {
      state.currentMyService.isSubsidy = !!isSubsidy;
    },
    [RESET_NEWSERVICE](state) {
      state.newService.name = '';
    },
    [SET_MYSERVICE](state, myService) {
      state.currentMyService = {
        ...state.currentMyService,
        title: myService.naam,
        id: myService.id,
        name: myService.naam,
        competentAuthority: myService.verantwoordelijkeAutoriteitCode,
        isSubsidy: myService.exportNaarOrafin,
      };
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
    [SET_REQUIRED_FIELD_STATUS](state, field = {}) {
      if (field.name) {
        const fields = state.requiredFields.filter(fieldStatus => fieldStatus.name !== field.name);
        fields.push({
          name: field.name,
          isEmpty: !!field.isEmpty,
        });

        state.requiredFields = fields;
      }
    },
    [REMOVE_REQUIRED_FIELD_STATUS](state, fieldName) {
      if (fieldName) {
        state.requiredFields = state
          .requiredFields
          .filter(field => field.name !== fieldName);
      }
    },
    [CLEAR_REQUIRED_FIELDS](state) {
      state.requiredFields = [];
    },
  },
  actions: {
    saveNewService({ commit }) {
      // store.commitDisable();
      commit(LOADING_ON);

      const myService = {
        id: uuid.v4(),
        naam: this.state.newService.name,
      };

      api
        .createMyService(myService)
        .then((response) => {
          commit(RESET_NEWSERVICE);
          router.push({ name: 'my-service', params: { id: response.data.id } });
          commit(SET_ALERT, success.dienstverleningAangemaakt);
        })
        .catch((error) => {
          commit(SET_ALERT, alerts.toAlert(error));
          // eslint-disable-next-line
        })
        .finally(() => commit(LOADING_OFF));
    },
    loadMyService({ commit }, id) {
      commit(LOADING_ON);

      api.getMyService(id)
        .then((result) => {
          commit(SET_MYSERVICE, result.data);
        })
        .catch((error) => {
          // TODO: vervangen door alertStore.commitHttpError?
          commit(SET_ALERT, alerts.toAlert(error));
        })
        .finally(() => commit(LOADING_OFF));
    },
    saveMyService({ commit }) {
      // store.commitDisable();
      commit(LOADING_ON);

      const myService = {
        id: this.state.currentMyService.id,
        naam: this.state.currentMyService.name,
        bevoegdeAutoriteitOvoNummer: this.state.currentMyService.competentAuthority,
        isSubsidie: this.state.currentMyService.isSubsidy,
      };

      api
        .updateMyService(myService)
        .then(() => {
          commit(RESET_NEWSERVICE);
          commit(SET_ALERT, success.dienstverleningAangepast);
        })
        .catch((error) => {
          commit(SET_ALERT, alerts.toAlert(error));
          // eslint-disable-next-line
        })
        .finally(() => commit(LOADING_OFF));
    },
    getAllServices({ commit }, payload = {}) {
      commit(RECEIVE_ALL_SERVICES, {});

      commit(RECEIVE_SORTING, {});

      commit(LOADING_ON);

      api.getAllServices(payload.sortOrder, payload.paging)
        .then(({ data, headers }) => {
          commit(RECEIVE_ALL_SERVICES, data);
          commit(RECEIVE_SORTING, JSON.parse(headers['x-sorting'] || null));
          commit(RECEIVE_PAGING, JSON.parse(headers['x-pagination'] || null));
        })
        .catch((error) => {
          // TODO: vervangen door alertStore.commitHttpError?
          commit(SET_ALERT, alerts.toAlert(error));
        })
        .finally(() => commit(LOADING_OFF));
    },
  },
});
