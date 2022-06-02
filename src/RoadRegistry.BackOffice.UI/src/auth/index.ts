import Root from "./views/Root.vue";
import Home from "./views/Home.vue";
import Login from "./views/Login.vue";
import Logout from "./views/Logout.vue";

export const AuthRoutes = [
    {
        path: "/auth",
        component: Root,
        meta: {},
        children: [
            {
                path: "",
                component: Home
            },
            {
                path: "login",
                component: Login,
                name: "login"
            },
            {
                path: "logout",
                component: Logout,
                name: "logout"
            }
        ],
    },
];

export { AuthService } from './AuthService'
