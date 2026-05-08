import { ComponentPublicInstance, VNode } from "vue";

declare global {
  namespace JSX {
    type Element = VNode;
    type ElementClass = ComponentPublicInstance;
    interface IntrinsicElements {
      [elem: string]: any;
    }
  }

  class FeatureToggles {
    useAcmIdm: boolean;
    useDirectApiCalls: boolean;
    useOverlapCheck: boolean;
    inwinningAllowRequestExtractWhenUploaded: boolean;
    inwinningUploadDryRun: boolean;
  }

  interface Window {
    WR_ENV: WR_ENV_TYPE;
    API_VERSION: string;
    API_ENDPOINT: string;
    API_OLDENDPOINT: string;
    DOWNLOAD_WEGENREGISTER_URL: string;
    featureToggles: FeatureToggles;
  }

  type WR_ENV_TYPE = "development" | "test" | "staging" | "production";
}

window.WR_ENV = window.WR_ENV || "development";
window.API_VERSION = window.API_VERSION || "";
window.API_ENDPOINT = window.API_ENDPOINT || "";
window.API_OLDENDPOINT = window.API_OLDENDPOINT || "";
window.DOWNLOAD_WEGENREGISTER_URL = window.DOWNLOAD_WEGENREGISTER_URL || "";
window.featureToggles = window.featureToggles || {};
