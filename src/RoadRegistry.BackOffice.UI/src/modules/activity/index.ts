import Root from "./views/Root.vue";
import Activities from "./views/Activities.vue";

export const ActivityRoutes = [
  {
    path: "/activiteit",
    component: Root,
    children: [
      {
        path: "",
        component: Activities,
        name: "activiteit",
        meta: {
          requiresAuth: true,
        },
      },
    ],
  },
];
