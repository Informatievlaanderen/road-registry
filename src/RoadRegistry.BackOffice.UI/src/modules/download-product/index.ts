import { DOWNLOAD_WEGENREGISTER_URL } from "@/environment";

export const DownloadProductRoutes = [
  {
    path: "/download-product",
    beforeEnter(to: any, from: any) {
      console.log("Redirecting to download product URL:", DOWNLOAD_WEGENREGISTER_URL);
      window.open(DOWNLOAD_WEGENREGISTER_URL, "_blank");
      console.log("Redirecting to download product URL:", DOWNLOAD_WEGENREGISTER_URL);
      return from.path || "/";
    },
    redirect: "/",
    meta: {},
  },
];
