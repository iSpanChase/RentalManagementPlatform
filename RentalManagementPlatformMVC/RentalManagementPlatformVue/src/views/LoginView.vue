<template>
  <div class="auth-card">
    <h1 class="auth-title">登入系統</h1>
    <p class="auth-subtitle">請輸入帳號密碼以繼續使用系統功能。</p>

    <form class="auth-form" @submit.prevent="handleSubmit">
      <label class="form-field">
        <span>電子郵件</span>
        <input v-model.trim="email" type="email" required placeholder="name@example.com" />
      </label>

      <label class="form-field">
        <span>密碼</span>
        <input v-model="password" type="password" required placeholder="請輸入密碼" />
      </label>
      <div class="login-links">
        <RouterLink to="/forgot-password" class="link">忘記密碼？</RouterLink>
        <span class="divider"></span>
        <RouterLink to="/register" class="link">建立帳號</RouterLink>
      </div>

      <div v-if="errorMsg" class="error-box">{{ errorMsg }}</div>

        <button
          type="submit"
          class="primary-button"
          :disabled="loading"
        >
          {{ loading ? '登入中…' : '登入' }}
        </button>
    </form>
  </div>
</template>

<script setup lang="ts">
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
</script>

<style scoped>
.auth-card {
  max-width: 420px;
  margin: 48px auto;
  padding: 32px;
  border-radius: 16px;
  box-shadow: 0 16px 48px rgba(15, 23, 42, 0.12);
  background: #ffffff;
}

.auth-title {
  font-size: 1.75rem;
  font-weight: 700;
  color: #1f2937;
  margin-bottom: 8px;
}

.auth-subtitle {
  margin-bottom: 24px;
  color: #6b7280;
  font-size: 0.95rem;
}

.auth-form {
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.form-field {
  display: flex;
  flex-direction: column;
  gap: 8px;
  font-weight: 600;
  color: #374151;
  font-size: 0.95rem;
}

.form-field input {
  border-radius: 10px;
  border: 1px solid #d1d5db;
  padding: 12px 14px;
  transition: border 0.2s ease, box-shadow 0.2s ease;
  font-size: 1rem;
}

.form-field input:focus {
  outline: none;
  border-color: #2563eb;
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.15);
}

.primary-button {
  border: none;
  padding: 12px 16px;
  border-radius: 999px;
  background: linear-gradient(135deg, #2563eb, #7c3aed);
  color: white;
  font-weight: 600;
  letter-spacing: 0.5px;
  cursor: pointer;
  transition: filter 0.2s ease, transform 0.2s ease;
}

.primary-button:hover:not(:disabled) {
  filter: brightness(1.05);
  transform: translateY(-1px);
}

.primary-button:disabled {
  opacity: 0.7;
  cursor: progress;
}

.error-box {
  padding: 10px 12px;
  border-radius: 8px;
  background: #fee2e2;
  color: #b91c1c;
  font-size: 0.9rem;
}

.login-links {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-top: 8px;
}
.login-links .link {
  color: #2563eb;
  font-weight: 600;
  text-decoration: none;
}
.login-links .link:hover { text-decoration: underline; }
.login-links .divider { color: #94a3b8; }
</style>