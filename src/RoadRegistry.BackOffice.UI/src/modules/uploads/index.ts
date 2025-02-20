import Root from "./views/Root.vue";
import Uploads from "./views/Uploads.vue";
import UploadStatus from "./views/UploadStatus.vue";
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
    {
        path: "/upload",
        component: Root,
        meta: {},
        children: [
            {
                path: ":ticketId",
                component: UploadStatus,
                name: "uploadStatus",
                meta: {
                    requiresAuth: true,
                    requiresContexts: [RoadRegistry.UserContext.Editeerder, RoadRegistry.UserContext.Admin]
                }
            }
        ],
    },
];
