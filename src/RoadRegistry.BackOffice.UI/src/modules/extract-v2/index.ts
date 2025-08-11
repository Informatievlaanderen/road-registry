import Root from "./views/Root.vue";
import ExtractDetails from "./views/ExtractDetails.vue";
import ExtractList from "./views/ExtractList.vue";
import RoadRegistry from "@/types/road-registry";

export const ExtractRoutes = [
  {
    path: "/extracten",
    component: Root,
    meta: {
    },
    children: [
      {
        path: "",
        component: ExtractList,
        meta: {
          requiresAuth: true,
          requiresContexts: [RoadRegistry.UserContext.Editeerder, RoadRegistry.UserContext.Admin],
        },
      },
      {
        path: ":downloadId",
        component: ExtractDetails,
        name: "extractDetailsV2",
        meta: {
          requiresAuth: true,
          requiresContexts: [RoadRegistry.UserContext.Editeerder, RoadRegistry.UserContext.Admin],
        },
      },
    ],
  },
];
