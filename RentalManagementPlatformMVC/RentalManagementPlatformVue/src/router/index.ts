import { createRouter, createWebHistory, type RouteLocationNormalized } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'dashboard',
      component: () => import('@/views/DashboardView.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/LoginView.vue'),
      meta: { requiresGuest: true },
    },
    {
      path: '/profile',
      name: 'profile',
      component: () => import('@/views/ProfileView.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/register',
      name: 'register',
      component: () => import('@/views/RegisterView.vue'),
      meta: { requiresGuest: true },
    },
    {
      path: '/forgot-password',
      name: 'forgot-password',
      component: () => import('@/views/ForgotPasswordView.vue'),
      meta: { requiresGuest: true },
    },
    {
      path: '/roles',
      name: 'roles',
      component: () => import('@/views/RolesView.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/permissions',
      name: 'permissions',
      component: () => import('@/views/PermissionsView.vue'),
      meta: { requiresAuth: true },
    },
    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: () => import('@/views/NotFoundView.vue'),
    },
    { path: '/reset-password', 
      name: 'reset-password', 
      component: () => import('@/views/ResetPasswordView.vue'), meta: { requiresGuest: true } }
  ],
})

/** 依路由參數取得 redirect 目的地 */
function getRedirectTarget(to: RouteLocationNormalized) {
  const q = to.query?.redirect
  // 只接受字串，避免非預期型別
  return typeof q === 'string' && q.trim().length > 0 ? q : '/'
}

const ensureProfileLoaded = async () => {
  const auth = useAuthStore()
  // 若已登入且尚未有 profile，嘗試載入
  if (auth.isAuthenticated.value && !auth.state.profile) {
    try {
      await auth.fetchProfile()
    } catch (err) {
      console.warn('無法取得使用者資料', err)
    }
  }
}

router.beforeEach(async (to) => {
  const auth = useAuthStore()
  const hasToken = !!localStorage.getItem('access_token')

  // 已登入 -> 保險載入 profile
  if (auth.isAuthenticated.value && !auth.state.profile) {
    await ensureProfileLoaded()
  }

  // 需要登入的頁面
  if (to.meta.requiresAuth) {
    if (auth.isAuthenticated.value) {
      await ensureProfileLoaded()
      return true
    }

    // store 未登入，但有 token：嘗試續期/取資料；失敗就清 token 並去登入
    if (hasToken) {
      try {
        if (auth.canRefresh?.value) {
          const ok = await auth.refreshSession()
          if (ok) {
            await ensureProfileLoaded()
            return true
          }
        }
        // 即使不能 refresh，也試著拉一次 profile（若後端允許）
        await auth.fetchProfile()
        if (auth.isAuthenticated.value) return true
      } catch {
        // 失敗就當作未登入處理
      }
      // 清掉壞 token，避免下次又被當成 hasToken
      localStorage.removeItem('access_token')
      localStorage.removeItem('refresh_token')
      return { name: 'login', query: { redirect: to.fullPath } }
    }

    // 沒登入也沒 token
    return { name: 'login', query: { redirect: to.fullPath } }
  }

  // 僅允許訪客的頁面（/login）
  if (to.meta.requiresGuest) {
    const hasToken = !!localStorage.getItem('access_token')
    if (auth.isAuthenticated.value || hasToken) {
      const target = getRedirectTarget(to)  // 你現有的輔助函式：從 query.redirect 取目標
      return target === '/' ? { name: 'dashboard' } : target
    }
  }
  return true
})

export default router
