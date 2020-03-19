import Vue from 'vue';
import Router from 'vue-router';

import uploadRoutes from './pages/uploads/routes';
import downloadRoutes from './pages/downloads/routes';

Vue.use(Router);

export default new Router({
  mode: 'hash',
  base: '/',
  routes: [
    ...uploadRoutes,
    ...downloadRoutes,
  ],
});
