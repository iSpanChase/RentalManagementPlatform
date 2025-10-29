<template>
  <header class="app-header">
    <RouterLink class="brand" to="/">Rental Platform</RouterLink>
    <nav v-if="isAuthenticated" class="nav">
      <RouterLink to="/" class="nav-link" active-class="is-active">儀表板</RouterLink>
      <RouterLink to="/roles" class="nav-link" active-class="is-active">角色</RouterLink>
      <RouterLink to="/permissions" class="nav-link" active-class="is-active">權限</RouterLink>
      <RouterLink to="/profile" class="nav-link" active-class="is-active">更新個人資料</RouterLink>
    </nav>
    <div class="spacer"></div>
    <div class="actions">
      <RouterLink to="/profile" class="avatar-mini-wrap" v-if="profile">
        <img :src="avatarMini" alt="me" class="avatar-mini" @error="onMiniError" />
      </RouterLink>
      <span v-if="profile" class="greeting">嗨，{{ profile.name || profile.username }}</span>
      <button v-if="isAuthenticated" class="ghost-button" type="button" @click="handleLogout">
        登出
      </button>
      <RouterLink v-else class="ghost-button" to="/login">登入</RouterLink>
    </div>
  </header>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { RouterLink, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const auth = useAuthStore()

const isAuthenticated = computed(() => auth.isAuthenticated.value)
const profile = computed(() => auth.state.profile)

const fallbackMini = computed(() => {
  const name = profile.value?.name || profile.value?.username || 'User'
  return `https://ui-avatars.com/api/?name=${encodeURIComponent(name)}&background=C7D2FE&color=111827&size=64&rounded=true`
})

const avatarMini = computed(() => profile.value?.profileImageUrl || fallbackMini.value)

function onMiniError(e: Event) {
  ;(e.target as HTMLImageElement).src = fallbackMini.value
}

const handleLogout = async () => {
  await auth.logout()
  router.replace({ name: 'login' })
}
</script>

<style scoped>
.app-header {
  display: flex;
  align-items: center;
  gap: 16px;
  padding: 16px 24px;
  background: #2151c2;
  color: white;
  position: sticky;
  top: 0;
  z-index: 10;
}

.brand {
  font-weight: 700;
  font-size: 1.25rem;
  color: white;
  text-decoration: none;
}

.nav {
  display: flex;
  gap: 16px;
}

.nav-link {
  color: rgb(255, 255, 255);
  text-decoration: none;
  font-weight: 600;
  transition: color 0.2s ease;
}

.nav-link:hover,
.nav-link.is-active {
  color: #ffffff;
}

.spacer {
  flex: 1;
}

.actions {
  display: flex;
  align-items: center;
  gap: 12px;
}

.greeting {
  color: rgba(255, 255, 255, 0.75);
  font-weight: 600;
}

.ghost-button {
  border: 1px solid rgba(255, 255, 255, 0.4);
  border-radius: 999px;
  padding: 8px 16px;
  background: transparent;
  color: white;
  cursor: pointer;
  font-weight: 600;
  text-decoration: none;
}

.ghost-button:hover {
  background: rgba(255, 255, 255, 0.1);
}

.avatar-mini-wrap { display: inline-flex; align-items: center; gap: 10px; text-decoration: none; }
.avatar-mini { width: 32px; height: 32px; border-radius: 999px; object-fit: cover; border: 1px solid #000000; }
</style>