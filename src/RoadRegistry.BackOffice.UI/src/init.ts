export declare interface  WegenRegisterApiInfo {
    version: string;
    apiEndpoint: string;
    apiOldEndpoint: string;
    apiKey: string;
}

declare global {
    interface Window { 
        wegenregister: WegenRegisterApiInfo;
    }
}

function applyEnvironmentVariables() {
    const { API_VERSION, API_ENDPOINT, API_OLDENDPOINT, API_KEY } = process.env;
    const wegenregisterApi: WegenRegisterApiInfo = {
        version: API_VERSION || "",
        apiEndpoint: API_ENDPOINT || "",
        apiOldEndpoint: API_OLDENDPOINT || "",
        apiKey: API_KEY || "",
    };
    window.wegenregister = wegenregisterApi;
}

function init() {
    applyEnvironmentVariables();
};

init();

export default global