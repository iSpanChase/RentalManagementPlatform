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
      {
        path: 'mybookings',
        name: 'MyBookings',
        component: () => import('./pages/MyBookingsView.vue'),
        meta: {
          requiresAuth: true,
          requiredPerms: ['Booking.View']
        },
      },
      {
        path: 'myorders',
        name: 'MyOrders',
        component: () => import('./pages/MyOrdersView.vue'),
        meta: {
          requiresAuth: true,
          requiredPerms: ['Booking.ManageAll']
        },
      },
    ],
  },
];
