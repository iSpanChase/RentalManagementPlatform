import axios, { type AxiosError, type AxiosInstance, type InternalAxiosRequestConfig } from 'axios'

const baseURL = import.meta.env.VITE_API_BASE_URL ?? 'http://localhost:7230'

interface AuthHandlers {
    getAccessToken: () => string | null
    refreshToken: () => Promise<boolean>
    onUnauthorized: () => void
}

const api: AxiosInstance = axios.create({
    baseURL: '/api',
    withCredentials: true,
})

let authHandlers: AuthHandlers | null = null

export const registerAuthHandlers = (handlers: AuthHandlers) => {
    authHandlers = handlers
}

api.interceptors.request.use((config: InternalAxiosRequestConfig) => {
    if (authHandlers) {
        const token = authHandlers.getAccessToken()
        if (token) {
            config.headers = config.headers ?? {}
            config.headers.Authorization = `Bearer ${token}`
        }
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
            if (url.includes('/api/Auth/refresh')) {
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

// 啟動時自動把 localStorage 的 token 帶到 header
const existing = localStorage.getItem('access_token')
if (existing) {
    api.defaults.headers.common['Authorization'] = `Bearer ${existing}`
}

export default api