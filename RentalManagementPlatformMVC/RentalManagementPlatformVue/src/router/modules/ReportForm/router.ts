// src/modules/ReportForm/router.js
export default [
    {
        path: '/ReportForm',
        name: 'ReportForm',
        component: () => import('./layouts/ReportFormLayout.vue'), // ReportForm 的父頁面
        meta: { layout: 'ReportForm', requiresAuth: true },
        children: [
            { path: '', name: 'ReportForm.IndexView', component: () => import('./pages/IndexView.vue') },
            { path: 'other', name: 'ReportForm.OtherView', component: () => import('./pages/OtherView.vue') }
        ]
    }
]