import Root from "./views/Root.vue";
import Activities from "./views/Activities.vue";

export const ActivityRoutes = [
    {
        path: "/activiteit",
        component: Root,
        meta: {},
        children: [
            {
                path: "",
                component: Activities,
                name: "activiteit"
              },
        ],
    },
];
