import type { UserProfile } from '@/types/auth'

export function isManagerRole(roles: string[] = []): boolean {
    if (!roles?.length) return false
    const managerRoles = ['ADMIN', 'OPERATOR']
    return roles.some(r => managerRoles.includes(r.toUpperCase()))
}


/** 取得使用者顯示名稱 */
export function getDisplayName(profile: UserProfile | null | undefined): string {
    if (!profile) return '訪客'
    return profile.name || profile.username || '訪客'
}
