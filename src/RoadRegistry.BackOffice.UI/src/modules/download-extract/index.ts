import Root from "./views/Root.vue";
import DownloadExtract from "./views/DownloadExtract.vue";

export const DownloadExtractRoutes = [
    {
        path: "/download-extract",
        component: Root,
        meta: {},
        children: [
            {
                path: "",
                component: DownloadExtract,
                name: "download-extract"
              },
        ],
    },
];
