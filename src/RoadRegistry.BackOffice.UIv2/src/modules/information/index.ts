import Root from "./views/Root.vue";
import Information from "./views/Information.vue";

export const InformationRoutes = [
    {
        path: "/informatie",
        component: Root,
        meta: {},
        children: [
            {
                path: "",
                component: Information,
                name: "informatie"
              },
        ],
    },
];
