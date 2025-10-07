import { createRouter, createWebHistory } from 'vue-router'
import HomeView from '../views/HomeView.vue'
import AboutView from '../views/AboutView.vue'
import reportFormRoutes from '@/modules/reportForm/router'

const routes = [
    ...reportFormRoutes,
    { path: '/', name: 'home', component: HomeView },
    { path: '/about', name: 'about', component: AboutView },
]

const router = createRouter({
    history: createWebHistory(), // 使用 HTML5 History 模式
    routes,
})

export default router


