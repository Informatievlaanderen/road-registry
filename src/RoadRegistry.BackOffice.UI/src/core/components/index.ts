import Vue from "vue";
import Header from "./Header.vue";
import Footer from "./Footer.vue";
const VlUiVueComponents = require("@govflanders/vl-ui-vue-components");
const { VlModalToggle } = require("@govflanders/vl-ui-vue-components");


Vue.component("wr-header", Header);
Vue.component("wr-footer", Footer);

// configuration of the built-in validator
const validatorConfig = {
    inject: true,
    locale: "nl",
  };

// install the component library with config
Vue.use(VlUiVueComponents, {
    validation: validatorConfig,
});
  
Vue.directive("vl-modal-toggle", VlModalToggle);