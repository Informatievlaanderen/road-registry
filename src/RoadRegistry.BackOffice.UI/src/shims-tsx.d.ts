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
    useAcmIdm: boolean;
    useDirectApiCalls: boolean;
    useTransactionZonesTab: boolean;
    usePresignedUpload: boolean;
  }

  interface Window { 
    WR_ENV: WR_ENV_TYPE;
    API_VERSION: string;
    API_ENDPOINT: string;
    API_OLDENDPOINT: string;
    featureToggles: FeatureToggles;
  }

  type WR_ENV_TYPE = "development" | "test" | "staging" | "production";
}

window.WR_ENV = window.WR_ENV || "development";
window.API_VERSION = window.API_VERSION || "";
window.API_ENDPOINT = window.API_ENDPOINT || "";
window.API_OLDENDPOINT = window.API_OLDENDPOINT || "";
window.featureToggles = window.featureToggles || {};
