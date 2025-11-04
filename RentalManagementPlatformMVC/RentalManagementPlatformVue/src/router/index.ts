import { createRouter, createWebHistory } from 'vue-router';
import MainLayout from '@/layouts/MainLayout.vue';
import HomeView from '../views/HomeView.vue';
import exampleARouter from './modules/exampleA/router';
import exampleBRouter from './modules/exampleB/router';
import ReportFormRouter from './modules/ReportForm/router';
import bookingRoutes from '@/modules/booking/router';
import CouponCenterView from '../views/CouponCenterView.vue';
import supportRoutes from '@/modules/faq/router'


const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    // 1. 主頁面：用 MainLayout 包住
    {
      path: '/',
      component: MainLayout,
      children: [
        {
          path: '',
          name: 'home',
          component: () => import('../views/SearchView.vue'),
        },
        {
          path: 'about',
          name: 'about',
          component: () => import('../views/AboutView.vue'),
        },
        {
          path: 'rooms/:id',
          name: 'room-detail',
          component: () => import('../views/RoomDetailView.vue'),
        },
        {
          path: '/search',
          name: 'search',
          component: () => import('../views/SearchView.vue'),
        },
        {
          path: '/original-home',
          name: 'original-home',
          component: HomeView,
        },
        {
          path: '/hosting/rooms/new',
          name: 'create-room',
          component: () => import('../views/hosting/CreateRoomView.vue'),
        },
        {
          path: '/hosting/rooms/:id/edit',
          name: 'edit-room',
          component: () => import('../views/hosting/EditRoomView.vue'),
        },
        {
          path: '/hosting/rooms',
          name: 'room-list',
          component: () => import('../views/hosting/RoomListView.vue'),
        },
	{
      	  path:'/coupons',
      	  name:'coupons',
      	  component: CouponCenterView
    	},
    	{
      	  path: '/checkout',
     	  name: 'Checkout',
      	  component: () => import('../views/CheckoutPageView.vue')
    	},
      ],
    },

    // 2. 其他模組路由
    ...exampleARouter,
    ...exampleBRouter,
    ...ReportFormRouter,

    // 3. 訂單路由（使用自己的 BookingLayout）
    ...bookingRoutes,
    ...supportRoutes,

  ],
});

export default router;
