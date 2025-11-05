<script setup>
import { computed, ref, onMounted, onUnmounted } from 'vue'
import { useWindowScroll } from '@vueuse/core'
import { RouterLink, useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const route = useRoute()
const auth = useAuthStore()

const { y } = useWindowScroll()
const isSticky = computed(() => y.value > 100)

/**
 * 判斷是否已登入
 */
const isLoggedIn = computed(() =>
  (auth.isAuthenticated?.value === true) ||
  !!auth.state?.accessToken
)

/** 目前頁面的完整路徑（登入後可導回） */
const currentPath = computed(() => route.fullPath)

/** 顯示名稱：優先顯示 profile.name，其次 email */
const displayName = computed(() =>
  auth.state?.profile?.name ||
  auth.state?.profile?.email ||
  '已登入'
)

/** 用戶頭像 URL */
const userAvatarUrl = computed(() => {
  return auth.state?.profile?.avatarUrl || auth.state?.profile?.avatar || null
})

/** 用戶姓名縮寫（當沒有頭像時使用） */
const userInitials = computed(() => {
  const name = auth.state?.profile?.name
  if (name) {
    return name.split(' ').map(n => n[0]).join('').toUpperCase().slice(0, 2)
  }
  const email = auth.state?.profile?.email
  if (email) {
    return email[0].toUpperCase()
  }
  return 'U'
})

/** 儀表板路由名稱 */
const dashboardRouteName = 'dashboard'

/** 控制用戶下拉選單的顯示狀態 */
const showUserDropdown = ref(false)

/** 切換下拉選單 */
const toggleUserDropdown = () => {
  showUserDropdown.value = !showUserDropdown.value
}

/** 關閉下拉選單 */
const closeUserDropdown = () => {
  showUserDropdown.value = false
}

/** 點擊外部關閉下拉選單 */
const handleClickOutside = (event) => {
  const container = event.target.closest('.user-dropdown-container')
  if (!container && showUserDropdown.value) {
    closeUserDropdown()
  }
}

/** 掛載時添加全局點擊監聽器 */
onMounted(() => {
  document.addEventListener('click', handleClickOutside)
})

/** 卸載時移除全局點擊監聽器 */
onUnmounted(() => {
  document.removeEventListener('click', handleClickOutside)
})

/** 登出流程 */
const onLogout = async () => {
  try {
    // 1) 優先呼叫 store 的 logout（若有）
    if (typeof auth.logout === 'function') {
      await auth.logout()
    } else {
      // 2) Fallback：清除 token 與使用者資料
      localStorage.removeItem('access_token')
      localStorage.removeItem('refresh_token')

      if (auth.state) {
        Object.assign(auth.state, {
          accessToken: '',
          refreshToken: '',
          profile: null,
          permissions: [],
          roles: [],
        })
      }

      // 3) 若 isAuthenticated 是 ref，安全地設為 false
      if (isRef(auth.isAuthenticated)) {
        auth.isAuthenticated.value = false
      }
    }
  } catch (err) {
    console.warn('登出時發生錯誤：', err)
  } finally {
    router.push({ name: 'home' }) // 或 { name: 'login' }
  }
}
</script>

<template>
  <header class="main-header">
    <div class="container">
      <div class="header-flex">
        <!-- Logo -->
        <router-link to="/" class="logo-link">
          <img src="../../assets/images/AirNest_Logo.png" alt="AirNest Logo" class="logo">
        </router-link>

        <!-- Right Actions -->
        <div class="right-actions">
          <!-- 已登入：顯示用戶下拉選單 -->
          <template v-if="isLoggedIn">
            <div class="user-dropdown-container" @click.stop>
              <button
                class="user-dropdown-trigger"
                @click="toggleUserDropdown"
                :class="{ active: showUserDropdown }"
              >
                <div class="user-avatar">
                  <img
                    v-if="userAvatarUrl"
                    :src="userAvatarUrl"
                    :alt="displayName"
                    class="avatar-image"
                    @error="$event.target.style.display = 'none'"
                  />
                  <span v-else class="avatar-initials">{{ userInitials }}</span>
                </div>
                <span class="user-name">{{ displayName }}</span>
                <i class="fas fa-chevron-down dropdown-arrow"></i>
              </button>

              <!-- 下拉選單 -->
              <div class="user-dropdown-menu" v-show="showUserDropdown">
                <div class="dropdown-header">
                  <div class="user-info">
                    <div class="avatar-large">
                      <img
                        v-if="userAvatarUrl"
                        :src="userAvatarUrl"
                        :alt="displayName"
                        class="avatar-image-large"
                        @error="$event.target.style.display = 'none'"
                      />
                      <span v-else class="avatar-initials-large">{{ userInitials }}</span>
                    </div>
                    <div class="user-details">
                      <div class="name">{{ displayName }}</div>
                      <div class="email">{{ auth.state?.profile?.email || '' }}</div>
                    </div>
                  </div>
                </div>

                <nav class="dropdown-nav">
                  <!-- 個人管理 -->
                  <div class="nav-section">
                    <h6 class="section-title">個人中心</h6>
                    <router-link
                      to="/my-bookings"
                      class="dropdown-item"
                      @click="closeUserDropdown"
                    >
                      <i class="fas fa-calendar-check"></i>
                      <span>我的預訂</span>
                    </router-link>
                    <router-link
                      to="/my-orders"
                      class="dropdown-item"
                      @click="closeUserDropdown"
                      v-if="auth.can && auth.can('Booking.ManageAll')"
                    >
                      <i class="fas fa-clipboard-list"></i>
                      <span>我的訂單</span>
                    </router-link>
                    <router-link
                      to="/coupons"
                      class="dropdown-item"
                      @click="closeUserDropdown"
                    >
                      <i class="fas fa-ticket-alt"></i>
                      <span>優惠券</span>
                    </router-link>
                  </div>

                  <div class="dropdown-divider"></div>

                  <!-- 其他功能（預留擴展） -->
                  <div class="nav-section">
                    <h6 class="section-title">其他</h6>
                    <!-- 預留位置：可後續新增其他功能 -->
                    <router-link
                      to="/profile"
                      class="dropdown-item"
                      @click="closeUserDropdown"
                    >
                      <i class="fas fa-user-cog"></i>
                      <span>個人設定</span>
                    </router-link>
                  </div>

                  <div class="dropdown-divider"></div>

                  <!-- 登出 -->
                  <div class="nav-section">
                    <button
                      class="dropdown-item logout-item"
                      @click="onLogout"
                    >
                      <i class="fas fa-sign-out-alt"></i>
                      <span>登出</span>
                    </button>
                  </div>
                </nav>
              </div>
            </div>
          </template>

          <!-- 未登入：顯示登入 / 註冊 -->
          <template v-else>
            <RouterLink
              class="btn btn-outline-light btn-sm"
              :to="{ name: 'login', query: { redirect: currentPath } }"
            >
              登入
            </RouterLink>

            <RouterLink
              class="btn btn-light btn-sm"
              :to="{ name: 'register', query: { redirect: currentPath } }"
            >
              註冊
            </RouterLink>
          </template>
        </div>
      </div>
    </div>
  </header>
</template>

<style lang="scss" scoped>
.main-header {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  z-index: 1000;
  background: linear-gradient(135deg, #1a1a1a 0%, #2d2d2d 100%);
  color: white;
  box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
  padding: 12px 0;

  .container {
    max-width: 100%;
    margin: 0 auto;
    padding: 0 40px;
  }

  .header-flex {
    display: flex;
    align-items: center;
    justify-content: space-between;
  }
}


.logo-link {
  display: block;
  transition: transform 0.2s ease;

  .logo {
    height: 48px;
    max-width: 180px;
    object-fit: contain;
  }
}

.main-nav {
  .nav-menu {
    display: flex;
    list-style: none;
    margin: 0;
    padding: 0;
    gap: 30px;

    .nav-link {
      color: rgba(255, 255, 255, 0.9);
      text-decoration: none;
      font-weight: 500;
      font-size: 16px;
      padding: 8px 0;
      transition: all 0.2s ease;
      position: relative;

      &:hover,
      &.active {
        color: #BE9A78;
      }

      &::after {
        content: '';
        position: absolute;
        bottom: -2px;
        left: 0;
        width: 0;
        height: 2px;
        background: #BE9A78;
        transition: width 0.3s ease;
      }

      &:hover::after,
      &.active::after {
        width: 100%;
      }
    }
  }
}

.right-actions {
  display: flex;
  align-items: center;
  gap: 15px;

  .login-link {
    color: rgba(255, 255, 255, 0.9);
    text-decoration: none;
    display: flex;
    align-items: center;
    gap: 6px;
    padding: 6px 12px;
    border-radius: 15px;
    transition: all 0.2s ease;
    font-size: 14px;

    &:hover {
      background: rgba(255, 255, 255, 0.1);
      color: white;
    }

    i {
      font-size: 12px;
    }
  }

  .search-btn {
    width: 36px;
    height: 36px;
    border: none;
    background: rgba(255, 255, 255, 0.1);
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    cursor: pointer;
    transition: all 0.2s ease;
    color: rgba(255, 255, 255, 0.9);

    &:hover {
      background: #BE9A78;
      color: white;
    }

    i {
      font-size: 14px;
    }
  }

  .mobile-menu-btn {
    display: none;
    flex-direction: column;
    width: 24px;
    height: 20px;
    background: transparent;
    border: none;
    cursor: pointer;
    gap: 4px;
    align-items: center;
    justify-content: center;

    span {
      width: 20px;
      height: 2px;
      background: rgba(255, 255, 255, 0.9);
      border-radius: 1px;
      transition: all 0.3s ease;
    }

    &:hover span {
      background: #BE9A78;
    }
  }
}

@media (min-width: 1920px) {
  .main-header .container {
    padding: 0 60px;
  }
}

@media (max-width: 1024px) {
  .main-nav {
    display: none;
  }

  .right-actions {
    .mobile-menu-btn {
      display: flex;
    }
  }
}

@media (max-width: 768px) {
  .main-header {
    padding: 10px 0;

    .container {
      padding: 0 15px;
    }
  }

  .logo-link .logo {
    height: 40px;
  }

  .right-actions {
    gap: 10px;

    .login-link {
      font-size: 12px;
      padding: 4px 8px;
    }

    .search-btn {
      width: 32px;
      height: 32px;
    }
  }
}

@media (max-width: 480px) {
  .main-header {
    .header-flex {
      gap: 10px;
    }
  }

  .right-actions {
    .login-link {
      span {
        display: none;
      }
    }
  }
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

/* 用戶下拉選單樣式 */
.user-dropdown-container {
  position: relative;
  display: inline-block;
}

.user-dropdown-trigger {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 8px 12px;
  background: rgba(255, 255, 255, 0.1);
  border: 1px solid rgba(255, 255, 255, 0.2);
  border-radius: 25px;
  color: white;
  cursor: pointer;
  transition: all 0.3s ease;
  font-size: 14px;
  font-weight: 500;

  &:hover,
  &.active {
    background: rgba(255, 255, 255, 0.15);
    border-color: rgba(255, 255, 255, 0.3);
    transform: translateY(-1px);
  }

  .user-avatar {
    width: 28px;
    height: 28px;
    background: #BE9A78;
    border-radius: 50%;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 12px;
    color: white;
    overflow: hidden;
    position: relative;

    .avatar-image {
      width: 100%;
      height: 100%;
      object-fit: cover;
      border-radius: 50%;
    }

    .avatar-initials {
      font-weight: 600;
      font-size: 11px;
    }
  }

  .user-name {
    max-width: 120px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }
}

.user-dropdown-menu {
  position: absolute;
  top: 100%;
  right: 0;
  width: 280px;
  background: white;
  border-radius: 12px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.15);
  border: 1px solid #e0e0e0;
  margin-top: 8px;
  z-index: 1100;
  overflow: hidden;
  animation: dropdownFadeIn 0.2s ease-out;
}

@keyframes dropdownFadeIn {
  from {
    opacity: 0;
    transform: translateY(-10px) scale(0.95);
  }
  to {
    opacity: 1;
    transform: translateY(0) scale(1);
  }
}

.dropdown-header {
  padding: 16px;
  background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
  border-bottom: 1px solid #e0e0e0;

  .user-info {
    display: flex;
    align-items: center;
    gap: 12px;

    .avatar-large {
      width: 40px;
      height: 40px;
      background: #BE9A78;
      border-radius: 50%;
      display: flex;
      align-items: center;
      justify-content: center;
      color: white;
      font-size: 16px;
      overflow: hidden;
      position: relative;

      .avatar-image-large {
        width: 100%;
        height: 100%;
        object-fit: cover;
        border-radius: 50%;
      }

      .avatar-initials-large {
        font-weight: 600;
        font-size: 14px;
      }
    }

    .user-details {
      flex: 1;
      min-width: 0;

      .name {
        font-weight: 600;
        font-size: 14px;
        color: #333;
        margin-bottom: 2px;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
      }

      .email {
        font-size: 12px;
        color: #666;
        overflow: hidden;
        text-overflow: ellipsis;
        white-space: nowrap;
      }
    }
  }
}

.dropdown-nav {
  padding: 8px 0;

  .nav-section {
    padding: 8px 0;

    .section-title {
      font-size: 11px;
      font-weight: 700;
      color: #888;
      text-transform: uppercase;
      letter-spacing: 0.5px;
      margin: 0 0 8px 0;
      padding: 0 16px;
    }
  }

  .dropdown-item {
    display: flex;
    align-items: center;
    gap: 12px;
    padding: 10px 16px;
    color: #333;
    text-decoration: none;
    transition: all 0.2s ease;
    font-size: 14px;
    border: none;
    background: none;
    width: 100%;
    text-align: left;
    cursor: pointer;

    &:hover {
      background: #f8f9fa;
      color: #BE9A78;
      padding-left: 20px;
    }

    i {
      width: 16px;
      font-size: 14px;
      color: #666;
      transition: color 0.2s ease;
    }

    &:hover i {
      color: #BE9A78;
    }

    span {
      font-weight: 500;
    }

    &.logout-item {
      color: #dc3545;

      &:hover {
        background: #fff5f5;
        color: #dc3545;
      }

      i {
        color: #dc3545;
      }
    }
  }
}

.dropdown-divider {
  height: 1px;
  background: #e0e0e0;
  margin: 8px 16px;
}

/* 響應式設計 */
@media (max-width: 768px) {
  .user-dropdown-trigger {
    padding: 6px 10px;
    font-size: 13px;

    .user-name {
      max-width: 80px;
    }

    .user-avatar {
      width: 24px;
      height: 24px;
      font-size: 10px;

      .avatar-initials {
        font-size: 9px;
      }
    }
  }

  .user-dropdown-menu {
    width: 260px;
    right: -10px;
  }
}

@media (max-width: 480px) {
  .user-dropdown-trigger {
    .user-name {
      display: none;
    }
  }

  .user-dropdown-menu {
    width: 240px;
    right: -20px;
  }
}
</style>
