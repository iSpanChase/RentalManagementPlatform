// src/router/modules/booking/router.ts
export default [
    {
        path: '/booking',
        name: 'Booking',
        component: () => import('../../../modules/booking/layouts/BookingLayout.vue'),
        meta: { layout: 'booking', requiresAuth: true },
        children: [
            {
                path: 'confirm',
                name: 'Booking.Confirm',
                component: () => import('../../../modules/booking/pages/BookingConfirmView.vue')
            },
        ]
    }
]
