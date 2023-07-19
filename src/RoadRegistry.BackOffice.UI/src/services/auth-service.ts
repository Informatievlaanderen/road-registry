import router from "@/router";
import Vue from "vue";
import PublicApi from "./public-api";
import UserTokenResult from "@/auth/userTokenResult";
import OidcClient from "@/auth/roadRegistryOidcClient";

const WR_AUTH_APIKEY = "RoadRegistry.BackOffice.UI.Authentication.ApiKey";
const WR_AUTH_OIDC_VERIFIER = "RoadRegistry.BackOffice.UI.Authentication.OidcVerifier";
const WR_AUTH_OIDC_TOKEN = "RoadRegistry.BackOffice.UI.Authentication.OidcToken";
const WR_AUTH_REDIRECT_URL = "RoadRegistry.BackOffice.UI.Authentication.RedirectUrl";

export const isAuthenticated = Vue.observable({
  state: false,
});

export const user = Vue.observable({
  state: UserTokenResult.empty(),
});

export const AuthService = {
  reset() {
    sessionStorage.removeItem(WR_AUTH_APIKEY);
    sessionStorage.removeItem(WR_AUTH_OIDC_VERIFIER);
    sessionStorage.removeItem(WR_AUTH_OIDC_TOKEN);
    sessionStorage.removeItem(WR_AUTH_REDIRECT_URL);

    isAuthenticated.state = false;
    user.state = UserTokenResult.empty();
  },
  getApiKey(): string | null {
    return sessionStorage.getItem(WR_AUTH_APIKEY);
  },
  getToken(): string | null {
    return sessionStorage.getItem(WR_AUTH_OIDC_TOKEN);
  },
  async initialize(): Promise<void> {
    await OidcClient.initialize();
    await this.checkAuthentication();
  },
  async login(key: string, url: string): Promise<boolean> {
    sessionStorage.setItem(WR_AUTH_APIKEY, key);
    const isAuthenticated = await this.checkAuthentication();
    if (isAuthenticated) {
      router.push(url || "/");
      return true;
    }

    sessionStorage.removeItem(WR_AUTH_APIKEY);
    return false;
  },
  async loginAcmIdm(url: string): Promise<void> {
    await OidcClient.initialize();

    var request = await OidcClient.instance.createSigninRequest({
      state: {
        bar: 15,
      },
    });

    sessionStorage.setItem(WR_AUTH_OIDC_VERIFIER, request.state.code_verifier as string);
    sessionStorage.setItem(WR_AUTH_REDIRECT_URL, url);
    window.location.href = request.url;
  },
  async completeAcmIdmLogin(code: string): Promise<void> {
    const verifier = sessionStorage.getItem(WR_AUTH_OIDC_VERIFIER) as string;
    const redirectUri = OidcClient.instance.settings.redirect_uri;
    const token = await PublicApi.Security.getExchangeCode(code, verifier, redirectUri);
    sessionStorage.setItem(WR_AUTH_OIDC_TOKEN, token);

    try {
      await this.loadUserFromToken(token);

      const isAuthenticated = await this.checkAuthentication();
      if (!isAuthenticated) {
        throw new Error("Er is een fout gebeurd bij het inloggen.");
      }
      
      const url = sessionStorage.getItem(WR_AUTH_REDIRECT_URL) as string;
      router.push(url || "/");
    } catch (err) {
      this.reset();
      throw err;
    }
  },
  async logout(): Promise<void> {
    if (sessionStorage.getItem(WR_AUTH_APIKEY)) {
      this.reset();
      router.push({ name: "login" });
    } else if (sessionStorage.getItem(WR_AUTH_OIDC_VERIFIER)) {
      this.reset();
      let request = await OidcClient.instance.createSignoutRequest({
        state: {
          bar: 15,
        },
      });

      window.location.href = request.url;
    }
  },
  async checkAuthentication(): Promise<boolean> {
    isAuthenticated.state = await PublicApi.Security.userIsAuthenticated();
    return isAuthenticated.state;
  },
  async loadUserFromToken(token: string): Promise<void> {
    try {
      const userToken = UserTokenResult.fromJwt(token);
      if (userToken.isExpired) {
        throw new Error("Token session expired");
      }

      user.state = userToken;
    } catch (e) {
      console.error("Could not decode provided jwt", e);
      throw e;
    }
  },
};
