import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import exampleARouter from './modules/exampleA/router'
import exampleBRouter from './modules/exampleB/router'
import ReportFormRouter from './modules/ReportForm/router'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    ...exampleARouter,
    ...exampleBRouter,
    ...ReportFormRouter,
    {
      path: '/',
      name: 'home',
      component: HomeView,
    },
    {
      path: '/about',
      name: 'about',
      // route level code-splitting
      // this generates a separate chunk (About.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import('../views/AboutView.vue'),
    },
  ],
})

export default router
