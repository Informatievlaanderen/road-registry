import Root from "./views/Root.vue";
import ExtractDetails from "./views/ExtractDetails.vue";
import RoadRegistry from '@/types/road-registry';

export const ExtractRoutes = [
    {
        path: "/extract",
        component: Root,
        meta: {},
        children: [
            {
                path: ":downloadId",
                component: ExtractDetails,
                name: "extractDetails",
                meta: {
                    requiresAuth: true,
                    requiresContexts: [RoadRegistry.UserContext.Editeerder, RoadRegistry.UserContext.Admin]
                }
            }
        ],
    },
];
