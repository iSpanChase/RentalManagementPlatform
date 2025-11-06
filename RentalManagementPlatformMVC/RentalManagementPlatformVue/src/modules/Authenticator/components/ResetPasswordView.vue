<template>
  <div class="page-container">
    <section class="form-section">
      <header>
        <h1>重設密碼</h1>
        <p>請輸入新密碼完成重設。</p>
      </header>

      <form class="profile-form" @submit.prevent="handleSubmit" novalidate>
        <div class="form-grid">
          <label class="form-item">
            <span>電子郵件</span>
            <input v-model.trim="email" type="email" required autocomplete="email" readonly />
          </label>

          <label class="form-item">
            <span>新密碼</span>
            <div class="password-field">
              <input
                :type="showPwd ? 'text' : 'password'"
                v-model="password"
                required
                minlength="6"
                autocomplete="new-password"
                placeholder="至少 6 碼"
              />
              <button type="button" class="toggle-visibility" @click="showPwd = !showPwd">
                {{ showPwd ? '隱藏' : '顯示' }}
              </button>
            </div>
          </label>

          <label class="form-item">
            <span>確認新密碼</span>
            <div class="password-field">
              <input
                :type="showPwd ? 'text' : 'password'"
                v-model="confirm"
                required
                minlength="6"
                autocomplete="new-password"
                placeholder="再次輸入密碼"
              />
              <button type="button" class="toggle-visibility" @click="showPwd = !showPwd">
                {{ showPwd ? '隱藏' : '顯示' }}
              </button>
            </div>
          </label>
        </div>

        <footer class="form-footer">
            <button
                class="primary-button"
                :disabled="loading"
                @click="handleSubmit"
            >
                重設密碼
            </button>
          <RouterLink to="/login" class="link" style="margin-left:12px;">返回登入</RouterLink>
        </footer>

            <!-- 訊息 -->
        <p v-if="errorMsg" class="mt-4 text-red-600 text-sm">{{ errorMsg }}</p>
        <p v-if="okMsg" class="mt-4 text-green-600 text-sm">{{ okMsg }}</p>
      </form>
    </section>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref, onMounted } from 'vue'
import { useRoute, useRouter, RouterLink } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import http from '@/services/http'

const route = useRoute()
const router = useRouter()
const auth = useAuthStore()

// ---- 每欄位獨立 ref ----
const email    = ref<string>('')
const token    = ref<string>('')   // 會在 onMounted 從 ?token= 取得
const password = ref<string>('')
const confirm  = ref<string>('')
const showPwd  = ref<boolean>(false)

const loading  = ref(false)
const errorMsg = ref('')
const okMsg    = ref('')

const PASSWORD_RULE = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$/;
const errors = reactive<{ password?: string }>({})

function validatePassword(pw: string) {
  if (!pw) return '請輸入新密碼';
  if (!PASSWORD_RULE.test(pw)) return '密碼需至少 8 碼，且包含英文大小寫與數字';
  return null;
}

async function onReset() {
  const err = validatePassword(password.value);
  if (err) { errors.password = err; return; }
  if (password.value !== confirm.value) {
    errors.password = '確認密碼與新密碼不一致';
    return;
  }
  errors.password = undefined;

  // 呼叫 POST /api/Auth/reset-password，帶 { token, newPassword, email }
}

