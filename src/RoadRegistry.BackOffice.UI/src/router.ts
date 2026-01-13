import { createRouter, createWebHistory, RouteLocationNormalized, RouteLocationRaw, RouteRecordRaw } from "vue-router";
import * as environment from "@/environment";
import { ActivityRoutes } from "./modules/activity";
import { ExtractRoutes } from "./modules/extract";
import { ExtractRoutes as ExtractRoutesV2 } from "./modules/extract-v2";
import { InwinningRoutes } from "./modules/inwinning";
import { InformationRoutes } from "./modules/information";
import { DownloadExtractRoutes } from "./modules/download-extract";
import { UploadRoutes } from "./modules/uploads";
import { TransactionZonesRoutes } from "./modules/transaction-zones";
import { AuthRoutes, AuthService, isAuthenticated, user } from "./auth";

const routes: RouteRecordRaw[] = [
  {
    path: "/",
    redirect: (to: any, from: any) => {
      return {
        name: user.state.isInwinner
          ? "inwinning"
          : environment.featureToggles.useExtractsV2
            ? "extracten"
            : "activiteit",
      };
    },
  },
  ...(AuthRoutes as RouteRecordRaw[]),
  ...(ActivityRoutes as RouteRecordRaw[]),
  ...(ExtractRoutes as RouteRecordRaw[]),
  ...(ExtractRoutesV2 as RouteRecordRaw[]),
  ...(InwinningRoutes as RouteRecordRaw[]),
  ...(InformationRoutes as RouteRecordRaw[]),
  ...(DownloadExtractRoutes as RouteRecordRaw[]),
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

const findRouteToRedirectTo = (to: RouteLocationNormalized): RouteLocationRaw | undefined => {
  const routeWithAuth = to.matched.find((record) => record.meta.requiresAuth);
  if (!routeWithAuth) {
    return undefined;
  }

  if (!isAuthenticated.state) {
    return {
      name: "login",
      query: { redirect: to.fullPath },
    };
  }

  const meta = routeWithAuth.meta as {
    requiresContexts?: string[];
    inwinning: boolean;
  };

  if (user.state.isInwinner && !meta.inwinning) {
      return {
        name: "inwinning",
      };
    }

  if (meta.requiresContexts && meta.requiresContexts.length > 0) {
    if (!AuthService.userHasAnyContext(meta.requiresContexts)) {
      return {
        name: "login",
        query: { redirect: to.fullPath },
      };
    }
  }

  return undefined;
};

router.beforeEach((to, from, next) => {
  let redirectTo = findRouteToRedirectTo(to);
  if (redirectTo) {
    next(redirectTo);
  } else {
    next();
  }
});

export default router;
