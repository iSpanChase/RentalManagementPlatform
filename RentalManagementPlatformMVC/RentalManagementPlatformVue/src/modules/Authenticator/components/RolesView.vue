<template>
  <main class="page">
    <header class="page__header">
      <h1>角色管理</h1>
      <p class="muted">瀏覽系統角色、並以 Email 指派/收回角色。</p>
    </header>

    <!-- 警示/提示 -->
    <div v-if="errorMsg" class="alert alert-danger">{{ errorMsg }}</div>
    <div v-if="okMsg" class="alert alert-ok">{{ okMsg }}</div>

    <!-- 角色清單 -->
    <section class="card">
      <div class="card__head">
        <h3>角色清單</h3>
        <button class="btn" @click="loadRoles" :disabled="loading">重新整理</button>
      </div>

      <div v-if="loading" class="skeleton">載入中…</div>
      <table v-else class="table">
        <thead>
          <tr>
            <th style="width: 120px;">#</th>
            <th>角色名稱</th>
            <th>角色代碼</th>
          </tr>
        </thead>
        <tbody>
          <tr v-for="(r, idx) in roles" :key="r.roleId" @click="selectedRoleId = r.roleId" :class="{ selected: r.roleId === selectedRoleId }">
            <td>{{ idx + 1 }}</td>
            <td>{{ r.roleName }}</td>
            <td><code>{{ r.roleCode }}</code></td>
          </tr>
          <tr v-if="!roles.length">
            <td colspan="3" class="muted">尚無資料或無權限讀取。</td>
          </tr>
        </tbody>
      </table>
    </section>

    <!-- 指派 / 收回 -->
    <section class="card">
      <h3>指派 / 收回角色</h3>

      <div class="grid">
        <div class="field">
          <label>選擇角色</label>
          <select v-model="selectedRoleId">
            <option :value="null" disabled>請選擇角色</option>
            <option v-for="r in roles" :key="r.roleId" :value="r.roleId">
              {{ r.roleName }}（{{ r.roleCode }}）
            </option>
          </select>
        </div>

        <div class="field">
          <label>使用者 Email</label>
          <input
            v-model.trim="email"
            type="email"
            placeholder="輸入 user@example.com"
            autocomplete="email"
          />
        </div>
      </div>

      <div class="actions">
        <button
          class="btn primary"
          :disabled="loading || !can('Roles.Assign')"
          @click="assignRole"
        >
          指派角色
        </button>
        <button
          class="btn danger"
          :disabled="loading || !can('Roles.Assign')"
          @click="revokeRole"
        >
          收回角色
        </button>
      </div>

      <p class="muted" v-if="!can('Roles.Assign')">你目前沒有 <code>Roles.Assign</code> 權限，無法進行指派/收回。</p>
    </section>
  </main>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'
import http from '@/services/http'            // 既有的 Axios 實例
import { useAuthStore } from '@/stores/auth'  // 需要 can() / fetchAbilities() / state.profile

type RoleItem = {
  roleId: number
  roleName: string
  roleCode: string
}

const { can, fetchAbilities, state } = useAuthStore()

const roles = ref<RoleItem[]>([])
const selectedRoleId = ref<number | null>(null)
const email = ref('')

const loading = ref(false)
const errorMsg = ref('')
const okMsg = ref('')

const clearMsgLater = () => {
  window.setTimeout(() => {
    okMsg.value = ''
    errorMsg.value = ''
  }, 2000)
}

const loadRoles = async () => {
  loading.value = true
  errorMsg.value = ''
  try {
    const { data } = await http.get('/Roles')
    // ★ 正規化：兼容 RoleId/RoleName/RoleCode 或 roleId/roleName/roleCode
    const list = Array.isArray(data) ? data : []
    roles.value = list.map((r: any) => ({
      roleId:  r.roleId  ?? r.RoleId  ?? r.id  ?? r.ID  ?? null,
      roleName:r.roleName?? r.RoleName?? r.name?? r.Name?? '',
      roleCode:r.roleCode?? r.RoleCode?? r.code?? r.Code?? '',
    }))

    if (!roles.value.find(r => r.roleId === selectedRoleId.value)) {
      selectedRoleId.value = roles.value[0]?.roleId ?? null
    }
  } catch (err: any) {
    errorMsg.value = err?.response?.status === 403
      ? '沒有 Roles.View 權限，無法讀取角色清單'
      : `載入角色失敗：${err?.response?.data ?? err?.message ?? '未知錯誤'}`
  } finally {
    loading.value = false
  }
}

