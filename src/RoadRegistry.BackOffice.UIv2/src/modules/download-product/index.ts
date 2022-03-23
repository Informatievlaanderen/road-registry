import Root from "./views/Root.vue";
import DownloadProduct from "./views/DownloadProduct.vue";

export const DownloadProductRoutes = [
    {
        path: "/download-product",
        component: Root,
        meta: {},
        children: [
            {
                path: "",
                component: DownloadProduct,
                name: "download-product"
              },
        ],
    },
];
