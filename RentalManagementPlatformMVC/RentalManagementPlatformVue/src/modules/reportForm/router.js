// src/modules/reportForm/router.js
export default [
    {
        path: '/reportForm',
        name: 'ReportForm',
        component: () => import('./layouts/ReportFormLayout.vue'), // ReportForm 的父頁面
        meta: { layout: 'reportForm', requiresAuth: true },
        children: [
            { path: '', name: 'ReportForm.IndexView', component: () => import('./pages/IndexView.vue') },
            { path: 'other', name: 'ReportForm.OtherView', component: () => import('./pages/OtherView.vue') }
        ]
    }
]