// ===== types/auth.ts =====

export interface UserProfile {
    userId: number
    username: string
    email: string
    name: string
    gender: string
    /** ISO 8601 datetime string, e.g. "2025-10-28T00:00:00.000Z" */
    birthDate: string
    phone: string | null
    address: string
    point: number | null
    profileImageUrl: string | null
    /** 顯示用：Email 驗證狀態（後端若提供就帶，沒有可忽略） */
    isVerified?: boolean
}

export interface LoginResponse {
    accessToken: string
    /** ISO 8601 datetime string; 若後端不回，前端會從 JWT exp 解析 */
    expiresAt?: string
    refreshToken?: string
    profile: UserProfile
    roles: string[]
    permissions: string[]
}

export interface LoginRequest {
    email: string
    password: string
}

export interface RegistrationRequest {
    roleCode: string
    email: string
    passwordHash: string
    name: string
    username: string
    phone?: string
    gender: string
    birthDate: string        // yyyy-MM-dd
    address: string
    profileImageUrl?: string
    companyName?: string | null
    taxId?: string | null
    nationalIdTail?: string | null
}

/** 更新「我的個人資料」所需欄位（允許修改的部分） */
export interface UpdateProfileRequest {
    name: string
    gender: string
    /** 後端期望 yyyy-MM-dd */
    birthDate: string
    phone: string | null
    address: string
    point: number | null
    isverified?: boolean
    profileImageUrl: string | null
}

export interface RoleSummary {
    id: number
    name: string
    code: string
}

export interface PermissionSummary {
    id: number
    code: string
    displayName: string
    category: string
}
