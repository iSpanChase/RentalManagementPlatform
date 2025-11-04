export default [
  {
    path: '/auth',
    name: 'auth',
    component: () => import('./layouts/AuthenticatorLayout.vue'),
    children: [
      {
        path: 'reviewoperators',
        name: 'AdminReviewOperators',
        component: () => import('./components/AdminReviewOperators.vue'),
        meta: { layout: 'auth', requiresAuth: true },
        alias: ['/reviewoperators', '/review-operators'],
      },
      {
        path: 'dashboard',
        name: 'dashboard',
        component: () => import('./components/DashboardView.vue'),
        alias: '/dashboard',
      },
      {
        path: 'forbidden',
        name: 'forbidden',
        component: () => import('./components/ForbiddenView.vue'),
        alias: '/forbidden',
      },
      {
        path: 'forgotPassword',
        name: 'forgotPassword',
        component: () => import('./components/ForgotPasswordView.vue'),
        alias: ['/forgotPassword', '/forgot-password'],
      },
      {
        path: 'login',
        name: 'login',
        component: () => import('./components/LoginView.vue'),
        alias: '/login'
      },
      {
        path: 'notfound',
        name: 'NotFoundView',
        component: () => import('./components/NotFoundView.vue'),
        alias: ['/notfound', '/not-found'],
      },
      {
        path: 'perms',
        name: 'perms',
        component: () => import('./components/PermissionsView.vue'),
        alias: '/perms',
      },
      {
        path: 'profile',
        name: 'profile',
        component: () => import('./components/ProfileView.vue'),
        alias: '/profile'
      },
      {
        path: 'register',
        name: 'register',
        component: () => import('./components/RegisterView.vue'),
        alias: '/register'
      },
      {
        path: 'resetpassword',
        name: 'resetpassword',
        component: () => import('./components/ResetPasswordView.vue'),
        alias: ['/resetpassword', '/reset-password'],
      },
      {
        path: 'roles',
        name: 'roles',
        component: () => import('./components/RolesView.vue'),
        alias: '/roles',
      },
      {
        path: 'VerifyEmailSuccess',
        name: 'VerifyEmailSuccess',
        component: () => import('./components/VerifyEmailSuccessView.vue'),
        alias: ['/verify-email/success', '/Verify-EmailSuccess', '/VerifyEmailSuccess'],
      },
      {
        path: 'verifyemail',
        name: 'VerifyEmailView',
        component: () => import('./components/VerifyEmailView.vue'),
        alias: ['/verify-email', '/verifyemail'],
      },
    ],
  },
];

