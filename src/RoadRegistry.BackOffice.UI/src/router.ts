import Vue from "vue";
import Router, { RawLocation, Route, RouteConfig } from "vue-router";
import { ActivityRoutes } from "./modules/activity";
import { ExtractRoutes } from "./modules/extract";
import { InformationRoutes } from "./modules/information";
import { DownloadExtractRoutes } from "./modules/download-extract";
import { DownloadProductRoutes } from "./modules/download-product";
import { UploadRoutes } from "./modules/uploads";
import { TransactionZonesRoutes } from "./modules/transaction-zones";
import { AuthRoutes, AuthService, isAuthenticated } from "./auth";

Vue.use(Router);

const routes: RouteConfig[] = [
  {
    path: "/",
    redirect: { name: "activiteit" },
  },
  ...AuthRoutes,
  ...ActivityRoutes,
  ...ExtractRoutes,
  ...InformationRoutes,
  ...DownloadExtractRoutes,
  ...DownloadProductRoutes,
  ...UploadRoutes,
  ...TransactionZonesRoutes,
];

function ensureRouteMetaValue(route: Route, predicate: (meta: any) => boolean) {
  return route.matched.some((m) => predicate(m.meta));
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

const userHasAccessToRoute = (to: Route): boolean => {
  let routeWithAuth = to.matched.find((record) => record.meta.requiresAuth);
  if (!routeWithAuth) {
    return true;
  }

  if (!isAuthenticated.state) {
    return false;
  }

  if (routeWithAuth.meta.requiresContexts?.length > 0) {
    return AuthService.userHasAnyContext(routeWithAuth.meta.requiresContexts);
  }

  return true;
};

router.beforeEach((to, from, next) => {
  if (userHasAccessToRoute(to)) {
    next();
  } else {
    next({
      name: "login",
      query: { redirect: to.fullPath },
    });
  }
});

export default router;
