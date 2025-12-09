import { App } from "vue";
import Header from "./Header.vue";
import H2 from "./H2.vue";
import H3 from "./H3.vue";
import Footer from "./Footer.vue";
const VlUiVueComponents = require("@govflanders/vl-ui-vue-components");

// configuration of the built-in validator
const validatorConfig = {
  inject: true,
  locale: "nl",
};

export const registerComponents = (app: App): void => {
  app.component("wr-header", Header);
  app.component("wr-footer", Footer);
  app.component("wr-h2", H2);
  app.component("wr-h3", H3);

  // install the component library with config
  app.use(VlUiVueComponents, {
    validation: validatorConfig,
  });

  app.directive("vl-modal-toggle", VlUiVueComponents.VlModalToggle);
};
