import Vue, { VNode } from 'vue'

declare global {
  namespace JSX {
    interface Element extends VNode {}
    interface ElementClass extends Vue {}
    interface IntrinsicElements {
      [elem: string]: any
    }
  }

  class FeatureToggles {
    useFeatureCompare: boolean
  }

  interface Window { 
    API_VERSION: string;
    API_ENDPOINT: string;
    API_OLDENDPOINT: string;
    featureToggles: FeatureToggles;
  }
}

window.API_VERSION = window.API_VERSION || "";
window.API_ENDPOINT = window.API_ENDPOINT || "";
window.API_OLDENDPOINT = window.API_OLDENDPOINT || "";
window.featureToggles = window.featureToggles || {};
