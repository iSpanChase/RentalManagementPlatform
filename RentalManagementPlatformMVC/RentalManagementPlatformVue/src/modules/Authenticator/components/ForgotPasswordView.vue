<template>
  <div class="page-container">
    <section class="form-section">
      <header>
        <h1>重設密碼</h1>
        <p>請輸入您的電子郵件，我們會寄送重設密碼的連結到您的信箱。</p>
      </header>

      <form class="profile-form" @submit.prevent="handleSubmit" novalidate>
        <div class="form-grid">
          <label class="form-item">
            <span>電子郵件</span>
            <input
              v-model="email"
              type="email"
              class="text-input"
              placeholder="請輸入註冊的電子郵件"
              required
              aria-required="true"
            />
          </label>
        </div>

        <footer class="form-footer">
          <button class="primary-button" :disabled="submitting">
            {{ submitting ? '送出中…' : '寄送重設密碼信' }}
          </button>
          <RouterLink to="/login" class="link" style="margin-left:12px;">返回登入</RouterLink>
        </footer>

        <p v-if="msg" :class="ok ? 'status--success' : 'status--error'" style="margin-top:12px;">
          {{ msg }}
        </p>
      </form>
    </section>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { RouterLink } from 'vue-router'

const auth = useAuthStore()
const email = ref('')
const submitting = ref(false)
const msg = ref('')
const ok = ref(false)

const handleSubmit = async () => {
  msg.value = ''
  ok.value = false
  const v = email.value.trim()
  if (!v) {
    msg.value = '請輸入電子郵件'
    ok.value = false
    return
  }
  submitting.value = true
  try {
    await auth.forgotPassword(v)
    msg.value = '若此信箱存在，我們已寄出重設密碼的說明信。請至您的信箱收信。'
    ok.value = true
  } catch (err:any) {
    msg.value = auth.state?.error || err?.message || '寄送失敗，請稍後再試'
    ok.value = false
  } finally {
    submitting.value = false
  }
}
</script>

<style scoped>
/* 沿用你現有版型 class：page-container / form-section / profile-form / form-grid / form-item / primary-button */
.page-container { display: grid; place-items: center; padding: 40px 16px; background: #f8fafc; min-height: 50vh; }
.form-section { width: 100%; max-width: 560px; background: white; border-radius: 16px; padding: 24px; box-shadow: 0 10px 30px rgba(2, 6, 23, 0.08); }
.profile-form { display: flex; flex-direction: column; gap: 16px; }
.form-grid {   display: grid;  gap: 20px;  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr)); }
.form-item {   display: flex; flex-direction: column; gap: 8px; color: #374151; font-weight: 600; }
/* .form-item--full { grid-column: 1 / -1; } */
.form-footer { display: flex; align-items: center; gap: 10px; }
.primary-button { border: none; padding: 12px 20px; border-radius: 999px; background: linear-gradient(135deg, #2563eb, #7c3aed); color: white; font-weight: 600; cursor: pointer; }
.status--success { color: #059669; }
.status--error { color: #dc2626; }
.link { color: #2563eb; font-weight: 600; text-decoration: none; }
.link:hover { text-decoration: underline; }

.text-input{
  display: block;
  width: 100%;
  padding: 8px 12px;
  border: 1px solid #d1d5db;    /* gray-300 */
  border-radius: 8px;
  background: #fff;
  line-height: 1.5;
  outline: none;
}

.text-input::placeholder{
  color: rgb(180, 180, 180);                 /* gray-400 */
}

.text-input:focus{
  border-color: #2563eb;         /* blue-600 */
  box-shadow: 0 0 0 3px rgba(37,99,235,.15);
}
</style>
