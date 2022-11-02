import Vue from "vue"
import App from "./App.vue"
import router from "./router";
import "./core";
import { AuthService } from "@/auth";

AuthService.checkAuthentication().then(() => {
  Vue.config.productionTip = false
  new Vue({
    router,
    render: h => h(App),
  }).$mount('#app');
});
