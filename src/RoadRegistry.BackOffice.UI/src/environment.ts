export const API_VERSION = window.API_VERSION || process.env.VUE_APP_API_VERSION;
export const API_ENDPOINT = window.API_ENDPOINT || process.env.VUE_APP_API_ENDPOINT;
export const API_OLDENDPOINT = window.API_OLDENDPOINT || process.env.VUE_APP_API_OLDENDPOINT;
export const featureToggles = {
    useFeatureCompare: `${(window.featureToggles?.useFeatureCompare ?? process.env.VUE_APP_FEATURETOGGLES_USEFEATURECOMPARE)}` === 'true'
};
