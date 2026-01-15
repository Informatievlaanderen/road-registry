import Root from "./views/Root.vue";
import ExtractDetails from "./views/ExtractDetailsWrapper.vue";
import Extracts from "./views/Extracts.vue";
import RequestExtract from "./views/RequestExtract.vue";
import RoadRegistry from "@/types/road-registry";

export const InwinningRoutes = [
  {
    path: "/inwinning",
    component: Root,
    meta: {},
    children: [
      {
        path: "",
        component: Extracts,
        name: "inwinning",
        meta: {
          requiresAuth: true,
          requiresContexts: [RoadRegistry.UserContext.Editeerder, RoadRegistry.UserContext.Admin],
          inwinning: true,
        },
      },
      {
        path: ":downloadId",
        component: ExtractDetails,
        name: "inwinningExtractDetails",
        meta: {
          requiresAuth: true,
          requiresContexts: [RoadRegistry.UserContext.Editeerder, RoadRegistry.UserContext.Admin],
          inwinning: true,
        },
      },
    ],
  },
  {
    path: "/extractaanvraag",
    component: RequestExtract,
    name: "requestInwinningExtract",
    meta: {
      requiresAuth: true,
      requiresContexts: [RoadRegistry.UserContext.Editeerder, RoadRegistry.UserContext.Admin],
      inwinning: true,
    },
  },
];
