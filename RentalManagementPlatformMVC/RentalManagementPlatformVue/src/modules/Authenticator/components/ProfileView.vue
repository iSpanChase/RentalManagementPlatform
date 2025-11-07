<template>
  <div class="page-container">
    <section class="form-section">
      <header>
        <h1>個人基本資料</h1>
        <p>更新您的姓名與聯絡電話。電子郵件與帳號會做為登入識別，不提供修改，*為必填。</p>
      </header>

      <form class="profile-form" @submit.prevent="handleSubmit">
        <div class="form-grid">
          <!-- 隱藏/唯讀識別欄位 -->
          <input type="hidden" :value="form.userId" />

          <label class="form-item">
            <span>帳號</span>
            <input v-model="form.username" type="text" disabled />
          </label>

          <label class="form-item">
            <span>電子郵件</span>
            <input v-model="form.email" type="email" disabled />
          </label>

          <label class="form-item">
            <span>姓名 <span class="req">*</span></span>
            <input v-model.trim="form.name" type="text" required />
          </label>

          <label class="form-item">
            <span>性別 <span class="req">*</span></span>
            <select v-model="form.gender" class="select-like-input select-with-caret" required>
              <option value="">請選擇</option>
              <option value="男性">男性</option>
              <option value="女性">女性</option>
              <option value="其他">其他</option>
            </select>
          </label>

          <label class="form-item">
            <span>生日 <span class="req">*</span></span>
            <input v-model="form.birthDateInput" type="date" :max="todayStr" required />
          </label>

          <label class="form-item">
            <span>電話</span>
            <input v-model.trim="form.phone" type="tel" />
          </label>

          <label class="form-item form-item--full">
            <span>地址 <span class="req">*</span></span>
            <input v-model.trim="form.address" type="text" required />
          </label>

          <label class="form-item">
            <span>點數</span>
            <input v-model.number="form.point" type="number" min="0" />
          </label>

          <label class="form-item form-item--full">
            <span>大頭貼 URL（選填）</span>
            <input v-model.trim="form.profileImageUrl" type="url" placeholder="https://..." />
          </label>

          <div class="avatar-preview" v-if="previewUrl">
            <img :src="previewUrl" alt="avatar preview" class="avatar-preview-img" @error="onPreviewError" />
            <button type="button" class="outline-button" @click="clearAvatar">清除大頭貼</button>
          </div>

          <div class="form-item form-item--checkbox">
            <label class="checkbox">
              <input type="checkbox" :checked="isVerified" disabled />
              <span>帳號驗證（Email）</span>
            </label>
            <small :class="isVerified ? 'status--success' : 'status--error'">
              {{ isVerified ? '已通過驗證' : '尚未通過驗證' }}
            </small>
          </div>
          
          <div v-if="auth.state.profile && !isVerified" class="form-item form-item--checkbox verify-banner">
              <div class="msg">
                您的 Email 尚未完成驗證，部分功能可能受限。請至信箱點擊驗證連結，或重寄驗證信。
              </div>
              <div class="actions">
                <router-link class="link" :to="{ name: 'VerifyEmailView', query: { email: auth.state.profile.email } }">前往驗證頁</router-link>
              <button
                type="button"
                @click="onResendVerification"
                :disabled="resendPending || !isValidEmail"
              >
                {{ resendPending ? '寄送中…' : '重寄驗證信' }}
              </button>
              </div>
              <div v-if="resentOnce" class="hint">已送出（若帳號不存在或已驗證，系統不會顯示更多資訊）。</div>
            </div>
        </div>

        <footer class="form-footer">
          <button class="primary-button" :disabled="isSaving">
            {{ isSaving ? '儲存中…' : '儲存' }}
          </button>
          <span v-if="statusMessage" class="status" :class="statusClass" role="status">
            {{ statusMessage }}
          </span>
        </footer>
      </form>
      <!-- 已更新彈窗 -->
        <div v-if="showSavedDialog" class="modal-backdrop">
          <div class="modal-card">
            <h3>資料已更新</h3>
            <p>您的個人資料已成功儲存。</p>
            <div class="modal-actions">
              <button class="btn btn-primary" @click="closeSavedDialog">停在此頁</button>
              <button class="btn btn-outline" @click="goHomeFromSaved">回到首頁</button>
            </div>
          </div>
        </div>

        <!-- 未儲存離頁確認 -->
        <div v-if="showLeaveConfirm" class="modal-backdrop">
          <div class="modal-card">
            <h3>尚未儲存變更</h3>
            <p>您對個人資料做了修改尚未儲存，確定要離開此頁嗎？</p>
            <div class="modal-actions">
              <button class="btn btn-primary" @click="stayHere">留在此頁</button>
              <button class="btn btn-danger" @click="leavePage">離開此頁</button>
            </div>
          </div>
        </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref, computed, watchEffect, onMounted, onBeforeUnmount, watch } from 'vue'
