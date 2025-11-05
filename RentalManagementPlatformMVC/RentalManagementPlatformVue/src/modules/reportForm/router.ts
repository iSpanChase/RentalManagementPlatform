import type { RouteRecordRaw } from 'vue-router';

const reportFormRoutes: RouteRecordRaw[] = [
    {
        path: '/reportForm',
        name: 'reportForm',
        component: () => import('@/layouts/MainLayout.vue'),
        children: [
            {
                path: '',
                name: 'ReportForm.IndexView',
                component: () => import('@/modules/reportForm/pages/IndexView.vue'),
                meta: {
                    title: '報表分析',
                    requiresAuth: false,
                },
            }
        ]
    },
    {
        path: '/recommendations',
        component: () => import('@/layouts/MainLayout.vue'),
        children: [
            {
                path: '',
                name: 'Recommendations',
                component: () => import('@/modules/reportForm/pages/RecommendationView.vue'),
                meta: {
                    title: '推薦房間',
                    requiresAuth: false,
                },
            }
        ]
    },
];

export default reportFormRoutes;