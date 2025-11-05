export default [
  {
    path: '/booking',
    name: 'Booking',
    component: () => import('./layouts/BookingLayout.vue'),
    meta: { layout: 'Booking', requiresAuth: true },
    children: [
      {
        path: 'confirm',
        name: 'BookingConfirmView',
        component: () => import('./pages/BookingConfirmView.vue'),
        meta: {
          requiresAuth: true,
          requiredPerms: ['Booking.Create']
        },
      },
    ],
  },
];
