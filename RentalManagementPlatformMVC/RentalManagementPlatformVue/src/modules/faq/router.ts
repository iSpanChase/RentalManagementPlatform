import type { RouteRecordRaw } from 'vue-router'

export default [
  {
    path: '/support',
    component: () => import('@/layouts/MainLayout.vue'), // 跟既有主版一致
    children: [
      { path: 'customer', name: 'support-customer', component: () => import('@/views/Customer.vue'), meta: { role: 'user' } },
      { path: 'agent', name: 'support-agent', component: () => import('@/views/Agent.vue'), meat: { role: 'agent' } },
      { path: 'faq', name: 'support-faq', component: () => import('@/views/FaqPage.vue') },
      // （可選）/support 進來自動分流
      {
        path: '', name: 'support',
        redirect: (to) => ({ name: 'support-customer' })        // 先給預設，真正分流交給 beforeEach
      }
    ]
  }
] as RouteRecordRaw[]