import { useRouter, onBeforeRouteLeave } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import http from '@/services/http'

const router = useRouter()

const auth = useAuthStore()
const resendPending = ref(false)
const resentOnce = ref(false)
const email = computed(() => auth.state.profile?.email ?? '')
const isValidEmail = computed(() => !!email.value && /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.value))
const today = new Date(); today.setHours(0,0,0,0);
const todayStr = new Date().toISOString().slice(0,10);

/** 儲存成功彈窗 / 離頁確認彈窗 */
const showSavedDialog = ref(false)
const showLeaveConfirm = ref(false)

/** 導航守衛用：要前往的下一個路由 & 是否略過一次守衛 */
const pendingTo = ref<any>(null)
const bypassGuardOnce = ref(false)

const onResendVerification = async () => {
  if (!isValidEmail.value) return
  resendPending.value = true
  try {
    await auth.resendVerification(email.value) // ← 改用 store 方法
    resentOnce.value = true
  } finally {
    resendPending.value = false
  }
}

const previewUrl = computed(() => {
  const src = (form.profileImageUrl || '').trim()
  if (src) return src
  // fallback 跟 Dashboard 一致
  const name = form.name || form.username || 'User'
  return `https://ui-avatars.com/api/?name=${encodeURIComponent(name)}&background=C7D2FE&color=111827&size=128&rounded=true`
})

function onPreviewError(e: Event) {
  const name = form.name || form.username || 'User'
  ;(e.target as HTMLImageElement).src =
    `https://ui-avatars.com/api/?name=${encodeURIComponent(name)}&background=C7D2FE&color=111827&size=128&rounded=true`
}

function clearAvatar() {
  form.profileImageUrl = ''
}



/** 表單模型（與後端/DB 欄位對齊；birthDate 用 input 綁定字串 yyyy-MM-dd） */
type ProfileForm = {
  userId: number
  username: string
  email: string
  name: string
  gender: string          // 男性/女性/其他（若後端要英文再轉換）
  birthDateInput: string  // yyyy-MM-dd
  phone: string
  address: string
  point: number | null
  profileImageUrl: string
}

const form = reactive<ProfileForm>({
  userId: 0,
  username: '',
  email: '',
  name: '',
  gender: '',
  birthDateInput: '',
  phone: '',
  address: '',
  point: null,
  profileImageUrl: '',
})

const isSaving = ref(false)
const statusMessage = ref('')
const statusType = ref<'success' | 'error' | ''>('')

/** 表單原始快照與是否髒值 */
const originalSnapshot = ref('')
const isDirty = ref(false)
const snapshotForm = () => JSON.stringify(form)
const refreshSnapshot = () => {
  originalSnapshot.value = snapshotForm()
  isDirty.value = false
}

/** 深度監看表單，只要任何欄位變動就標記為髒 */
watch(
  () => form,
  () => { isDirty.value = snapshotForm() !== originalSnapshot.value },
  { deep: true }
)

/** 顯示用：Email 驗證狀態（唯讀） */
const isVerified = computed(() => {
  const p: any = auth.state.profile || {}
   // 兼容多種命名：isVerified / isverified / emailConfirmed / isEmailVerified / is_verified
   return Boolean(
     p.isVerified      // 小駝峰
     ?? p.IsVerified   // 大駝峰（有些 DTO 會這樣》
     ?? p.isverified   // ✅ 後端目前就是這個
     ?? p.is_verified  // 蛇形
     ?? p.emailConfirmed
     ?? p.isEmailVerified
     ?? false
   )
 })

