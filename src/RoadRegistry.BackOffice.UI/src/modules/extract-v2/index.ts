import Root from "./views/Root.vue";
import ExtractDetails from "./views/ExtractDetails.vue";
import Extracts from "./views/Extracts.vue";
import RequestExtract from "./views/RequestExtract.vue";
import RoadRegistry from "@/types/road-registry";

export const ExtractRoutes = [
  {
    path: "/extracten",
    component: Root,
    meta: {},
    children: [
      {
        path: "",
        component: Extracts,
        name: "extracten",
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
  {
    path: "extractaanvraag",
    component: RequestExtract,
    name: "requestExtract",
    meta: {
      requiresAuth: true,
      requiresContexts: [RoadRegistry.UserContext.Editeerder, RoadRegistry.UserContext.Admin],
    },
  },
];
