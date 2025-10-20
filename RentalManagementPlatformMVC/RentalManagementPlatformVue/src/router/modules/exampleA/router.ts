// src/modules/exampleA/router.js
export default [
    {
        path: '/exampleA',
        name: 'ExampleA',
        component: () => import('./layouts/ExampleALayout.vue'), // exampleA 的父頁面
        meta: { layout: 'exampleA', requiresAuth: true },
        children: [
            { path: '', name: 'ExampleA.IndexView', component: () => import('./pages/IndexView.vue') },
            { path: 'other', name: 'ExampleA.OtherView', component: () => import('./pages/OtherView.vue') }
        ]
    }
]