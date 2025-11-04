import axios, { type AxiosError, type AxiosInstance, type InternalAxiosRequestConfig } from 'axios'
import { useAuthStore } from '@/stores/auth'

interface AuthHandlers {
    getAccessToken: () => string | null
    refreshToken: () => Promise<boolean>
    onUnauthorized: () => void
}

const api: AxiosInstance = axios.create({
    baseURL: import.meta.env.VITE_API_BASE_URL ?? '/api',
    withCredentials: true,
})

let authHandlers: AuthHandlers | null = null

export const registerAuthHandlers = (handlers: AuthHandlers) => {
    authHandlers = handlers
}

api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
  let token: string | null | undefined = authHandlers?.getAccessToken?.()
  // handlers 尚未註冊或沒拿到值 → 從 localStorage 兜底
  if (!token) {
    token = localStorage.getItem('rmp.accessToken')
  }
  token = (token ?? '').toString().trim()
  if (token.length > 10) {
    config.headers = config.headers ?? {}
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

type RetriableConfig = InternalAxiosRequestConfig & { _retry?: boolean }

api.interceptors.response.use(
    (response) => response,
    async (error: AxiosError) => {
        const originalRequest = error.config as RetriableConfig | undefined

        if (
            error.response?.status === 401 &&
            authHandlers &&
            originalRequest &&
            !originalRequest._retry
        ) {
            const url = originalRequest.url ?? ''
            if (url.includes('/Auth/refresh')) {
                authHandlers.onUnauthorized()
                return Promise.reject(error)
            }
            originalRequest._retry = true
            try {
                const refreshed = await authHandlers.refreshToken()
                if (refreshed) {
                    return api(originalRequest)
                }
            } catch (refreshError) {
                console.error('Token refresh failed', refreshError)
            }
            authHandlers.onUnauthorized()
        }

        return Promise.reject(error)
    },
)

// // 啟動時自動把 localStorage 的 token 帶到 header
// const existing = localStorage.getItem('access_token')
// if (existing) {
//     api.defaults.headers.common['Authorization'] = `Bearer ${existing}`
// }

export default api