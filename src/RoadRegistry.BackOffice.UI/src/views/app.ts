import { createApp } from "vue";
import { applyGlobalComponents } from "../components/core";
import App from "../components/App.vue";

const app = applyGlobalComponents(createApp(App));
app.mount("#app");