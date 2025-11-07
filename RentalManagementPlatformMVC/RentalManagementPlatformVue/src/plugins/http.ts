import axios from 'axios'
import { useAuthStore } from '@/stores/auth'

// 建立 axios 實例
const http = axios.create({
    baseURL: import.meta.env.VITE_API_BASE,
    withCredentials: true,
})

// 請求攔截器：自動帶上 JWT Token（若有登入）
http.interceptors.request.use((config) => {
    try {
        const auth = useAuthStore()

        // 這裡改用你新的 auth.state.accessToken
        const token = auth.state.accessToken
        if (token) {
            config.headers.Authorization = `Bearer ${token}`
        }
    } catch {
        // 在初始化階段 store 可能還沒載入，不做動作
    }

    return config
})

// 回應攔截器（可選）：自動處理過期 Token
http.interceptors.response.use(
    (response) => response,
    async (error) => {
        const auth = useAuthStore()

        // 如果後端回傳 401，試著刷新一次 session
        if (error.response?.status === 401 && auth.canRefresh?.value) {
            try {
                const ok = await auth.refreshSession()
                if (ok) {
                    // 成功刷新，重新發送原請求
                    const newToken = auth.state.accessToken
                    error.config.headers.Authorization = `Bearer ${newToken}`
                    return http.request(error.config)
                }
            } catch {
                console.warn('Token refresh failed, logging out...')
            }
        }

        return Promise.reject(error)
    }
)

export default http
