import router from "@/router"
import Vue from "vue"

const WR_AUTH_APIKEY = "RoadRegistry.BackOffice.UI.Authentication.ApiKey"
export const isAuthenticated = Vue.observable({
    state: false
})

export const AuthService = {
    getApiKey() : string | null {
        return sessionStorage.getItem(WR_AUTH_APIKEY)
    },
    login(key : string, url : string) : void {
        sessionStorage.setItem(WR_AUTH_APIKEY, key)
        router.push(url)
        this.checkAuthentication()
    },
    logout() : void {
        sessionStorage.removeItem(WR_AUTH_APIKEY)
        router.push("/login?redirect=%2Factiviteit")
        this.checkAuthentication()
    },
    checkAuthentication(): void {
        console.trace(isAuthenticated)
        isAuthenticated.state = this.getApiKey() !== null
        console.trace(isAuthenticated)
    }
}