const assignRole = async () => {
  errorMsg.value = ''
  okMsg.value = ''
  if (!selectedRoleId.value) { errorMsg.value = '請先選擇角色'; return }
  if (!email.value) { errorMsg.value = '請輸入 Email'; return }
  if (!can('Roles.Assign')) { errorMsg.value = '你沒有 Roles.Assign 權限'; return }

  loading.value = true
  try {
    await http.post(`/Roles/${selectedRoleId.value}/users/by-email`, { email: email.value })
    okMsg.value = '指派成功'
    // 若是指派給自己 → 同步本地 abilities
    if (state.profile?.email && state.profile.email.toLowerCase() === email.value.toLowerCase()) {
      await fetchAbilities()
    }
  } catch (err: any) {
    errorMsg.value = err?.response?.status === 403
      ? '沒有 Roles.Assign 權限'
      : `指派失敗：${err?.response?.data ?? err?.message ?? '未知錯誤'}`
  } finally {
    loading.value = false
    clearMsgLater()
  }
}

const revokeRole = async () => {
  errorMsg.value = ''
  okMsg.value = ''
  if (!selectedRoleId.value) { errorMsg.value = '請先選擇角色'; return }
  if (!email.value) { errorMsg.value = '請輸入 Email'; return }
  if (!can('Roles.Assign')) { errorMsg.value = '你沒有 Roles.Assign 權限'; return }

  loading.value = true
  try {
    await http.delete(`/Roles/${selectedRoleId.value}/users/by-email`, {
      // Axios 的 DELETE 若要帶 body，要寫在 data
      data: { email: email.value }
    })
    okMsg.value = '收回成功'
    if (state.profile?.email && state.profile.email.toLowerCase() === email.value.toLowerCase()) {
      await fetchAbilities()
    }
  } catch (err: any) {
    errorMsg.value = err?.response?.status === 403
      ? '沒有 Roles.Assign 權限'
      : `收回失敗：${err?.response?.data ?? err?.message ?? '未知錯誤'}`
  } finally {
    loading.value = false
    clearMsgLater()
  }
}

onMounted(async () => {
  await loadRoles()
})
</script>

<style scoped>
.page { display: grid; gap: 16px; padding: 16px; }
.page__header { display: grid; gap: 6px; }
.muted { color: #6b7280; }
.alert { padding: 10px 12px; border-radius: 10px; }
.alert-danger { background: #fef2f2; border: 1px solid #fecaca; color: #991b1b; }
.alert-ok { background: #ecfdf5; border: 1px solid #a7f3d0; color: #065f46; }
.card { border: 1px solid #e5e7eb; border-radius: 12px; padding: 14px; background: #fff; display: grid; gap: 12px; }
.card__head { display: flex; align-items: center; justify-content: space-between; }
.table { width: 100%; border-collapse: collapse; }
.table th, .table td { padding: 10px 8px; border-bottom: 1px solid #eee; }
.table tr.selected { background: #f3f4f6; }
.grid { display: grid; grid-template-columns: repeat(2, minmax(0, 1fr)); gap: 12px; }
.field label { display: block; margin-bottom: 6px; color: #374151; }
.field input, .field select { width: 100%; padding: 8px 10px; border: 1px solid #d1d5db; border-radius: 8px; }
.actions { display: flex; gap: 10px; flex-wrap: wrap; }
.btn { border: 1px solid #d1d5db; padding: 8px 12px; border-radius: 10px; background: #fff; cursor: pointer; }
.btn.primary { background: #1f6feb; border-color: #1f6feb; color: #fff; }
.btn.danger { background: #dc2626; border-color: #dc2626; color: #fff; }
.skeleton { padding: 12px; color: #6b7280; }
</style>
