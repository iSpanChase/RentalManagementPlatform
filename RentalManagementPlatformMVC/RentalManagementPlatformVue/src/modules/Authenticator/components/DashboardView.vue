<template>
  <div class="page-container">
    <section class="section">
      <header class="section-header">
        <div>
          <h1>歡迎回來，{{ profile?.name ?? profile?.username ?? '使用者' }}！</h1>
          <p class="section-subtitle">您可以在這裡快速檢視個人資訊、角色與權限。</p>
        </div>
        <RouterLink class="outline-button" to="/profile">更新個人資料</RouterLink>
      </header>

      <div v-if="profile" class="card-grid">
        <article class="info-card">
          <h2>基本資料</h2>
          <ul>
            <li><span>使用者ID</span><strong>{{ profile.userId }}</strong></li>
            <li><span>帳號</span><strong>{{ profile.username }}</strong></li>
            <li><span>姓名</span><strong>{{ profile.name }}</strong></li>
            <li><span>Email</span><strong>{{ profile.email }}</strong></li>
            <li><span>性別</span><strong>{{ profile.gender || '—' }}</strong></li>
            <li><span>生日</span><strong>{{ formatDate(profile.birthDate) }}</strong></li>
            <li><span>電話</span><strong>{{ profile.phone || '—' }}</strong></li>
            <li><span>地址</span><strong class="break-all">{{ profile.address }}</strong></li>
          </ul>
        </article>

        <article class="info-card">
          <h2>其他</h2>
          <ul>
            <li><span>點數</span><strong>{{ profile.point ?? 0 }}</strong></li>
            <!-- 取代 DashboardView.vue 裡的「其他」卡片內的大頭貼區塊 -->
            <li>
              <span>Email 驗證</span>
              <strong :class="profile.isVerified ? 'status--success' : 'status--error'">
                {{ profile.isVerified ? '已通過' : '未通過' }}
              </strong>
            </li>
            <li v-if="avatarUrl">
              <span>大頭貼連結</span>
              <strong>
                <a class="link" :href="avatarUrl" target="_blank" rel="noopener">開啟連結</a>
              </strong>
            </li>
            <li>
              <span>大頭貼</span>
              <strong class="avatar-wrap">
                <img
                  v-if="avatarUrl"
                  :src="avatarUrl"
                  alt="avatar"
                  class="avatar-lg"
                  @error="onAvatarError"
                />
                <span v-else>—</span>
              </strong>
            </li>
          </ul>
        </article>

        <article class="info-card" v-if="roles?.length">
          <h2>角色</h2>
          <div class="chips">
            <span v-for="r in roles" :key="r" class="chip">{{ r }}</span>
          </div>
        </article>

        <article class="info-card" v-if="permissions?.length">
          <h2>權限</h2>
          <div class="chips">
            <span v-for="p in permissions" :key="p" class="chip">{{ p }}</span>
          </div>
        </article>
      </div>

      <div v-else class="skeleton">
        載入中…
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { onMounted } from 'vue'
import { computed } from 'vue'
import { RouterLink } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
const { state, fetchProfile } = useAuthStore()

const auth = useAuthStore()
const profile = computed(() => auth.state.profile)
const roles = computed(() => auth.state.roles)
const permissions = computed(() => auth.state.permissions)

const fallbackAvatar = computed(() => {
  const name = profile.value?.name || profile.value?.username || 'User'
  // 你也可以換成本地 /images/default-avatar.png
  return `https://ui-avatars.com/api/?name=${encodeURIComponent(name)}&background=C7D2FE&color=111827&size=128&rounded=true`
})

const avatarUrl = computed(() => profile.value?.profileImageUrl || fallbackAvatar.value)

onMounted(async () => {
  if (!state.profile) {
    try { await fetchProfile() } catch {}
  }
})

function onAvatarError(e: Event) {
  (e.target as HTMLImageElement).src = fallbackAvatar.value
}

function formatDate(iso: string | null | undefined) {
  if (!iso) return '—'
  const d = new Date(iso)
  if (Number.isNaN(+d)) return '—'
  return `${d.getFullYear()}-${String(d.getMonth()+1).padStart(2,'0')}-${String(d.getDate()).padStart(2,'0')}`
}
</script>

<style scoped>
/* 保留你的原本風格與命名 */

.page-container {
  display: grid;
  place-items: center;
  padding: 40px 16px;
  background: #f8fafc;
  min-height: 100vh;
}

.section {
  width: 100%;
  max-width: 1100px;
  background: white;
  border-radius: 16px;
  padding: 24px;
  box-shadow: 0 10px 30px rgba(2, 6, 23, 0.08);
}

.section-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
  margin-bottom: 16px;
}

.section-header h1 {
  margin: 0 0 6px;
  font-size: 1.5rem;
  font-weight: 700;
  letter-spacing: 0.4px;
  color: #0f172a;
}

.section-subtitle {
  margin: 0;
  color: #475569;
}

.card-grid {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 16px;
}

.info-card {
  border: 1px solid #e2e8f0;
  border-radius: 12px;
  padding: 18px;
}

.info-card h2 {
  margin: 0 0 12px;
  font-size: 1.1rem;
  font-weight: 700;
  color: #0f172a;
}

.info-card ul {
  list-style: none;
  padding: 0;
  margin: 0;
  display: grid;
  gap: 8px;
}

.info-card li {
  display: flex;
  align-items: baseline;
  justify-content: space-between;
  gap: 12px;
}

.info-card li span {
  color: #475569;
}

.info-card li strong {
  color: #0f172a;
}

.chips {
  display: flex;
  flex-wrap: wrap;
  gap: 8px;
}

.chip {
  background: #eff6ff;
  color: #1d4ed8;
  border: 1px solid rgba(59, 130, 246, 0.15);
  padding: 6px 10px;
  border-radius: 999px;
  font-weight: 600;
  font-size: 0.85rem;
}

.skeleton {
  padding: 12px;
  color: #64748b;
}

.link {
  color: #2563eb;
  font-weight: 600;
  text-decoration: none;
}

.link:hover {
  text-decoration: underline;
}

.outline-button {
  border: 1px solid rgba(59, 130, 246, 0.4);
  border-radius: 999px;
  padding: 10px 18px;
  color: #2563eb;
  font-weight: 600;
  text-decoration: none
}

.status--success {
  color: #059669;
}

.status--error {
  color: #dc2626;
}

.avatar-wrap { display: inline-flex; align-items: center; gap: 8px; }
.avatar-lg { width: 96px; height: 96px; border-radius: 999px; object-fit: cover; border: 1px solid #e5e7eb; }
</style>
