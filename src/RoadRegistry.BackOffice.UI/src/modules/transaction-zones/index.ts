import Root from "./views/Root.vue";
import TransactionZones from "./views/TransactionZones.vue";

export const TransactionZonesRoutes = [
    {
        path: "/transaction-zones",
        component: Root,
        meta: {},
        children: [
            {
                path: "",
                component: TransactionZones,
                name: "transaction-zones",
                meta: {
                    requiresAuth: true
                }
            },
        ],
    },
];
