import Root from "./views/Root.vue";
import Uploads from "./views/Uploads.vue";

export const UploadRoutes = [
    {
        path: "/uploads",
        component: Root,
        meta: {},
        children: [
            {
                path: "",
                component: Uploads,
                name: "uploads"
              },
        ],
    },
];
