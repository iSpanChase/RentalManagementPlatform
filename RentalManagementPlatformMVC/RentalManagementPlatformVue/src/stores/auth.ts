// ===== stores/auth.ts =====
import { computed, reactive } from 'vue'
import api, { registerAuthHandlers } from '@/services/http'
import type {
  LoginRequest,
  LoginResponse,
  PermissionSummary,
  RegistrationRequest,
  RoleSummary,
  UpdateProfileRequest,
  UserProfile,
} from '@/types/auth'

const storageKeys = {
  accessToken: 'rmp.accessToken',
  refreshToken: 'rmp.refreshToken',
  expiresAt: 'rmp.expiresAt',
  profile: 'rmp.profile',
  roles: 'rmp.roles',
  permissions: 'rmp.permissions',
}

interface AuthState {
  accessToken: string | null
  refreshToken: string | null
  expiresAt: string | null
  profile: UserProfile | null
  roles: string[]
  permissions: string[]
  loading: boolean
  error: string | null
}

const loadArray = (key: string): string[] => {
  try {
    const raw = localStorage.getItem(key)
    return raw ? (JSON.parse(raw) as string[]) : []
  } catch {
    return []
  }
}

const loadProfile = (): UserProfile | null => {
  try {
    const raw = localStorage.getItem(storageKeys.profile)
    return raw ? (JSON.parse(raw) as UserProfile) : null
  } catch {
    return null
  }
}

/** 安全的 base64url 轉一般 base64 並解碼成字串；失敗回 null */
function safeBase64UrlDecode(segment: string | null | undefined): string | null {
  if (!segment || typeof segment !== 'string') return null
  let b64 = segment.replace(/-/g, '+').replace(/_/g, '/')
  const pad = b64.length % 4
  if (pad) b64 += '='.repeat(4 - pad)
  try {
    return atob(b64)
  } catch {
    return null
  }
}

/** 從 JWT 取 exp（秒）。任何錯誤一律回 null，不拋例外 */
function readJwtExpSeconds(token: string | null | undefined): number | null {
  if (!token || typeof token !== 'string') return null
  const parts = token.split('.')
  if (parts.length < 2) return null
  const payloadJson = safeBase64UrlDecode(parts[1])
  if (!payloadJson) return null
  try {
    const payload = JSON.parse(payloadJson) as { exp?: number }
    return typeof payload.exp === 'number' ? payload.exp : null
  } catch {
    return null
  }
}

/** 設定/移除 Axios Authorization header */
function applyAuthHeader(token: string | null) {
  if (token) {
    api.defaults.headers.common['Authorization'] = `Bearer ${token}`
  } else {
    delete api.defaults.headers.common['Authorization']
  }
}

const state = reactive<AuthState>({
  accessToken: localStorage.getItem(storageKeys.accessToken),
  refreshToken: localStorage.getItem(storageKeys.refreshToken),
  expiresAt: localStorage.getItem(storageKeys.expiresAt),
  profile: loadProfile(),
  roles: loadArray(storageKeys.roles),
  permissions: loadArray(storageKeys.permissions),
  loading: false,
  error: null,
})

/** 啟動時把現有 token 套到 Axios */
applyAuthHeader(state.accessToken)

const persistSession = () => {
  if (state.accessToken) localStorage.setItem(storageKeys.accessToken, state.accessToken)
  else localStorage.removeItem(storageKeys.accessToken)

  if (state.refreshToken) localStorage.setItem(storageKeys.refreshToken, state.refreshToken)
  else localStorage.removeItem(storageKeys.refreshToken)

  if (state.expiresAt) localStorage.setItem(storageKeys.expiresAt, state.expiresAt)
  else localStorage.removeItem(storageKeys.expiresAt)

  if (state.profile) localStorage.setItem(storageKeys.profile, JSON.stringify(state.profile))
  else localStorage.removeItem(storageKeys.profile)

  localStorage.setItem(storageKeys.roles, JSON.stringify(state.roles))
  localStorage.setItem(storageKeys.permissions, JSON.stringify(state.permissions))
}

const resolveErrorMessage = (err: unknown): string => {
  if (!err) return '發生未知錯誤'
  if (typeof err === 'string') return err
  if (err instanceof Error) return err.message
  const maybeAxios = err as { response?: { data?: unknown; status?: number } }
  const data = maybeAxios.response?.data
  if (typeof data === 'string') return data
  if (data && typeof data === 'object' && 'title' in data && typeof (data as any).title === 'string')
    return (data as any).title
  if (data && typeof data === 'object' && 'message' in data && typeof (data as any).message === 'string')
    return (data as any).message
  if (maybeAxios.response?.status === 401) return '尚未登入或登入已過期'
  return '發生未知錯誤'
}

