<template>
  <div class="page-container">
    <section class="section">
      <header class="section-header">
        <div>
          <h1>權限管理</h1>
          <p>依角色調整可使用的系統功能。一次可選擇多個權限進行指派或收回。</p>
        </div>
        <button class="ghost-button" type="button" :disabled="isLoading" @click="loadData">重新整理</button>
      </header>

      <div class="card">
        <h2>選擇角色</h2>
        <label class="role-select">
          <span>角色</span>
          <select v-model="selectedRoleId">
            <option :value="null" disabled>請選擇要管理的角色</option>
            <option v-for="role in roles" :key="role.id" :value="role.id">{{ role.name }}</option>
          </select>
        </label>
      </div>

      <div class="card">
        <header class="card-header">
          <h2>權限清單</h2>
          <span v-if="selectedPermissions.length" class="selected-count">
            已選擇 {{ selectedPermissions.length }} 項
          </span>
        </header>

        <div v-if="isLoading" class="placeholder">載入中...</div>
        <div v-else-if="loadError" class="error-box">{{ loadError }}</div>
        <div v-else class="permissions-grid">
          <div v-for="(group, category) in groupedPermissions" :key="category" class="permission-group">
            <h3>{{ category }}</h3>
            <ul>
              <li v-for="permission in group" :key="permission.id">
                <label>
                  <input
                    v-model="selectedPermissions"
                    type="checkbox"
                    :value="permission.id"
                    :disabled="!selectedRoleId"
                  />
                  <span class="badge perm-code">{{ formatCode(permission.code) }}</span>
                  <!-- <span class="perm-name">{{ permission.displayName }}</span> -->
                </label>
              </li>
            </ul>
          </div>
        </div>
      </div>

      <div class="card actions">
        <h2>套用變更</h2>
        <p>選擇角色後勾選權限，可進行指派或收回操作。</p>
        <div class="button-row">
          <button class="primary-button" type="button" :disabled="!canSubmit" @click="submit('assign')">
            指派權限
          </button>
          <button class="danger-button" type="button" :disabled="!canSubmit" @click="submit('remove')">
            收回權限
          </button>
        </div>
        <p v-if="actionMessage" class="status" :class="statusClass">{{ actionMessage }}</p>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref, watch } from 'vue'
import { useAuthStore } from '@/stores/auth'
import type { PermissionSummary, RoleSummary } from '@/types/auth'
import http from '@/services/http'

const auth = useAuthStore()

const roles = ref<RoleSummary[]>([])
const permissions = ref<PermissionSummary[]>([])
const isLoading = ref(true)
const loadError = ref('')

const selectedRoleId = ref<number | null>(null)
const selectedPermissions = ref<number[]>([])
const ownedPermissionIds = ref<number[]>([])
const actionMessage = ref('')
const actionType = ref<'success' | 'error' | ''>('')

const statusClass = computed(() => (actionType.value === 'success' ? 'status--success' : 'status--error'))

const groupedPermissions = computed(() => {
  return permissions.value.reduce<Record<string, PermissionSummary[]>>((acc, perm) => {
    const key = perm.category || '其他'
    if (!acc[key]) acc[key] = []
    acc[key].push(perm)
    return acc
  }, {})
})

async function fetchOwnedPermissionIds(roleId: number) {
  ownedPermissionIds.value = []
  if (!roleId || roleId <= 0) return
  const { data } = await http.get<number[]>(`/Permissions/roles/${roleId}`)
  ownedPermissionIds.value = Array.isArray(data) ? data : []
}

// 放在 <script setup> 內
function formatCode(code?: string): string {
  return (code ?? '').replace(/\./g, '.\u200b')  // 在每個 . 後插入零寬空白
}

function syncOwnedIntoSelected() {
  // 把目前角色已擁有的權限，直接塞進你原本的勾選陣列
  selectedPermissions.value = [...ownedPermissionIds.value]
}

watch(selectedRoleId, async (rid: number | null) => {
  // 切換角色時先清空
  ownedPermissionIds.value = []
  // 沒選角色就不打 API，並清空你原本的勾選模型
  if (!rid) { 
    syncOwnedIntoSelected()  // 讓畫面也清掉勾選
    return
  }
  try {
    await fetchOwnedPermissionIds(rid)
    syncOwnedIntoSelected()
  } catch (err) {
    // 這裡不要 throw，避免 Vue 提示 unhandled watcher error
    console.warn('讀取角色權限失敗', err)
  }
})

