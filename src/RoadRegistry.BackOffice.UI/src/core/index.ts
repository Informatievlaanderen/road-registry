import { App } from "vue";
import { registerComponents } from "./components";

export const registerCore = (app: App): void => {
  registerComponents(app);
};
