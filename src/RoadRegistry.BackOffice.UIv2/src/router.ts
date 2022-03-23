import Vue from "vue";
import Router, { RawLocation, Route, RouteConfig } from "vue-router";
import Home from "./views/Home.vue";
import { ActivityRoutes } from "./modules/activity/";
// import { InformationRoutes } from "./modules/information/";
// import { DownloadExtract } from "./modules/download-extract/views/DownloadExtract.vue";
// import { DownloadProduct } from "./modules/download-product/views/DownloadProduct.vue";
// import { Upload } from "./modules/upload/views/Upload.vue";


const routes: RouteConfig[] = [
    {
        path: "/",
        component: Home
    },
    ...ActivityRoutes,
    // ...InformationRoutes,
    // {
    //     path: "/download-extract",
    //     name: "download-extract",
    //     component: DownloadExtract
    // },
    // {
    //     path: "/download-product",
    //     name: "download-product",
    //     component: DownloadProduct
    // },
    // {
    //     path: "/opladen",
    //     name: "opladen",
    //     component: Upload
    // },
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

// router.beforeEach((to, from, next) => {
//     Vue.prototype.$prevRoute = from;
//     //TODO check if authenticated
//     // IF not redirect to decent login page instead
//     return next();
// });

export default router;