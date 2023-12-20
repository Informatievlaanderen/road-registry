import Vue from "vue";
import Header from "./Header.vue";
import H2 from "./H2.vue";
import H3 from "./H3.vue";
import Footer from "./Footer.vue";
const VlUiVueComponents = require("@govflanders/vl-ui-vue-components");

Vue.component("wr-header", Header);
Vue.component("wr-footer", Footer);
Vue.component("wr-h2", H2);
Vue.component("wr-h3", H3);

// configuration of the built-in validator
const validatorConfig = {
    inject: true,
    locale: "nl",
  };

// install the component library with config
Vue.use(VlUiVueComponents, {
    validation: validatorConfig,
});
  
Vue.directive("vl-modal-toggle", VlUiVueComponents.VlModalToggle);