import type { RouteRecordRaw } from 'vue-router';

// Lazy load components for better performance
const HostDashboard = () => import('./views/HostDashboard.vue');
const ListingCreate = () => import('./views/ListingCreate.vue');
const ListingManage = () => import('./views/ListingManage.vue');

export const roomManagementRoutes: RouteRecordRaw[] = [
  {
    path: '/host/listings',
    name: 'HostDashboard',
    component: HostDashboard,
  },
  {
    path: '/host/listings/new',
    name: 'ListingCreate',
    component: ListingCreate,
  },
  {
    path: '/host/listings/:id/manage',
    name: 'ListingManage',
    component: ListingManage,
    props: true, // Allows passing route params as component props
  }
];
