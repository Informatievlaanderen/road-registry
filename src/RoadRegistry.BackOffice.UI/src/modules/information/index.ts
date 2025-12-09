import Root from "./views/Root.vue";
import Information from "./views/Information.vue";

export const InformationRoutes = [
  {
    path: "/informatie",
    component: Root,
    children: [
      {
        path: "",
        component: Information,
        name: "informatie",
        meta: {
          requiresAuth: true,
        },
      },
    ],
  },
];
