import Root from "./views/Root.vue";
import DownloadExtract from "./views/DownloadExtract.vue";
import RoadRegistry from "@/types/road-registry";

export const DownloadExtractRoutes = [
  {
    path: "/download-extract",
    component: Root,
    meta: {},
    children: [
      {
        path: "",
        component: DownloadExtract,
        name: "download-extract",
        meta: {
          requiresAuth: true,
          requiresContexts: [RoadRegistry.UserContext.Editeerder, RoadRegistry.UserContext.Admin],
        },
      },
    ],
  },
];
