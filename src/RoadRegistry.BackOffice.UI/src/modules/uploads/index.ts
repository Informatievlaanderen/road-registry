import Root from "./views/Root.vue";
import Uploads from "./views/Uploads.vue";
import RoadRegistry from '@/types/road-registry';

export const UploadRoutes = [
    {
        path: "/uploads",
        component: Root,
        meta: {},
        children: [
            {
                path: "",
                component: Uploads,
                name: "uploads",
                meta: {
                    requiresAuth: true,
                    requiresContexts: [RoadRegistry.UserContext.Editeerder, RoadRegistry.UserContext.Admin]
                }
            }
        ],
    },
];
