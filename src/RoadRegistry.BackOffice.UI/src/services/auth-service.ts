import router from "@/router";
import Vue from "vue";
import PublicApi from "./public-api";
import UserTokenResult from "@/auth/userTokenResult";
import OidcClient from "@/auth/roadRegistryOidcClient";
import RoadRegistry from "@/types/road-registry";

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
  async loginApiKey(key: string, url: string): Promise<boolean> {
    sessionStorage.setItem(WR_AUTH_APIKEY, key);
    const isAuthenticated = await this.checkAuthentication();
    if (isAuthenticated) {
      const userInfo = await this.getCurrentUser();
      this.loadUserFromUserInfo(userInfo);
      
      router.push(url || "/");
      return true;
    }

    sessionStorage.removeItem(WR_AUTH_APIKEY);
    return false;
  },
  async loginAcmIdm(url: string): Promise<void> {
    await OidcClient.initialize();

    var request = await OidcClient.instance.createSigninRequest({});

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
      this.loadUserFromToken(token);

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
    if (this.getToken()) {
      await OidcClient.instance.createSignoutRequest();
    }
    this.reset();
    router.push({ name: "login" });
  },
  async checkAuthentication(): Promise<boolean> {
    isAuthenticated.state = (await this.getCurrentUser()).claims.length > 0;
    return isAuthenticated.state;
  },
  async getCurrentUser(): Promise<RoadRegistry.UserInfo> {
    try {
      return await PublicApi.Security.getAuthenticatedUser();
    } catch (err) {
      return { claims: [] };
    }
  },
  loadUserFromToken(token: string): void {
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
  loadUserFromUserInfo(userInfo: RoadRegistry.UserInfo): void {
    const userToken = UserTokenResult.fromUserInfo(userInfo);
    if (userToken.isExpired) {
      throw new Error("Token session expired");
    }

    user.state = userToken;
  },
  userHasAnyContext(contexts: string[]): boolean {
    if (!contexts || contexts.length === 0 || this.getApiKey()) {
      return true;
    }

    let hasAccess = false;

    contexts.forEach((context: string) => {
      if (~user.state.contexts.indexOf(context)) {
        hasAccess = true;
      }
    });

    return hasAccess;
  },
};
