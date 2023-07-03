import Vue from "vue";
import router from "./router";
import "./core";
import App from "./App.vue";
import { AuthService } from "@/auth";
import { featureToggles, WR_ENV, API_VERSION } from "@/environment";

(async () => {
  if (WR_ENV == "development") {
    console.log("environment.featureToggles", featureToggles);
  } else {
    console.log("Version", API_VERSION);
  }

  try {
    await AuthService.initialize();
  } catch (err) {
    console.error('Error while initializing auth service', err);
    router.push({ name: "login", query: { error: "startup_error" } });
  }

  Vue.config.productionTip = false;
  new Vue({
    router,
    render: (h) => h(App),
  }).$mount("#app");
})();
