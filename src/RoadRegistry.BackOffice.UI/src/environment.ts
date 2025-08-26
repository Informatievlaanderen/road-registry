export const WR_ENV = window.WR_ENV || process.env.VUE_APP_WR_ENV;
export const API_VERSION = window.API_VERSION || process.env.VUE_APP_API_VERSION;
export const API_ENDPOINT = window.API_ENDPOINT || process.env.VUE_APP_API_ENDPOINT;
export const API_OLDENDPOINT = window.API_OLDENDPOINT || process.env.VUE_APP_API_OLDENDPOINT;
export const DOWNLOAD_WEGENREGISTER_URL =
  window.DOWNLOAD_WEGENREGISTER_URL || process.env.VUE_APP_DOWNLOAD_WEGENREGISTER_URL;
export const WMS_URL = window.WMS_URL || process.env.VUE_APP_WMS_URL;
export const WMS_LAYER_TRANSACTIONZONES =
  window.WMS_LAYER_TRANSACTIONZONES || process.env.VUE_APP_WMS_LAYER_TRANSACTIONZONES;
export const WMS_LAYER_OVERLAPPINGTRANSACTIONZONES =
  window.WMS_LAYER_OVERLAPPINGTRANSACTIONZONES || process.env.VUE_APP_WMS_LAYER_OVERLAPPINGTRANSACTIONZONES;

export const featureToggles = {
  useAcmIdm: `${window.featureToggles?.useAcmIdm ?? process.env.VUE_APP_FEATURETOGGLES_USEACMIDM}` === "true",
  useDirectApiCalls:
    `${window.featureToggles?.useDirectApiCalls ?? process.env.VUE_APP_FEATURETOGGLES_USEDIRECTAPICALLS}` === "true",
  useTransactionZonesTab:
    `${window.featureToggles?.useTransactionZonesTab ?? process.env.VUE_APP_FEATURETOGGLES_USETRANSACTIONZONESTAB}` ===
    "true",
  useOverlapCheck:
    `${window.featureToggles?.useOverlapCheck ?? process.env.VUE_APP_FEATURETOGGLES_USEOVERLAPCHECK}` === "true",
  usePresignedEndpoints:
    `${window.featureToggles?.usePresignedEndpoints ?? process.env.VUE_APP_FEATURETOGGLES_USEPRESIGNEDENDPOINTS}` ===
    "true",
  useExtractsV2:
    `${window.featureToggles?.useExtractsV2 ?? process.env.VUE_APP_FEATURETOGGLES_USEEXTRACTSV2}` === "true",
};
