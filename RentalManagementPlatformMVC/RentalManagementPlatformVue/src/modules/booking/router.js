// src/modules/reportForm/router.js
export default [
  {
    path: '/booking',
    name: 'Booking',
    component: () => import('./layouts/BookingLayout.vue'), // ReportForm 的父頁面
    meta: { layout: 'Booking', requiresAuth: true },
    children: [
      {
        path: '',
        name: 'Booking.BookingConfirmView',
        component: () => import('./pages/BookingConfirmView.vue'),
      },
    ],
  },
];
