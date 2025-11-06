import { createRouter, createWebHistory, type RouteLocationNormalized } from 'vue-router';
import MainLayout from '@/layouts/MainLayout.vue';
import HomeView from '../views/HomeView.vue';
import AuthenticatorRouter from '@/modules/Authenticator/router';
import ReportFormRouter from '@/modules/ReportForm/router';
import bookingRoutes from '@/modules/booking/router';
import CouponCenterView from '../views/CouponCenterView.vue';
import supportRoutes from '@/modules/faq/router';
import { useAuthStore } from '@/stores/auth';

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
                    meta: {
                        requiresAuth: true,
                        requiredPerms: ['RoomList.Create'],
                    },
                },
                {
                    path: '/hosting/rooms/:id/edit',
                    name: 'edit-room',
                    component: () => import('../views/hosting/EditRoomView.vue'),
                    meta: {
                        requiresAuth: true,
                        requiredPerms: ['RoomList.Edit'],
                    },
                },
                {
                    path: '/hosting/rooms',
                    name: 'room-list',
                    component: () => import('../views/hosting/RoomListView.vue'),
                    meta: {
                        requiresAuth: true,
                        requiredPerms: ['RoomList.View'],
                    },
                },
                {
                    path: '/coupons',
                    name: 'coupons',
                    component: CouponCenterView
                },
                {
                    path: '/checkout',
                    name: 'Checkout',
                    component: () => import('../views/CheckoutPageView.vue')
                },
                {
                    path: '/my-bookings',
                    name: 'MyBookings',
                    component: () => import('@/modules/booking/pages/MyBookingsView.vue'),
                    meta: {
                        requiresAuth: true,
                        requiredPerms: ['Booking.View'],
                    },
                },
                {
                    path: '/my-orders',
                    name: 'MyOrders',
                    component: () => import('@/modules/booking/pages/MyOrdersView.vue'),
                    meta: {
                        requiresAuth: true,
                        requiredPerms: ['Booking.ManageAll'],
                    },
                },
            ],
        },

        // 2. 其他模組路由
        ...ReportFormRouter,

        // 3. 訂單路由（使用自己的 BookingLayout）
        ...AuthenticatorRouter,
        ...bookingRoutes,
        ...supportRoutes,
    ],
});

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
};

router.beforeEach(async (to) => {
    // ★ callback 路由一律放行（不要做登入檢查/導轉）
    if (to.path === '/auth/callback') return true
    const auth = useAuthStore()
    // ★ 先還原 token & headers，避免第一發 /Users/me 變 401
    auth.restoreSession()
    // ★ 與 http.ts / AuthCallback.vue 對齊
    const hasToken = !!localStorage.getItem('rmp.accessToken')

    // 需要登入的頁面才取個資；若沒有登入，等會一起導去登入或 Forbidden
    if (to.meta.requiresAuth || (Array.isArray(to.meta.requiredPerms) && to.meta.requiredPerms.length)) {
        if (!auth.state.profile) {
            await auth.fetchProfile() // 這裡 401 會被吞掉並清 session
        }
        // 若還是沒登入（或 token 壞掉被清），導去登入頁
        if (!auth.state.accessToken) {
            return { name: 'login', query: { redirect: to.fullPath } }
        }
        // 權限需求再補拉 abilities（你有實作就會更新；沒有就維持登入回傳）
        if (!auth.state.permissions?.length && auth.fetchAbilities) {
            try { await auth.fetchAbilities() } catch { }
        }
        const need = (to.meta.requiredPerms as string[]) || []
        if (need.length && !need.every(auth.can)) {
            return { name: 'forbidden' }
        }
    }

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
            localStorage.removeItem('rmp.accessToken')
            localStorage.removeItem('rmp.refreshToken')
            return { name: 'login', query: { redirect: to.fullPath } }
        }

        // 沒登入也沒 token
        return { name: 'login', query: { redirect: to.fullPath } };
    }

    // 僅允許訪客的頁面（/login）
    if (to.meta.requiresGuest) {
        const hasToken = !!localStorage.getItem('rmp.accessToken')
        if (auth.isAuthenticated.value || hasToken) {
            const target = getRedirectTarget(to)  // 你現有的輔助函式：從 query.redirect 取目標
            return target === '/' ? { name: 'dashboard' } : target
        }
    }
    return true
})

export default router;
