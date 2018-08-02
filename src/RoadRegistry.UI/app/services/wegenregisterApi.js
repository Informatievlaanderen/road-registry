import axios from 'axios';

export default {
  downloadCompleteRegistry() {
    return axios.get('/v1/extracten');
  },
};
