import router from "@/router";
import Vue from "vue";
import PublicApi from "./public-api";
import BackOfficeApi from "./backoffice-api";

const WR_AUTH_APIKEY = "RoadRegistry.BackOffice.UI.Authentication.ApiKey";
export const isAuthenticated = Vue.observable({
  state: false,
});

export const AuthService = {
  getApiKey(): string | null {
    return sessionStorage.getItem(WR_AUTH_APIKEY);
  },
  async login(key: string, url: string): Promise<boolean> {
    sessionStorage.setItem(WR_AUTH_APIKEY, key);
    await this.checkAuthentication();

    if (isAuthenticated.state) {
      router.push(url);
      return true;
    }

    sessionStorage.removeItem(WR_AUTH_APIKEY);
    return false;
  },
  async logout(): Promise<void> {
    sessionStorage.removeItem(WR_AUTH_APIKEY);
    router.push({
      name: "login",
      query: { redirect: "/activiteit" },
    });
    await this.checkAuthentication();
  },
  async checkAuthentication(): Promise<void> {
    try {
      if (process.env.NODE_ENV === "production") {
        await PublicApi.Information.getInformation();
      } else {
        await BackOfficeApi.Information.getInformation();
      }
      isAuthenticated.state = true;
    } catch (err) {
      isAuthenticated.state = false;
    }
  },
};
