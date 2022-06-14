import Root from "./views/Root.vue";
import Home from "./views/Home.vue";
import Login from "./views/Login.vue";
import Logout from "./views/Logout.vue";
import { isAuthenticated } from "../services/auth-service";

export const AuthRoutes = [
    {
        path: "/login",
        component: Login,
        name: "login",
        meta: {
            requiresAuth: false
        }
    },
    {
        path: "/logout",
        component: Logout,
        name: "logout",
        meta: {
            requiresAuth: false
        }
    }
]

export { AuthService, isAuthenticated } from '../services/auth-service'
