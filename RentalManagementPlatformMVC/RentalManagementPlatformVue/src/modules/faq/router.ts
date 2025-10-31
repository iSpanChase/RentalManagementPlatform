import type { RouteRecordRaw } from 'vue-router'

export default [
  {
    path: '/support',
    component: () => import('@/layouts/MainLayout.vue'), // 跟既有主版一致
    children: [
      { path: 'customer', name: 'support-customer', component: () => import('@/views/Customer.vue') },
      { path: 'agent', name: 'support-agent', component: () => import('@/views/Agent.vue') },
      { path: 'faq',      name: 'support-faq',      component: () => import('@/views/FaqPage.vue') },
    ]
  }
] as RouteRecordRaw[]