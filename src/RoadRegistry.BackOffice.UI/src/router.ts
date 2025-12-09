import { createRouter, createWebHistory, RouteLocationNormalized, RouteRecordRaw } from "vue-router";
import * as environment from "@/environment";
import { ActivityRoutes } from "./modules/activity";
import { ExtractRoutes } from "./modules/extract";
import { ExtractRoutes as ExtractRoutesV2 } from "./modules/extract-v2";
import { InformationRoutes } from "./modules/information";
import { DownloadExtractRoutes } from "./modules/download-extract";
import { DownloadProductRoutes } from "./modules/download-product";
import { UploadRoutes } from "./modules/uploads";
import { TransactionZonesRoutes } from "./modules/transaction-zones";
import { AuthRoutes, AuthService, isAuthenticated } from "./auth";

const defaultRedirect = environment.featureToggles.useExtractsV2 ? "extracten" : "activiteit";

const routes: RouteRecordRaw[] = [
  {
    path: "/",
    redirect: { name: defaultRedirect },
  },
  ...(AuthRoutes as RouteRecordRaw[]),
  ...(ActivityRoutes as RouteRecordRaw[]),
  ...(ExtractRoutes as RouteRecordRaw[]),
  ...(ExtractRoutesV2 as RouteRecordRaw[]),
  ...(InformationRoutes as RouteRecordRaw[]),
  ...(DownloadExtractRoutes as RouteRecordRaw[]),
  //...(DownloadProductRoutes as RouteRecordRaw[]),
  ...(UploadRoutes as RouteRecordRaw[]),
  ...(TransactionZonesRoutes as RouteRecordRaw[]),
];

routes.push({
  path: "/:pathMatch(.*)*",
  redirect: "/",
});

export const router = createRouter({
  history: createWebHistory(process.env.BASE_URL),
  scrollBehavior(to, from, savedPosition) {
    if (savedPosition) {
      return savedPosition;
    } else {
      return { top: 0, left: 0 };
    }
  },
  routes,
});

const userHasAccessToRoute = (to: RouteLocationNormalized): boolean => {
  const routeWithAuth = to.matched.find((record) => record.meta.requiresAuth);
  if (!routeWithAuth) {
    return true;
  }

  if (!isAuthenticated.state) {
    return false;
  }

  const meta = routeWithAuth.meta as {
    requiresContexts?: string[];
  };

  if (meta.requiresContexts && meta.requiresContexts.length > 0) {
    return AuthService.userHasAnyContext(meta.requiresContexts);
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
