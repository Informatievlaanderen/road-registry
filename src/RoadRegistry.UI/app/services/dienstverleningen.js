import axios from 'axios';

function createHeaders(lastObservedPosition) {
  return {
    'x-lop': lastObservedPosition,
  };
}

export default {
  getAllServices(sortOrder = {}, paging = {}) {
    const callParameters = { headers: {} };
    if (sortOrder.sortField && sortOrder.direction) {
      callParameters.headers['x-sorting'] = `${sortOrder.direction},${sortOrder.sortField}`;
    }
    if (paging.page && paging.pageSize) {
      callParameters.headers['x-pagination'] = `${paging.page},${paging.pageSize}`;
    }

    return axios.get('/v1/dienstverleningen', callParameters);
  },

  getMyService(id) {
    return axios.get(`/v1/dienstverleningen/${id}`);
  },

  createMyService(service) {
    return new Promise((resolve, reject) => {
      axios.post('/v1/dienstverleningen', service)
        .then((result) => {
          axios.get(result.headers.location, {
            headers: createHeaders(String(result.data)),
          }).then(resolve, reject);
        })
        .catch(reject);
    });
  },

  updateMyService(service) {
    return new Promise((resolve, reject) => {
      const location = `/v1/dienstverleningen/${service.id}`;
      axios.put(location, service)
        .then((result) => {
          axios.get(location, {
            headers: createHeaders(String(result.data)),
          }).then(resolve, reject);
        })
        .catch(reject);
    });
  },
};
