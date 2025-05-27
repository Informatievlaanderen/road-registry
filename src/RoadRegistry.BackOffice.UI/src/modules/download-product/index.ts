import { DOWNLOAD_WEGENREGISTER_URL } from "@/environment";

export const DownloadProductRoutes = [
  {
    path: "/download-product",
    meta: {},
    beforeEnter() {
      window.open(DOWNLOAD_WEGENREGISTER_URL, "_blank");
    },
  },
];
