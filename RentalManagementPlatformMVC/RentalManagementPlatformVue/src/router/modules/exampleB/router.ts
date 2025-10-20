// src/modules/exampleB/router.js
export default [
    {
        path: '/exampleB',
        name: 'ExampleB',
        component: () => import('./layouts/ExampleBLayout.vue'), // exampleA 的父頁面
        meta: { layout: 'exampleB', requiresAuth: true },
        children: [
            { path: '', name: 'ExampleB.IndexView', component: () => import('./pages/IndexView.vue') },
            { path: 'other', name: 'ExampleB.OtherView', component: () => import('./pages/OtherView.vue') }
        ]
    }
]