onMounted(async () => {
  // 若目前快取顯示未驗證，嘗試拉一次最新資料
  await auth.fetchProfile?.()
})

/** 進頁/更新後，把 store.profile 映射到表單 */
watch(
  () => auth.state.profile,
  (p) => { if (p) fillFromProfile() },
  { immediate: true }
)

function fillFromProfile() {
  const p: any = auth.state.profile || {}

  form.userId = Number(p.userId ?? p.id ?? 0)
  form.username = String(p.username ?? '')
  form.email = String(p.email ?? '')
  form.name = String(p.name ?? '')
  form.gender = (p.gender ?? '').toString()
  form.phone = String(p.phone ?? '')
  form.address = String(p.address ?? '')
  form.point = p.point === null || p.point === undefined ? null : Number(p.point)
  form.profileImageUrl = String(p.profileImageUrl ?? p.profile_imageurl ?? p.avatarUrl ?? '')

  // 後端可能回 ISO / Date / 其他可解析字串 → 統一轉 yyyy-MM-dd 給 <input type="date">
  const ymd = toYmdString(p.birthDate ?? p.birth_date)
  form.birthDateInput = ymd ?? ''
  refreshSnapshot()
}

/** 任意可解析日期 → yyyy-MM-dd；失敗回 null */
function toYmdString(v: any): string | null {
  if (!v) return null
  const d = v instanceof Date ? v : new Date(String(v))
  if (Number.isNaN(d.getTime())) return null
  const yyyy = d.getFullYear()
  const mm = String(d.getMonth() + 1).padStart(2, '0')
  const dd = String(d.getDate()).padStart(2, '0')
  return `${yyyy}-${mm}-${dd}`
}

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

  // 檢查 yyyy-MM-dd 是否是未來日期（以本地時區的日界線比較）
  function isFutureYmd(ymd: string): boolean {
    const d = new Date(ymd + 'T00:00:00')
    const today = new Date()
    today.setHours(0, 0, 0, 0)
    return d.getTime() > today.getTime()
  }

const statusClass = computed(() =>
  statusType.value === 'success' ? 'status--success' : 'status--error',
)

/** 送出更新（後端要 yyyy-MM-dd） */
const handleSubmit = async () => {
  statusMessage.value = ''
  statusType.value = ''
  isSaving.value = true

  try {
    const missing: string[] = []
    const name = form.name?.trim()
    const gender = form.gender?.trim()
    const address = form.address?.trim()
    const birthYmd = normalizeYmd(form.birthDateInput)

    // ✅ 先擋未來日期（有選生日才檢查）
    if (birthYmd && isFutureYmd(birthYmd)) {
      throw new Error('生日不可晚於今天')
    }
    if (!name) missing.push('姓名')
    if (!gender) missing.push('性別')
    if (!birthYmd) missing.push('生日')
    if (!address) missing.push('地址')
    if (missing.length) {
      throw new Error(`請完整填寫：${missing.join(' / ')}`)
    }

    await auth.updateProfile({
      name,
      gender,
      birthDate: birthYmd!,                    // ✅ 直接送 yyyy-MM-dd
      phone: form.phone?.trim() || null,      // NULLABLE
      address,
      point: form.point ?? null,              // NULLABLE
      profileImageUrl: form.profileImageUrl?.trim() || null, // NULLABLE
    })

    fillFromProfile()
    refreshSnapshot()  // 標記為已儲存（關閉髒值）
    statusMessage.value = '已成功更新個人資料'
    statusType.value = 'success'
    showSavedDialog.value = true  // ← 打開「資料已更新」彈窗
  } catch (error: any) {
    statusMessage.value = error?.message || auth.state.error || '更新失敗，請稍候再試'
    statusType.value = 'error'
  } finally {
    isSaving.value = false
  }
}

/** 元件內的路由守衛：若有未儲存變更，攔截並跳出彈窗 */
onBeforeRouteLeave((to, from, next) => {
  if (bypassGuardOnce.value) {        // 按下「離開此頁」時放行一次
    bypassGuardOnce.value = false
    next()
    return
  }
  if (isDirty.value) {
    pendingTo.value = to
    showLeaveConfirm.value = true
    next(false)                       // 先攔截，等使用者選擇
  } else {
    next()
  }
})

