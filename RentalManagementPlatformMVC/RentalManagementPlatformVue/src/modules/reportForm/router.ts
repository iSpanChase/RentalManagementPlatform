import type { RouteRecordRaw } from 'vue-router';

const reportFormRoutes: RouteRecordRaw[] = [
    {
        path: '/ReportForm',
        name: 'ReportForm',
        component: () => import('@/layouts/MainLayout.vue'),
        children: [
            {
                path: '',
                name: 'ReportForm.IndexView',
                component: () => import('@/modules/ReportForm/pages/IndexView.vue'),
                meta: {
                    title: '報表分析',
                    requiresAuth: true,
                    requiredPerms: ['ReportForm.View'],
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
                component: () => import('@/modules/ReportForm/pages/RecommendationView.vue'),
                meta: {
                    title: '推薦房間',
                    requiresAuth: false,
                },
            }
        ]
    },
];

export default reportFormRoutes;