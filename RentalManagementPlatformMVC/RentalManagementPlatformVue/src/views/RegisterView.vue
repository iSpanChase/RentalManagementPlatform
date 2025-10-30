<template>
  <div class="page-container">
    <section class="form-section">
      <header>
        <h1>建立帳號</h1>
        <p>請填寫您的基本資料以建立新帳號。</p>
      </header>

      <form class="profile-form" @submit.prevent="handleSubmit" novalidate>
        <div class="form-grid">
          <!-- 角色 -->
          <label class="form-item">
            <span>角色</span>
            <select v-model="form.roleCode" required>
              <option value="">請選擇</option>
              <option value="LANDLORD">房東</option>
              <option value="TENANT">房客</option>
              <option value="VENDOR">廠商</option>
              <option value="OPERATOR">系統管理員</option>
            </select>
          </label>

          <!-- 姓名 -->
          <label class="form-item">
            <span>姓名</span>
            <input v-model.trim="form.name" type="text" required />
          </label>

          <!-- 帳號（username） -->
          <label class="form-item">
            <span>帳號</span>
            <input v-model.trim="form.username" type="text" required />
          </label>

          <!-- Email -->
          <label class="form-item">
            <span>電子郵件</span>
            <input v-model.trim="form.email" type="email" required placeholder="name@example.com" autocomplete="email" />
          </label>

          <!-- 性別 -->
          <label class="form-item">
            <span>性別</span>
            <select v-model="form.gender" required>
              <option value="">請選擇</option>
              <option value="男性">男性</option>
              <option value="女性">女性</option>
              <option value="其他">其他</option>
            </select>
          </label>

          <!-- 生日 -->
          <label class="form-item">
            <span>生日</span>
            <input v-model="form.birthDate" type="date" required />
          </label>

          <!-- 地址 -->
          <label class="form-item form-item--full">
            <span>地址</span>
            <input v-model.trim="form.address" type="text" required />
          </label>

          <!-- 密碼 -->
            <label class="form-item">
            <span>密碼</span>
            <div class="password-field">
                <input
                :type="showPwd ? 'text' : 'password'"
                v-model="form.password"
                required
                minlength="6"
                autocomplete="new-password"
                placeholder="至少 6 碼"
                />
                <button
                type="button"
                class="toggle-visibility"
                @click="showPwd = !showPwd"
                :aria-pressed="showPwd"
                :title="showPwd ? '隱藏密碼' : '顯示密碼'"
                >
                {{ showPwd ? '隱藏' : '顯示' }}
                </button>
            </div>
            </label>

          <!-- 確認密碼 -->
          <label class="form-item">
            <span>確認密碼</span>
            <input
              :type="showPwd ? 'text' : 'password'"
              v-model="form.confirmPassword"
              required
              minlength="6"
              autocomplete="new-password"
              placeholder="再次輸入密碼"
            />
          </label>

          <!-- 電話（選填） -->
          <label class="form-item">
            <span>電話（選填）</span>
            <input v-model.trim="form.phone" type="tel" placeholder="0912-345-678" />
          </label>
        </div>

        <footer class="form-footer">
          <button class="primary-button" :disabled="submitting">
            {{ submitting ? '建立中…' : '建立帳號' }}
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
import { reactive, ref } from 'vue'
import { useRouter, RouterLink } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const auth = useAuthStore()

const form = reactive({
  roleCode: '',
  name: '',
  username: '',
  email: '',
  gender: '',
  birthDate: '',       // yyyy-MM-dd（<input type="date">）
  address: '',
  password: '',
  confirmPassword: '',
  phone: '',
  profileImageUrl: '',
})

const submitting = ref(false)
const msg = ref('')
const ok = ref(false)
const showPwd = ref(false)

/** 將輸入正規化為 'YYYY-MM-DD'；接受 'YYYY/MM/DD' 或 'YYYY-MM-DD'，月/日 1~2 位；無效回 null */
function normalizeYmd(v: string | null | undefined): string | null {
  if (v == null) return null
  const s = String(v).trim()
  if (!s) return null
  const m = s.replace(/\//g, '-').match(/^(\d{4})-(\d{1,2})-(\d{1,2})$/)
  if (!m) return null
  const y = Number(m[1]), mo = Number(m[2]), d = Number(m[3])
  if (mo < 1 || mo > 12 || d < 1 || d > 31) return null
  // 用 UTC 驗證日期真實存在（避免 2/30）
  const dt = new Date(Date.UTC(y, mo - 1, d))
  if (dt.getUTCFullYear() !== y || dt.getUTCMonth() + 1 !== mo || dt.getUTCDate() !== d) return null
  return `${String(y).padStart(4,'0')}-${String(mo).padStart(2,'0')}-${String(d).padStart(2,'0')}`
}

const handleSubmit = async () => {
  msg.value = ''
  ok.value = false

  // 基本檢核
  const missing: string[] = []
  if (!form.roleCode) missing.push('角色')
  if (!form.name.trim()) missing.push('姓名')
  if (!form.username.trim()) missing.push('帳號')
  if (!form.email.trim()) missing.push('電子郵件')
  if (!form.gender.trim()) missing.push('性別')
  if (!form.address.trim()) missing.push('地址')

  const ymd = normalizeYmd(form.birthDate)
  if (!ymd) missing.push('生日')

  if (!form.password) missing.push('密碼')
  if (!form.confirmPassword) missing.push('確認密碼')

  if (missing.length) {
    msg.value = `請完整填寫：${missing.join(' / ')}`
    return
  }
  if (form.password.length < 6) {
    msg.value = '密碼至少 6 碼'
    return
  }
  if (form.password !== form.confirmPassword) {
    msg.value = '兩次密碼不一致'
    return
  }
  if (form.roleCode === 'ADMIN') {
    msg.value = '此角色無法在註冊時選擇'
    return
  }

  submitting.value = true
  try {
    await auth.register({
      roleCode: form.roleCode,
      email: form.email.trim(),
      passwordHash: form.password, // 後端雜湊
      name: form.name.trim(),
      username: form.username.trim(),
      phone: form.phone.trim() || undefined,
      gender: form.gender.trim(),
      birthDate: ymd!,                 // yyyy-MM-dd
      address: form.address.trim(),
      profileImageUrl: form.profileImageUrl.trim() || undefined,
      companyName: null,
      taxId: null,
      nationalIdTail: null,
    })
  if (form.roleCode === 'OPERATOR') {
      ok.value = true
      msg.value = '已送出申請，待系統管理員審核通過後生效。'
      router.push({ path: '/login', query: { pending: 'operator' } })
    } else {
      ok.value = true
      msg.value = '帳號已建立，請使用新帳號登入。'
      router.push({ path: '/login', query: { registered: '1' } })
    }
  } catch (err: any) {
    ok.value = false
    msg.value = auth.state?.error || err?.message || '註冊失敗，請稍候再試'
  } finally {
    submitting.value = false
  }
}
</script>

<style scoped>
/* 與你現有風格一致 */
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

.link { color: #2563eb; font-weight: 600; text-decoration: none; }
.link:hover { text-decoration: underline; }
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

.link.small { font-size: 0.9rem; padding: 0; background: transparent; border: none; }
</style>
