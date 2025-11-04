// src/stores/faqauth.ts
import { defineStore } from 'pinia'
import http from '@/plugins/http'

export type UserRole = 'user' | 'agent' | 'guest'   // ✅ 加上 guest
export type UserProfile = {
  userId: number | string
  name: string
  username?: string
  email?: string
  profileImageUrl?: string
  role: UserRole
}

export const useAuthStore = defineStore('auth', {
  state: () => ({
    token: '',                         // 登入後塞進來
    user: null as UserProfile | null,  // 當前使用者（可為 guest）
  }),
  actions: {
    // ✅ 先寫死（含 guest）
    useMock(role: UserRole = 'guest') {
      if (role === 'agent') {
        this.user = { userId: 'A-0001', name: '測試客服', role }
      } else if (role === 'user') {
        this.user = { userId: 'U-0001', name: '測試房客', role }
      } else {
        this.user = { userId: 'G-' + Math.random().toString(36).slice(2, 8), name: '訪客', role }
      }
    },

    setToken(t: string) { this.token = t },

    // ★ 正式：從 /me 取得（沒有就維持 mock）
    async fetchMe() {
      const { data } = await http.get('/api/users/me')
      // 若後端沒有 role 欄位，就用你們的規則推斷；預設 user
      const role = (data.role ??
        (data.username?.startsWith('agent') ? 'agent' : 'user')) as UserRole

      this.user = {
        userId: data.userId,
        name: data.name ?? data.username ?? '訪客',
        username: data.username,
        email: data.email,
        profileImageUrl: data.profileImageUrl,
        role,
      }
    }
  }
})
