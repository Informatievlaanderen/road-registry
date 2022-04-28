import Vue, { VNode } from 'vue'

declare global {
  namespace JSX {
    interface Element extends VNode {}
    interface ElementClass extends Vue {}
    interface IntrinsicElements {
      [elem: string]: any
    }
  }

  interface Window { 
    API_KEY: string;
    API_VERSION: string;
    API_ENDPOINT: string;
    API_OLDENDPOINT:string;
  }
}

window.API_KEY = window.API_KEY || "";
window.API_VERSION = window.API_VERSION || "";
window.API_ENDPOINT = window.API_ENDPOINT || "";
window.API_OLDENDPOINT = window.API_OLDENDPOINT || "";
