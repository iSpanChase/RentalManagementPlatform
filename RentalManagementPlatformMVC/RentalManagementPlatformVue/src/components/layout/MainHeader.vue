<script setup>
import { computed } from 'vue'
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
 * - 你的 store 先前有 isAuthenticated.value 與 state.accessToken
 * - 若命名不同，改這裡即可
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

/** 儀表板路由名稱（若你專案不是 'dashboard'，改成實際名稱） */
const dashboardRouteName = 'dashboard'

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

          <!-- Navigation
          <nav class="main-nav">
            <ul class="nav-menu">
              <li><router-link to="/" class="nav-link" active-class="active">首頁</router-link></li>
              <li><router-link to="/about" class="nav-link" active-class="active">關於我們</router-link></li>
              <li><router-link to="/rooms" class="nav-link" active-class="active">房源</router-link></li>
              <li><router-link to="/services" class="nav-link" active-class="active">服務</router-link></li>
              <li><router-link to="/contact" class="nav-link" active-class="active">聯繫我們</router-link></li>
            </ul>
          </nav> -->

        <!-- Right Actions -->
             <div class="ms-auto d-flex align-items-center gap-2">
      <!-- 已登入：顯示帳號 & 登出 -->
      <template v-if="isLoggedIn">
        <span class="small opacity-75">
          {{ displayName }}
        </span>

        <button
          type="button"
          class="btn btn-outline-light btn-sm"
          @click="onLogout"
        >
          登出
        </button>

        <!-- （可選）進入個人頁或儀表板 -->
        <RouterLink
          class="btn btn-teal btn-sm"
          :to="{ name: dashboardRouteName }"
        >
          儀表板
        </RouterLink>
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
</style>
