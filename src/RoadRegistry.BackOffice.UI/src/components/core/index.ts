import { App } from "vue";
import Header from "./header/Header.vue";
import Footer from "./footer/Footer.vue";


export const applyGlobalComponents = (app: App<Element>): App<Element> => app
    .component("dv-header",Header)
    .component("dv-footer", Footer);

export default applyGlobalComponents;