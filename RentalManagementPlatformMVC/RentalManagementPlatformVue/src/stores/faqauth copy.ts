// src/stores/faqauth.ts
import { defineStore } from 'pinia'
import http from '@/plugins/http'

export type UserRole = 'GUEST' | 'TENANT' | 'HOST' | 'SUPPLIER' | 'OPERATOR' | 'ADMIN'
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
    token: localStorage.getItem('token') || '',
    user: null as UserProfile | null,
    roles: [] as UserRole[],
    perms: [] as string[],
    bootstrapped: false,
  }),
  getters: {
    // 管理/客服角色
    isManager: (s) => s.roles.includes('ADMIN') || s.roles.includes('OPERATOR'),
    isSignedIn: (s) => !!s.user?.userId,
  },
  actions: {
    setToken(t: string) { this.token = t; localStorage.setItem('token', t) },

    async fetchMe() {
      const [{ data: profile }, { data: abilities }] = await Promise.all([
        http.get('/api/auth/me'),
        http.get('/api/auth/me/abilities'),
      ])
      this.user = profile as UserProfile
      this.roles = (abilities.roles ?? []) as UserRole[]
      this.perms = abilities.perms ?? []
    },

    async bootstrap() {
      try {
        if (this.token) await this.fetchMe()
        else {
          // 無 token → 當訪客（可省略）
          this.user = null
          this.roles = ['GUEST']
          this.perms = []
        }
      } finally { this.bootstrapped = true }
    },

    logout() {
      this.token = ''; localStorage.removeItem('token')
      this.user = null; this.roles = ['GUEST']; this.perms = []
    }
  }
})