/** 把整條 URL 或 "token=xxx&email=..." 等型態抽成純 JWT */
function normalizeToken(input: string | null): string {
  let raw = (input ?? '').trim()
  if (!raw) return ''
  // 完整 URL -> 取 query ?token=
  if (raw.startsWith('http')) {
    try {
      const url = new URL(raw)
      raw = (url.searchParams.get('token') ?? '').trim()
    } catch { /* ignore */ }
  }
  // 若包含 token=xxx
  const iEq = raw.toLowerCase().indexOf('token=')
  if (iEq >= 0) raw = raw.substring(iEq + 'token='.length)
  // 切除 & 後面的其他參數
  const iAmp = raw.indexOf('&')
  if (iAmp > 0) raw = raw.substring(0, iAmp)
  // 去頭尾引號
  return raw.replace(/^["']|["']$/g, '')
}

/** 從 JWT 取出 email（Base64Url 解 payload）。安全版：不假設 parts[1] 一定存在 */
function extractEmailFromJwt(jwt?: string): string {
  try {
    if (!jwt) return ''

    // 先安全地切開，不直接用 parts[1]
    const parts = jwt.split('.')
    const payloadPart = parts.length > 1 ? parts[1] : ''
    if (!payloadPart) return ''

    // Base64Url -> Base64
    let b64 = payloadPart.replace(/-/g, '+').replace(/_/g, '/')
    const padLen = b64.length % 4
    if (padLen) b64 += '='.repeat(4 - padLen)

    // 在瀏覽器環境使用 atob
    const payloadJson = typeof atob === 'function' ? atob(b64) : ''
    if (!payloadJson) return ''

    const payload = JSON.parse(payloadJson)

    // 常見 email claim
    return (
      payload?.email ??
      payload?.Email ??
      payload?.['http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress'] ??
      ''
    )
  } catch {
    return ''
  }
}


onMounted(() => {
  // 1) 取網址上的 token
  const qToken = (route.query.token as string | undefined) ?? ''
  token.value = normalizeToken(qToken)

  // 2) 先用網址上的 ?email= 帶入（如果有）
  const qEmail = (route.query.email as string | undefined) ?? ''
  if (qEmail && qEmail.trim()) {
    // decodeURIComponent 可防止網址有 %40 之類的編碼
    email.value = decodeURIComponent(qEmail.trim())
    return
  }

  // 3) 網址沒有 email，就從 JWT payload 自動帶入
  if (token.value) {
    const claimEmail = extractEmailFromJwt(token.value)
    if (claimEmail) email.value = claimEmail
  }
})

async function handleSubmit() {
  errorMsg.value = ''
  okMsg.value = ''

  const t = normalizeToken(token.value)
  if (!t) {
    errorMsg.value = '重設連結已失效，請重新操作「忘記密碼」。'
    return
  }
  const emailTrimmed = (email.value ?? '').trim()
  if (!emailTrimmed) {
    errorMsg.value = '請輸入電子郵件'
    return
  }
  if (!password.value) {
    errorMsg.value = '請輸入新密碼'
    return
  }
  if (password.value !== confirm.value) {
    errorMsg.value = '兩次輸入的新密碼不一致'
    return
  }

  loading.value = true
  try {
    await http.post('/Auth/reset-password', {
      token: t,                    // 純 JWT；後端也有 Normalize，雙重保險
      newPassword: password.value,
      email: emailTrimmed          // 一定帶上，做舊 token 的保底
    })
    okMsg.value = '已成功重設密碼，將返回登入頁…'
    setTimeout(() => router.push('/login'), 1200)
  } catch (err: any) {
    errorMsg.value = err?.response?.data?.message || '重設密碼失敗'
  } finally {
    loading.value = false
  }
}

</script>

<style scoped>
.page-container {
  padding: 32px 24px;
}

.form-section {
  max-width: 720px;
  margin: 0 auto;
  background: #ffffff;
  padding: 32px;
  border-radius: 18px;
  box-shadow: 0 16px 40px rgba(15, 23, 42, 0.08);
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.form-section header h1 {
  margin: 0 0 8px;
  font-size: 1.8rem;
  color: #111827;
}

.form-section header p {
  margin: 0;
  color: #6b7280;
}

.profile-form {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.form-grid {
  display: grid;
  gap: 20px;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
}

.form-item {
  display: flex;
  flex-direction: column;
  gap: 8px;
  color: #374151;
  font-weight: 600;
}

.form-item span {
  font-size: 0.9rem;
  color: #6b7280;
}

.form-item input {
  border-radius: 10px;
  border: 1px solid #d1d5db;
  padding: 12px 14px;
  font-size: 1rem;
  transition: border 0.2s ease, box-shadow 0.2s ease;
}

.form-item input:focus {
  outline: none;
  border-color: #2563eb;
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.15);
}

.form-item input:disabled {
  background: #f3f4f6;
  color: #6b7280;
}

.form-actions {
  display: flex;
  align-items: center;
  gap: 16px;
}

.primary-button {
  border: none;
  padding: 12px 22px;
  border-radius: 999px;
  background: linear-gradient(135deg, #2563eb, #7c3aed);
  color: white;
  font-weight: 600;
  cursor: pointer;
  transition: transform 0.2s ease, filter 0.2s ease;
}

.primary-button:hover:not(:disabled) {
  transform: translateY(-1px);
  filter: brightness(1.05);
}

.primary-button:disabled {
  opacity: 0.7;
  cursor: progress;
}

.status {
  font-size: 0.95rem;
  font-weight: 600;
}

.status--success {
  color: #059669;
}

.status--error {
  color: #dc2626;
}

.password-field {
  position: relative;
}
.password-field > input {
  /* 預留給右側按鈕的空間，依字數可微調 */
  padding-right: 66px;
}

.toggle-visibility {
  position: absolute;
  right: 8px;
  top: 50%;
  transform: translateY(-50%);
  border: none;
  background: transparent;
  padding: 6px 10px;
  border-radius: 8px;
  font-weight: 600;
  color: #2563eb;
  cursor: pointer;
}
.toggle-visibility:hover {
  background: #f1f5f9;
}

.avatar-preview { display: flex; align-items: center; gap: 12px; margin-top: 8px; }
.avatar-preview-img { width: 96px; height: 96px; border-radius: 12px; object-fit: cover; border: 1px solid #e5e7eb; }
.outline-button {
  border: 1px solid rgba(59, 130, 246, 0.4);
  border-radius: 999px;
  padding: 8px 14px;
  color: #2563eb;
  font-weight: 600;
  background: white;
  cursor: pointer;
}
.outline-button:hover { background: #f8fafc; }
</style>