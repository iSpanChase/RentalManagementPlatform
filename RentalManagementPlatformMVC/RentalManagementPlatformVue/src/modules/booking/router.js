export default [
  {
    path: '/booking',
    name: 'Booking',
    component: () => import('./layouts/BookingLayout.vue'),
    meta: { layout: 'Booking', requiresAuth: true },
    children: [
      {
        path: '/booking/confirm',
        name: 'BookingConfirmView',
        component: () => import('./pages/BookingConfirmView.vue'),
      },
    ],
  },
  {
    path: '/booking',
    component: () => import('./layouts/BookingLayout.vue'), // ← 使用訂單專用 Layout
    children: [
      {
        path: 'list',
        name: 'BookingList',
        component: () => import('./pages/BookingList.vue'),
      },
    ],
  },
];
