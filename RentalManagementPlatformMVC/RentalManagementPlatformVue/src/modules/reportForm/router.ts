import type { RouteRecordRaw } from 'vue-router';

const reportFormRoutes: RouteRecordRaw[] = [
    {
        path: '/ReportForm',
        name: 'ReportForm',
        component: () => import('./layouts/ReportFormLayout.vue'),
        meta: { layout: 'ReportForm', requiresAuth: true },
        children: [
            {
                path: '', name: 'ReportForm.IndexView', component: () => import(
                    './pages/IndexView.vue')
            },
        ]
    },
    {
        path: '/recommendations',
        component: () => import('@/layouts/MainLayout.vue'),
        children: [
            {
                path: '',
                name: 'Recommendations',
                component: () => import('./pages/RecommendationView.vue'),
                meta: {
                    title: '推薦房間',
                    requiresAuth: false,
                },
            }
        ]
    },
];

export default reportFormRoutes;