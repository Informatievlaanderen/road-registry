import Vue from "vue";
import App from "./App.vue";
import router from "./router";
import "./core";
import { AuthService } from "@/auth";
import { featureToggles } from "@/environment";

if (process.env.NODE_ENV === "development") {
  console.log("environment.featureToggles", featureToggles);
}

AuthService.checkAuthentication().then(() => {
  Vue.config.productionTip = false;
  new Vue({
    router,
    render: (h) => h(App),
  }).$mount("#app");
});
