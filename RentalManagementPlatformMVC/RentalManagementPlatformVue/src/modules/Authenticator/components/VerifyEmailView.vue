<!-- src/views/VerifyEmailView.vue -->
<template>
  <div class="verify-wrapper">
    <div class="card">
      <h1>電子郵件驗證</h1>

      <!-- 載入中 -->
      <div v-if="state === 'loading'" class="status loading">
        <p>正在驗證您的信箱，請稍候…</p>
      </div>

      <!-- 驗證成功 -->
      <div v-else-if="state === 'success'" class="status success">
        <p>✅ 您的 Email 已完成驗證！</p>
        <button @click="goLogin">前往登入</button>
      </div>

      <!-- 驗證失敗 -->
      <div v-else-if="state === 'error'" class="status error">
        <p>❌ 驗證連結無效或已過期。</p>
        <div class="resend">
          <label for="email">請輸入 Email 以重寄驗證信：</label>
          <input
            id="email"
            type="email"
            v-model.trim="resendEmail"
            placeholder="your@email.com"
            @keyup.enter="onResendVerification"
          />
          <button :disabled="resendPending || !isValidEmail" @click="onResendVerification">
            {{ resendPending ? '寄送中…' : '重寄驗證信' }}
          </button>
          <p v-if="resendDone" class="hint">
            若該 Email 未註冊或已驗證，系統不會顯示詳細資訊；請至信箱收信。
          </p>
        </div>
        <div class="actions">
          <button class="link" @click="goLogin">返回登入</button>
        </div>
      </div>

      <!-- 缺少參數（直接提供重寄） -->
      <div v-else class="status idle">
        <p>請點擊信件中的驗證連結完成驗證。</p>
        <div class="resend">
          <label for="email2">若沒收到信件，輸入 Email 以重寄：</label>
          <input
            id="email2"
            type="email"
            v-model.trim="resendEmail"
            placeholder="your@email.com"
            @keyup.enter="onResendVerification"
          />
          <button
            type="button"
            @click="onResendVerification"
            :disabled="resendPending || !isValidEmail"
          >
            {{ resendPending ? '寄送中…' : '重寄驗證信' }}
          </button>
          <p v-if="resendDone" class="hint">
            已送出（若該帳號已驗證或不存在，為避免洩漏也不會顯示更多資訊）。
          </p>
        </div>
        <div class="actions">
          <button class="link" @click="goLogin">返回登入</button>
        </div>
      </div>

      <!-- 如後端回傳訊息（選擇性顯示） -->
      <p v-if="message" class="server-msg">{{ message }}</p>
    </div>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import http from '@/services/http'
import { useAuthStore } from '@/stores/auth'

type State = 'idle' | 'loading' | 'success' | 'error'

const auth = useAuthStore()
const route = useRoute()
const router = useRouter()

const state = ref<State>('idle')
const message = ref<string>('')

const email = ref<string>((route.query.email as string) || '')
const token = ref<string>((route.query.token as string) || '')

const resendEmail = ref<string>(email.value || '')
const resendPending = ref<boolean>(false)
const resentOnce = ref(false)
const resendDone = ref<boolean>(false)

const isValidEmail = computed(() => !!email.value && /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.value))

const isSuccessPage = computed(() => route.path.endsWith('/success'))

onMounted(async () => {
    // 🚩 參數保險讀取（route.query 取不到時，改用 URLSearchParams）
  const q = new URLSearchParams(window.location.search);
  const qEmail = (route.query.email as string) || q.get('email') || '';
  const qToken = (route.query.token as string) || q.get('token') || '';
  // 若網址上帶有 email 與 token，直接嘗試驗證
  if (qEmail && qToken) {
    state.value = 'loading'
    try {
        await http.get('/Auth/verify-email', { params: { email: qEmail, token: qToken } });
    await auth.fetchProfile?.()                 
    if (auth.state?.profile) {
        (auth.state.profile as any).isVerified = true;
        (auth.state.profile as any).IsVerified = true;
        (auth.state.profile as any).isverified = true;
        (auth.state.profile as any).is_verified = true;
        (auth.state.profile as any).emailConfirmed = true;
    }
    router.replace({ name: 'Profile' })       // 或 { path: '/profile' }
    } catch (err: any) {
      // 後端可能回 400 並帶訊息
      message.value =
        err?.response?.data?.message ||
        '驗證連結無效或已過期，請重寄驗證信。'
      state.value = 'error'
    }
  } else {
    state.value = 'idle'
  }
})

const onResendVerification = async () => {
  if (!isValidEmail.value) return
  resendPending.value = true
  try {
    await auth.resendVerification(email.value) // 確保 store 有此方法（上一回我已提供最小補丁）
    resentOnce.value = true
  } catch (err) {
    // 可選：顯示錯誤訊息
    console.error('[resend-verification failed]', err)
  } finally {
    resendPending.value = false
  }
}

function goLogin() {
  router.push({ path: '/login' })
}
</script>

<style scoped>
.verify-wrapper {
  min-height: 70vh;
  display: grid;
  place-items: center;
  padding: 24px;
}
.card {
  width: min(560px, 92vw);
  border: 1px solid #e5e7eb;
  border-radius: 14px;
  padding: 24px 20px;
  box-shadow: 0 6px 24px rgba(0,0,0,.06);
  background: #fff;
}
h1 {
  margin: 0 0 12px;
  font-size: 20px;
}
.status { margin-top: 6px; line-height: 1.7; }
.status.success { color: #065f46; }
.status.error { color: #7f1d1d; }
.resend { margin-top: 12px; display: grid; gap: 8px; }
.resend input {
  padding: 10px 12px;
  border: 1px solid #d1d5db;
  border-radius: 8px;
  outline: none;
}
.resend input:focus { border-color: #3b82f6; box-shadow: 0 0 0 3px rgba(59,130,246,.15); }
button {
  padding: 10px 14px;
  border: none;
  border-radius: 8px;
  background: #111827;
  color: #fff;
  cursor: pointer;
}
button[disabled] { opacity: .6; cursor: not-allowed; }
.actions { margin-top: 12px; }
button.link {
  background: transparent;
  color: #2563eb;
  padding: 6px 0;
}
.hint { color: #6b7280; font-size: 13px; }
.server-msg { margin-top: 10px; color: #374151; font-size: 14px; }
</style>