const clearSession = () => {
  state.accessToken = null
  state.refreshToken = null
  state.expiresAt = null
  state.profile = null
  state.roles = []
  state.permissions = []
  state.error = null
  persistSession()
  applyAuthHeader(null)
}

/** 這裡若 payload 沒帶 expiresAt，就從 JWT 解析 exp 自動補上 */
const setSession = (payload: LoginResponse) => {
  state.accessToken = payload.accessToken ?? null
  state.refreshToken = payload.refreshToken ?? null

  let expiresAt = payload.expiresAt ?? null
  if (!expiresAt && payload.accessToken) {
    const expSec = readJwtExpSeconds(payload.accessToken)
    if (expSec) expiresAt = new Date(expSec * 1000).toISOString()
  }
  state.expiresAt = expiresAt

  // 後端回傳的 profile 直接保存（型別即為 UserProfile）
  state.profile = payload.profile ?? null

  state.roles = payload.roles ?? []
  state.permissions = payload.permissions ?? []

  persistSession()
  applyAuthHeader(state.accessToken)
}

/** 若沒有 expiresAt，就用 JWT exp；若也解析不到，視為「尚未過期」 */
const isTokenExpired = computed(() => {
  if (state.expiresAt) {
    const ms = new Date(state.expiresAt).getTime()
    if (!Number.isNaN(ms)) return ms <= Date.now() + 30_000
  }
  const expSec = readJwtExpSeconds(state.accessToken)
  if (expSec) return expSec * 1000 <= Date.now() + 30_000
  return false
})

const isAuthenticated = computed(() => !!state.accessToken && !isTokenExpired.value)
const canRefresh = computed(() => !!state.refreshToken)

const login = async (request: LoginRequest) => {
  state.loading = true
  state.error = null
  try {
    const { data } = await api.post<LoginResponse>('/Auth/login', request)
    setSession(data)
    // 立刻拉一次 /Users/me，確保拿到 gender / birthDate / address 等完整欄位
    try {
      await fetchProfile()
      await fetchAbilities()
    } catch {
    // 即便失敗也不影響原本登入流程
    }
    // 回傳最新的 profile（若成功）或原本登入回來的
    return state.profile ?? data
  } catch (error) {
    state.error = resolveErrorMessage(error)
    throw error
  } finally {
    state.loading = false
  }
}

/** 從後端取得「目前使用者」的 roles 與 permissions（即時） */
const fetchAbilities = async () => {
  state.error = null
  try {
    const { data } = await api.get<{ roles: string[]; perms: string[] }>('/Auth/me/abilities')
    state.roles = Array.isArray(data?.roles) ? data.roles : []
    state.permissions = Array.isArray(data?.perms) ? data.perms : []
    persistSession()
    return { roles: state.roles, permissions: state.permissions }
  } catch (error) {
    // 若沒有這支 API，保留現狀（從 LoginResponse 來的陣列）
    state.error = resolveErrorMessage(error)
    // 可選：這裡不 throw，避免頁面啟動時卡住
    return { roles: state.roles, permissions: state.permissions }
  }
}

/** 前端授權判斷輔助 */
const hasRole = (roleCode: string) => state.roles.includes(roleCode)
const can = (permCode: string) => state.permissions.includes(permCode)


const forgotPassword = async (email: string) => {
  state.error = null
  try {
    // 後端建議路由：POST /Auth/forgot-password  body: { email }
    await api.post('/Auth/forgot-password', { email })
    return true
  } catch (error) {
    state.error = resolveErrorMessage(error)
    throw error
  }
}


const register = async (request: RegistrationRequest) => {
  state.loading = true
  state.error = null
  try {
    const { data } = await api.post<UserProfile>('/Users/register', request)
    return data
  } catch (error) {
    state.error = resolveErrorMessage(error)
    throw error
  } finally {
    state.loading = false
  }
}

const refreshSession = async (): Promise<boolean> => {
  if (!state.refreshToken) return false
  try {
    const { data } = await api.post<LoginResponse>('/Auth/refresh', {
      refreshToken: state.refreshToken,
    })
    setSession(data)
    return true
  } catch (error) {
    console.error('Refresh token request failed', error)
    clearSession()
    return false
  }
}

const logout = async () => {
  if (!state.accessToken) {
    clearSession()
    return
  }
  try {
    await api.post('/Auth/logout')
  } catch {
    /* ignore */
  } finally {
    clearSession()
  }
}