/** 瀏覽器重新整理或關閉分頁時的提醒 */
const onBeforeUnload = (e: BeforeUnloadEvent) => {
  if (isDirty.value) {
    e.preventDefault()
    e.returnValue = ''  // 讓瀏覽器顯示預設提示
  }
}
onMounted(() => window.addEventListener('beforeunload', onBeforeUnload))
onBeforeUnmount(() => window.removeEventListener('beforeunload', onBeforeUnload))

/** 彈窗按鈕動作 */
const stayHere = () => {
  showLeaveConfirm.value = false
  pendingTo.value = null
}
const leavePage = async () => {
  showLeaveConfirm.value = false
  bypassGuardOnce.value = true
  const target = pendingTo.value
  pendingTo.value = null
  if (target) await router.push(target)
}

const goHomeFromSaved = async () => {
  showSavedDialog.value = false
  bypassGuardOnce.value = true      // 若剛好又變髒，放行一次
  await router.push({ path: '/' })  // 你的首頁路由如有名稱可換成 { name: 'Home' }
}
const closeSavedDialog = () => { showSavedDialog.value = false }

// 若進頁尚未有 profile，補抓一次（不影響 UI 互動）
if (!auth.state.profile) {
  auth.fetchProfile().then(fillFromProfile).catch(() => {})
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

.verify-banner{border:1px solid #fde68a;background:#fffbeb;color:#92400e;border-radius:10px;padding:12px;margin-bottom:16px}
.actions{margin-top:8px;display:flex;gap:12px;align-items:center}
.link{color:#2563eb}
.hint{margin-top:6px;font-size:13px;color:#6b7280}

/* 彈窗樣式 */
.modal-backdrop{
  position: fixed; inset: 0;
  background: rgba(0,0,0,.45);
  display: grid; place-items: center;
  z-index: 9999;
}
.modal-card{
  width: min(520px, 92vw);
  background: #fff;
  border-radius: 14px;
  box-shadow: 0 20px 50px rgba(0,0,0,.25);
  padding: 22px 20px;
}
.modal-card h3{ margin: 0 0 8px; font-size: 18px; }
.modal-card p{ margin: 0 0 16px; color: #4b5563; }

.modal-actions{
  display: flex; gap: 10px; justify-content: flex-end;
}

.btn{
  border: 1px solid transparent;
  border-radius: 10px;
  padding: 8px 14px;
  font-weight: 600;
  cursor: pointer;
}
.btn-primary{ background:#2563eb; color:#fff; }
.btn-primary:hover{ filter: brightness(1.05); }
.btn-outline{ background:#fff; color:#2563eb; border-color:#93c5fd; }
.btn-outline:hover{ background:#f1f5f9; }
.btn-danger{ background:#ef4444; color:#fff; }
.btn-danger:hover{ filter: brightness(1.05); }

/* 必填星號 */
.req {
  color: #ff0000; /* Tailwind 的 red-500 色調 */
  margin-left: 4px;
  font-weight: 700;
}

/* 讓性別 <select> 看起來跟 input 一樣 */
.select-like-input {
  display: block;
  width: 100%;
  padding: 8px 12px;
  border: 1px solid #d1d5db;      /* gray-300 */
  border-radius: 8px;
  background: #fff;
  line-height: 1.5;
  outline: none;
  appearance: none;                /* 移除原生外觀，維持一致 */
}

/* 加入內嵌 SVG 當作下拉箭頭 */
.select-with-caret{
  /* 預留箭頭空間 */
  padding-right: 36px;

  background-repeat: no-repeat;
  background-position: right 12px center; /* 箭頭位置 */
  background-size: 14px 14px;
  background-image: url("data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' viewBox='0 0 20 20' fill='none' stroke='%236b7280' stroke-width='2' stroke-linecap='round' stroke-linejoin='round'%3E%3Cpath d='M6 8l4 4 4-4'/%3E%3C/svg%3E");
  /* ↑ 灰色(#6b7280)小箭頭，無需外部檔案 */
}

.select-like-input:focus {
  border-color: #2563eb;           /* blue-600 */
  box-shadow: 0 0 0 3px rgba(37,99,235,.15);
}

</style>