const canSubmit = computed(() => !!selectedRoleId.value && selectedPermissions.value.length > 0)

const loadData = async () => {
  isLoading.value = true
  loadError.value = ''
  try {
    const [roleList, permissionList] = await Promise.all([auth.getRoles(), auth.getPermissions()])
    roles.value = roleList
    permissions.value = permissionList
  } catch (error) {
    console.error('載入資料失敗', error)
    loadError.value = auth.state.error ?? '無法載入資料，請稍後再試'
  } finally {
    isLoading.value = false
  }
}

const submit = async (action: 'assign' | 'remove') => {
  if (!selectedRoleId.value || selectedPermissions.value.length === 0) return
  actionMessage.value = ''
  actionType.value = ''
  try {
    await auth.updateRolePermissions(selectedRoleId.value, selectedPermissions.value, action)
    actionMessage.value = action === 'assign' ? '已成功指派權限' : '已成功收回權限'
    actionType.value = 'success'
    selectedPermissions.value = []
  } catch (error) {
    console.error('更新權限失敗', error)
    actionMessage.value = auth.state.error ?? '操作失敗，請確認是否具有權限'
    actionType.value = 'error'
  }
}

onMounted(() => {
  loadData()
})
</script>

<style scoped>
.page-container {
  padding: 32px 24px;
}

.section {
  max-width: 1080px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.section-header {
  display: flex;
  justify-content: space-between;
  flex-wrap: wrap;
  gap: 16px;
  align-items: center;
}

.section-header h1 {
  margin: 0;
  font-size: 1.9rem;
  color: #111827;
}

.section-header p {
  margin: 4px 0 0;
  color: #6b7280;
}

.card {
  background: #ffffff;
  border-radius: 18px;
  padding: 24px;
  box-shadow: 0 16px 42px rgba(15, 23, 42, 0.08);
  display: flex;
  flex-direction: column;
  gap: 18px;
}

.card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.placeholder {
  color: #6b7280;
}

.error-box {
  padding: 12px 16px;
  border-radius: 12px;
  background: #fee2e2;
  color: #b91c1c;
}

.role-select span {
  display: block;
  font-weight: 600;
  color: #6b7280;
  margin-bottom: 6px;
}

.role-select select {
  width: 100%;
  padding: 12px 14px;
  border-radius: 10px;
  border: 1px solid #d1d5db;
  font-size: 1rem;
}

.permissions-grid {
  display: grid;
  gap: 20px;
  grid-template-columns: repeat(auto-fit, minmax(260px, 1fr));
}

.permission-group {
  background: #f9fafb;
  border-radius: 14px;
  padding: 16px;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.permission-group h3 {
  margin: 0;
  font-size: 1.05rem;
  color: #1f2937;
}

.permission-group ul {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.permission-group label {
  display: flex;
  align-items: center;
  gap: 10px;
  font-weight: 600;
  color: #374151;
}

.permission-group input[type='checkbox'] {
  width: 18px;
  height: 18px;
}

.perm-code {
  display:inline-block;
  max-width:100%;
  white-space:normal;
  overflow-wrap:anywhere;
  line-height:1.25;
  font-family: 'Fira Code', 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace;
  background: #eef2ff;
  color: #3730a3;
  padding: 2px 6px;
  border-radius: 6px;
}

.perm-name {
  color: #4b5563;
}

.actions {
  gap: 16px;
}

.button-row {
  display: flex;
  gap: 12px;
  flex-wrap: wrap;
}

.primary-button,
.danger-button,
.ghost-button {
  border-radius: 999px;
  padding: 12px 18px;
  font-weight: 600;
  cursor: pointer;
  border: none;
  transition: transform 0.2s ease, filter 0.2s ease;
}

.primary-button {
  background: linear-gradient(135deg, #2563eb, #7c3aed);
  color: #ffffff;
}

.primary-button:disabled,
.danger-button:disabled,
.ghost-button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.danger-button {
  background: #fee2e2;
  color: #b91c1c;
}

.ghost-button {
  background: transparent;
  border: 1px solid rgba(59, 130, 246, 0.4);
  color: #2563eb;
}

.selected-count {
  color: #2563eb;
  font-weight: 600;
}

.status {
  font-weight: 600;
}

.status--success {
  color: #047857;
}

.status--error {
  color: #dc2626;
}
</style>