/** 取得目前登入者的完整個人資料（UserProfile） */
const fetchProfile = async () => {
  state.error = null
  try {
    const { data } = await api.get<UserProfile>('/Users/me')
    state.profile = data
    persistSession()
    return data
  } catch (err: any) {
    // 未登入 / token 失效：清理並不拋出，讓路由守衛接手導向
    if (err?.response?.status === 401) {
      clearSession()
      return null
    }
    state.error = resolveErrorMessage(err)
    throw err
  }
}

// auth.ts
const updateProfile = async (payload: UpdateProfileRequest) => {
  state.error = null
  try {
    // 取回 status，因為可能是 204
    const res = await api.put<UserProfile | undefined>('/Users/me', payload)
    const { status, data } = res

    if (status === 200 && data) {
      // 只有在真的拿到資料時才覆蓋
      state.profile = data
      persistSession()
      return data
    }

    // 204 或沒有資料：改用 fetchProfile 取最新
    try {
      const latest = await fetchProfile()
      return latest
    } catch (e) {
      // 拉不回資料就維持現狀，不要把 profile 清空
      return state.profile
    }
  } catch (error) {
    state.error = resolveErrorMessage(error)
    throw error
  }
}

/** 啟動時從 localStorage 還原登入狀態，並把 Authorization 設回 Axios */
const restoreSession = () => {
  try {
    const raw = localStorage.getItem('auth.session')
    if (!raw) return
    const s = JSON.parse(raw)

    state.accessToken = s?.accessToken || ''
    state.refreshToken = s?.refreshToken || ''
    state.profile = s?.profile || null
    state.roles = Array.isArray(s?.roles) ? s.roles : []
    state.permissions = Array.isArray(s?.permissions) ? s.permissions : []

    if (state.accessToken) {
      api.defaults.headers.common.Authorization = `Bearer ${state.accessToken}`
    }
  } catch {
    // 壞掉的快取就清掉
    clearSession()
  }
}

const getRoles = async () => {
  state.error = null
  try {
    const { data } = await api.get<RoleSummary[]>('/Roles')
    return data
  } catch (error) {
    state.error = resolveErrorMessage(error)
    throw error
  }
}

const getPermissions = async () => {
  state.error = null
  try {
    const { data } = await api.get<PermissionSummary[]>('/Permissions')
    return data
  } catch (error) {
    state.error = resolveErrorMessage(error)
    throw error
  }
}

const assignRoleToUser = async (roleId: number, userId: number) => {
  state.error = null
  try {
    await api.post(`/Roles/${roleId}/users/${userId}`)
    if (state.profile?.userId === userId) await fetchAbilities()
  } catch (error) {
    state.error = resolveErrorMessage(error)
    throw error
  }
}

const revokeRoleFromUser = async (roleId: number, userId: number) => {
  state.error = null
  try {
    await api.delete(`/Roles/${roleId}/users/${userId}`)
    if (state.profile?.userId === userId) await fetchAbilities()
  } catch (error) {
    state.error = resolveErrorMessage(error)
    throw error
  }
}

const updateRolePermissions = async (
  roleId: number,
  permissionIds: number[],
  action: 'assign' | 'remove',
) => {
  const url = `/Permissions/roles/${roleId}`
  state.error = null
  try {
    if (action === 'assign') {
      await api.post(url, { permissionIds })
    } else {
      await api.delete(url, { data: { permissionIds } })
    }
    // 若當前使用者擁有此 role，則也更新本地 abilities（需要後端 /Auth/me/abilities）
    await fetchAbilities()
  } catch (error) {
    state.error = resolveErrorMessage(error)
    throw error
  }
}

const resetPassword = async (email: string, token: string, newPassword: string) => {
  state.error = null
  try {
    await api.post('/Auth/reset-password', { email, token, newPassword })
    return true
  } catch (error: any) {
    // 取出後端訊息
    const m = error?.response?.data?.message || error?.message || '重設失敗'
    state.error = m
    throw error
  }
}

/** 交給 http 攔截器：自動帶 token / 自動 refresh / 401 清除 */
registerAuthHandlers({
  getAccessToken: () => state.accessToken,
  refreshToken: async () => {
    if (!canRefresh.value) return false
    if (!isTokenExpired.value) return true
    return refreshSession()
  },
  onUnauthorized: () => {
    clearSession()
  },
})

export const useAuthStore = () => ({
  state,
  isAuthenticated,
  canRefresh,
  login,
  logout,
  refreshSession,
  forgotPassword,
  register,
  fetchProfile,
  updateProfile,
  getRoles,
  getPermissions,
  assignRoleToUser,
  revokeRoleFromUser,
  updateRolePermissions,
  resetPassword,
  setSession,
  clearSession,
  fetchAbilities,
  hasRole,
  can,
  restoreSession,
})

export type AuthStore = ReturnType<typeof useAuthStore>
