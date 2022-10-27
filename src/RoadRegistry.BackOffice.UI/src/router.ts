import Vue from "vue";
import Router, { RawLocation, Route, RouteConfig } from "vue-router";
import { ActivityRoutes } from "./modules/activity";
import { InformationRoutes } from "./modules/information";
import { DownloadExtractRoutes } from "./modules/download-extract";
import { DownloadProductRoutes } from "./modules/download-product";
import { UploadRoutes } from "./modules/uploads";
import { AuthRoutes, AuthService, isAuthenticated } from "./auth";

Vue.use(Router);

const routes: RouteConfig[] = [
    {
        path: "/",
        redirect: { name: "activiteit" }
    },
    ...AuthRoutes,
    ...ActivityRoutes,
    ...InformationRoutes,
    ...DownloadExtractRoutes,
    ...DownloadProductRoutes,
    ...UploadRoutes
];

function ensureRouteMetaValue(route: Route, predicate: (meta: any) => boolean) {
    return route.matched.some(m => predicate(m.meta));
}

routes.push({
    path: "*",
    redirect: "/",
});

export const router = new Router({
    mode: "history",
    base: process.env.BASE_URL,
    scrollBehavior(to, from, savedPosition) {
        if (savedPosition) {
            return savedPosition;
        } else {
            return { x: 0, y: 0 };
        }
    },
    routes,
});
router.beforeEach((to, from, next) => {
    if (to.matched.some(record => record.meta.requiresAuth)) {
        if (!isAuthenticated.state) {
            next({
                path: '/login',
                query: { redirect: to.fullPath }
            })
        } else {
            next()
        }
    } else {
        next()
    }
})

export default router;