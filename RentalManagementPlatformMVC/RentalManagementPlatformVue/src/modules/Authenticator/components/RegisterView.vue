<template>
  <div class="auth-container">
    <div class="auth-card">
      <h1 class="card-title">建立帳號</h1>
      <p class="card-subtitle">請填寫您的基本資料以建立新帳號。</p>

      <form class="auth-form" @submit.prevent="handleSubmit" novalidate>
        <div class="form-grid">
          <div class="form-group">
            <label for="role">角色</label>
            <select id="role" v-model="form.roleCode" required class="form-control">
              <option disabled value="">請選擇</option>
              <option value="HOST">我是房東</option>
              <option value="TENANT">我是房客</option>
            </select>
          </div>

          <div class="form-group">
            <label for="name">姓名</label>
            <input id="name" v-model.trim="form.name" type="text" required class="form-control" />
          </div>

          <div class="form-group form-group-full">
            <label for="username">帳號 (Username)</label>
            <input id="username" v-model.trim="form.username" type="text" required class="form-control" />
          </div>

          <div class="form-group form-group-full">
            <label for="email">電子郵件</label>
            <input id="email" v-model.trim="form.email" type="email" required placeholder="name@example.com" autocomplete="email" class="form-control" />
          </div>

          <div class="form-group">
            <label for="gender">性別</label>
            <select id="gender" v-model="form.gender" required class="form-control">
              <option value="">請選擇</option>
              <option value="男性">男性</option>
              <option value="女性">女性</option>
              <option value="其他">其他</option>
            </select>
          </div>

          <div class="form-group">
            <label for="birthDate">生日</label>
            <input id="birthDate" v-model="form.birthDate" type="date" required class="form-control" />
          </div>

          <div class="form-group form-group-full">
            <label for="address">地址</label>
            <input id="address" v-model.trim="form.address" type="text" required class="form-control" />
          </div>

          <div class="form-group">
            <label for="password">密碼</label>
            <div class="input-group">
              <input :type="showPwd ? 'text' : 'password'" v-model="form.passwordHash" required minlength="6" autocomplete="new-password" placeholder="至少 6 碼" class="form-control" />
              <button type="button" class="btn-toggle-visibility" @click="showPwd = !showPwd">
                {{ showPwd ? '隱藏' : '顯示' }}
              </button>
            </div>
          </div>

          <div class="form-group">
            <label for="confirmPassword">確認密碼</label>
            <input id="confirmPassword" :type="showPwd ? 'text' : 'password'" v-model="form.confirmPassword" required minlength="6" autocomplete="new-password" placeholder="再次輸入密碼" class="form-control" />
          </div>
        </div>

        <div v-if="msg" :class="['status-box', ok ? 'status-success' : 'status-error']">
          {{ msg }}
        </div>

        <div class="form-footer">
          <button type="submit" class="btn btn-primary" :disabled="submitting">
            <span v-if="submitting">建立中…</span>
            <span v-else>建立帳號</span>
          </button>
          <RouterLink to="/login" class="link">已經有帳號了？返回登入</RouterLink>
        </div>
      </form>
    </div>
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
  birthDate: '',
  address: '',
  passwordHash: '',
  confirmPassword: '',
  phone: '',
  profileImageUrl: '',
})

const submitting = ref(false)
const msg = ref('')
const ok = ref(false)
const showPwd = ref(false)

