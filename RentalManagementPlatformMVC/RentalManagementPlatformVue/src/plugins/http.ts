import axios from 'axios'
import { useAuthStore } from '@/stores/faqauth'

const http = axios.create({
    baseURL: import.meta.env.VITE_API_BASE,
    withCredentials: true,
})

// 在每次請求自動帶上 JWT（如果有）
http.interceptors.request.use((config) => {
    try {
        const auth = useAuthStore()
        if (auth.token) config.headers.Authorization = `Bearer ${auth.token}`
    } catch { /* 首次載入時 store 尚未建立 */ }
    return config
})

export default http