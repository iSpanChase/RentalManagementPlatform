<template>
  <div class="auth-container">
    <div class="auth-card">
      <h1 class="card-title">登入系統</h1>
      <p class="card-subtitle">請輸入帳號密碼以繼續使用系統功能。</p>

      <form class="auth-form" @submit.prevent="handleSubmit">
        <div class="form-group">
          <label for="email">電子郵件</label>
          <input id="email" v-model.trim="email" type="email" required placeholder="name@example.com" class="form-control" />
        </div>

        <div class="form-group">
          <label for="password">密碼</label>
          <input id="password" v-model="password" type="password" required placeholder="請輸入密碼" class="form-control" />
        </div>

        <div class="form-links">
          <RouterLink to="/forgot-password" class="link">忘記密碼？</RouterLink>
          <RouterLink to="/register" class="link">建立帳號</RouterLink>
        </div>

        <div v-if="errorMsg" class="error-box">{{ errorMsg }}</div>

        <button type="submit" class="btn btn-primary" :disabled="loading">
          <span v-if="loading">登入中…</span>
          <span v-else>登入</span>
        </button>
      </form>
      <div>
        <div class="oauth-btns">
          <button class="oauth-btn oauth-btn--google mt-2" type="button" @click="loginWith('google')">
            <img :src="googlePng" alt="Google Logo" width="18" height="18" />
              <span class="oauth-btn__label">Google 登入</span>
          </button>
          <button class="oauth-btn oauth-btn--line" type="button" @click="loginWith('line')">
            <img :src="linePng" alt="Line Logo" width="18" height="18" />
              <span class="oauth-btn__label">LINE 登入</span>
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import googlePng from '@/assets/images/GoogleIcon.png'
import linePng from '@/assets/images/LINEIcon.png'
import { ref } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { RouterLink } from 'vue-router'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()

const email = ref('')
const password = ref('')
const loading = ref(false)
const errorMsg = ref('')

const handleSubmit = async () => {
  if (loading.value) return
  errorMsg.value = ''

  if (!email.value || !password.value) {
    errorMsg.value = '請輸入 Email 與密碼'
    return
  }

  loading.value = true
  try {
    // 1) 呼叫 store 的登入（會自動 setSession + 帶上 Authorization）
    const res = await auth.login({ email: email.value, password: password.value })

    // 2) 若後端沒有在登入回傳 profile，就補抓一次
    if (!auth.state.profile) {
      try {
        await auth.fetchProfile()
      } catch {
        // 抓不到也不阻擋導頁
      }
    }

    // 3) 導頁（預設 /）
    const redirect = (route.query.redirect as string) || '/'
    await router.replace(redirect)
  } catch (err: any) {
    errorMsg.value =
      err?.response?.data?.message ||
      err?.response?.data?.title ||
      err?.message ||
      '登入失敗，請確認帳號或密碼'
  } finally {
    loading.value = false
  }
}
const API_BASE   = import.meta.env.VITE_API_BASE_URL || 'https://localhost:7230/api'
const RETURN_URL = '/auth/callback' // 或 import.meta.env.VITE_OAUTH_RETURN_URL

function loginWith(provider: 'google' | 'line') {
  const redirect = encodeURIComponent(RETURN_URL + (location.search || ''))
  window.location.href = `${API_BASE}/auth/oauth/${provider}/challenge?returnUrl=${redirect}`
}
</script>

<style lang="scss" scoped>
@use "sass:color";

// 參考 Booking 模組的 SASS 變數
$primary-color: #222;
$secondary-color: #008489;
$danger-color: #d9534f;
$border-color: #ebebeb;
$background-light: #f9f9f9;
$text-light: #717171;
$text-dark: #484848;
$primary-brand-color: #007bff; // 假設一個品牌主色

.auth-container {
  display: flex;
  justify-content: center;
  align-items: flex-start;
  padding-top: 48px;
  padding-bottom: 48px;
  width: 100%;
}

.auth-card {
  width: 100%;
  max-width: 450px;
  background: white;
  border: 1px solid $border-color;
  border-radius: 16px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.06);
  padding: 32px 40px;
}

.card-title {
  font-size: 28px;
  font-weight: bold;
  color: $primary-color;
  margin-bottom: 8px;
  text-align: center;
}

.card-subtitle {
  font-size: 16px;
  color: $text-light;
  margin-bottom: 32px;
  text-align: center;
}

.auth-form {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;

  label {
    font-weight: 600;
    color: $text-dark;
    font-size: 14px;
  }
}

.form-control {
  width: 100%;
  padding: 12px 16px;
  border-radius: 8px;
  border: 1px solid #ccc;
  font-size: 16px;
  transition: border-color 0.2s, box-shadow 0.2s;

  &:focus {
    outline: none;
    border-color: $primary-brand-color;
    box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.2);
  }
}

.btn {
  padding: 10px 16px;
  border-radius: 8px;
  border: 1px solid transparent;
  font-weight: 600;
  font-size: 16px;
  cursor: pointer;
  transition: all 0.2s;
  width: 100%;
}

.btn-primary {
  background-color: $secondary-color;
  color: white;
  border-color: $secondary-color;

  &:hover:not(:disabled) {
    background-color: color.adjust($secondary-color, $lightness: -10%);
    border-color: color.adjust($secondary-color, $lightness: -10%);
  }

  &:disabled {
    background-color: #ccc;
    border-color: #ccc;
    cursor: not-allowed;
  }
}

.error-box {
  padding: 12px 16px;
  border-radius: 8px;
  background: #f8d7da;
  color: #721c24;
  font-size: 14px;
  border: 1px solid #f5c6cb;
}

.form-links {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 8px;

  .link {
    color: $primary-brand-color;
    font-weight: 500;
    text-decoration: none;
    font-size: 14px;

    &:hover {
      text-decoration: underline;
    }
  }
}

.oauth-btns {
  display: grid;
  gap: 10px;
}

/* 基礎按鈕樣式 */
.oauth-btn {
  width: 100%;
  display: inline-flex;
  align-items: center;
  justify-content: center;
  gap: 10px;
  padding: 10px 14px;
  border-radius: 6px;
  font-size: 14px;
  line-height: 20px;
  font-weight: 600;
  transition: background-color .15s ease, box-shadow .15s ease, border-color .15s ease, transform .02s ease-in-out;
  user-select: none;
  cursor: pointer;
  border: 1px solid transparent;
}

/* Google 樣式（依常見規範：白底、灰邊、深灰字） */
.oauth-btn--google {
  background-color: #ffffff;
  color: #3c4043;
  border-color: #dadce0;
}
.oauth-btn--google:hover {
  background-color: #f7f8f8;
  border-color: #d0d1d3;
}
.oauth-btn--google:active {
  transform: translateY(1px);
}
.oauth-btn--google:focus-visible {
  outline: none;
  box-shadow: 0 0 0 3px rgba(66,133,244,.25);
}

/* LINE 樣式（官方綠 #06C755、白字） */
.oauth-btn--line {
  background-color: #06C755;
  color: #ffffff;
  border-color: #06C755;
}
.oauth-btn--line:hover {
  background-color: #05b94f;
  border-color: #05b94f;
}
.oauth-btn--line:active {
  transform: translateY(1px);
}
.oauth-btn--line:focus-visible {
  outline: none;
  box-shadow: 0 0 0 3px rgba(6,199,85,.28);
}
</style>