function normalizeYmd(v: string | null | undefined): string | null {
  if (v == null) return null
  const s = String(v).trim()
  if (!s) return null
  const m = s.replace(/\//g, '-').match(/^(\d{4})-(\d{1,2})-(\d{1,2})$/)
  if (!m) return null
  const y = Number(m[1]), mo = Number(m[2]), d = Number(m[3])
  if (mo < 1 || mo > 12 || d < 1 || d > 31) return null
  const dt = new Date(Date.UTC(y, mo - 1, d))
  if (dt.getUTCFullYear() !== y || dt.getUTCMonth() + 1 !== mo || dt.getUTCDate() !== d) return null
  return `${String(y).padStart(4,'0')}-${String(mo).padStart(2,'0')}-${String(d).padStart(2,'0')}`
}

const handleSubmit = async () => {
  msg.value = ''
  ok.value = false

  const missing: string[] = []
  if (!form.roleCode) missing.push('角色')
  if (!form.name.trim()) missing.push('姓名')
  if (!form.username.trim()) missing.push('帳號')
  if (!form.email.trim()) missing.push('電子郵件')
  if (!form.gender) missing.push('性別')
  if (!form.address.trim()) missing.push('地址')
  const ymd = normalizeYmd(form.birthDate)
  if (!ymd) missing.push('生日')
  if (!form.passwordHash) missing.push('密碼')
  if (!form.confirmPassword) missing.push('確認密碼')

  if (missing.length) {
    msg.value = `請完整填寫：${missing.join('、')}`
    return
  }
  if (form.passwordHash.length < 6) {
    msg.value = '密碼至少 6 碼'
    return
  }
  if (form.passwordHash !== form.confirmPassword) {
    msg.value = '兩次密碼不一致'
    return
  }

  submitting.value = true
  try {
    await auth.register({
      roleCode: form.roleCode,
      email: form.email.trim(),
      passwordHash: form.passwordHash,
      name: form.name.trim(),
      username: form.username.trim(),
      phone: form.phone.trim() || '',
      gender: form.gender,
      birthDate: ymd!,
      address: form.address.trim(),
      profileImageUrl: form.profileImageUrl.trim() || undefined,
      companyName: '',
      taxId: '',
      nationalIdTail: '',
    })

    const emailForVerify = form.email.trim();
    // Redirect to a page that tells the user to check their email
    await router.push({ name: 'VerifyEmailView', query: { email: emailForVerify } });
    
  } catch (err: any) {
    ok.value = false
    const serverMsg = err?.response?.data?.message ??
      err?.response?.data?.title ??
      (typeof err?.response?.data === 'string' ? err.response.data : '') ??
      auth.state?.error ??
      err?.message ??
      '註冊失敗，請稍候再試'
    msg.value = serverMsg
    console.error('[Register failed]', err?.response?.data ?? err)
  } finally {
    submitting.value = false
  }
}
</script>

<style lang="scss" scoped>
// 統一 SASS 變數
$primary-color: #222;
$secondary-color: #008489;
$danger-color: #d9534f;
$border-color: #ebebeb;
$text-light: #717171;
$text-dark: #484848;
$primary-brand-color: #007bff;

.auth-container {
  display: flex;
  justify-content: center;
  align-items: flex-start;
  padding: 48px 24px;
  width: 100%;
}

.auth-card {
  width: 100%;
  max-width: 720px; 
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
  gap: 24px;
}

.form-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;

  &.form-group-full {
    grid-column: 1 / -1;
  }

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
  background-color: white; // 確保 select 有背景色
  -webkit-appearance: none; // 移除 iOS 上的原生樣式
  appearance: none;

  &:focus {
    outline: none;
    border-color: $primary-brand-color;
    box-shadow: 0 0 0 3px rgba(0, 123, 255, 0.2);
  }
}

select.form-control {
  background-image: url("data:image/svg+xml,%3csvg xmlns=\'http://www.w3.org/2000/svg\' viewBox=\'0 0 16 16\'/%3e%3cpath fill=\'none\' stroke=\'%23343a40\' stroke-linecap=\'round\' stroke-linejoin=\'round\' stroke-width=\'2\' d=\'M2 5l6 6 6-6\'/%3e%3c/svg%3e");
  background-repeat: no-repeat;
  background-position: right 0.75rem center;
  background-size: 16px 12px;
}


.input-group {
  position: relative;
  display: flex;

  .form-control {
    padding-right: 60px;
  }

  .btn-toggle-visibility {
    position: absolute;
    right: 1px;
    top: 1px;
    bottom: 1px;
    border: none;
    background: #f8f9fa;
    color: $text-dark;
    padding: 0 12px;
    border-radius: 0 8px 8px 0;
    cursor: pointer;
    font-size: 13px;

    &:hover {
      background: #e9ecef;
    }
  }
}

.btn {
  padding: 12px 16px;
  border-radius: 8px;
  border: 1px solid transparent;
  font-weight: 600;
  font-size: 16px;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-primary {
  background-color: $secondary-color;
  color: white;
  border-color: $secondary-color;
  width: 100%;

  &:hover:not(:disabled) {
    background-color: darken($secondary-color, 10%);
    border-color: darken($secondary-color, 10%);
  }

  &:disabled {
    background-color: #ccc;
    border-color: #ccc;
    cursor: not-allowed;
  }
}

.status-box {
  padding: 12px 16px;
  border-radius: 8px;
  font-size: 14px;
  border: 1px solid transparent;
}

.status-success {
  background-color: #d4edda;
  color: #155724;
  border-color: #c3e6cb;
}

.status-error {
  background-color: #f8d7da;
  color: #721c24;
  border-color: #f5c6cb;
}

.form-footer {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 16px;
  margin-top: 8px;
}

.link {
  color: $primary-brand-color;
  font-weight: 500;
  text-decoration: none;
  font-size: 14px;

  &:hover {
    text-decoration: underline;
  }
}

@media (max-width: 768px) {
  .form-grid {
    grid-template-columns: 1fr;
  }
  .auth-card {
    padding: 24px;
  }
}

</style>

