import Login from "./views/Login.vue";
import Logout from "./views/Logout.vue";
import AuthCallback from "./views/AuthCallback.vue";

export const AuthRoutes = [
  {
    path: "/login",
    component: Login,
    name: "login",
    meta: {
      requiresAuth: false,
    },
    props: (route: any) => ({ error: route.query.error }),
  },
  {
    path: "/logout",
    component: Logout,
    name: "logout",
    meta: {
      requiresAuth: false,
    },
  },
  {
    path: "/oic",
    component: AuthCallback,
    name: "openidconnect",
    meta: {
      requiresAuth: false,
    },
    props: (route: any) => ({ code: route.query.code }),
  },
];

export { AuthService, isAuthenticated, user } from "../services/auth-service";
