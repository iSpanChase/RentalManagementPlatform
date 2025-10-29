<template>
  <div class="page-container">
    <section class="section">
      <header class="section-header">
        <div>
          <h1>角色管理</h1>
          <p>檢視系統中的角色，並可快速指派或收回指定使用者的角色。</p>
        </div>
        <button class="ghost-button" type="button" @click="loadRoles" :disabled="isLoading">
          重新整理
        </button>
      </header>

      <div class="card">
        <h2>角色清單</h2>
        <div v-if="isLoading" class="placeholder">載入中...</div>
        <div v-else-if="loadError" class="error-box">{{ loadError }}</div>
        <table v-else class="data-table">
          <thead>
            <tr>
              <th>角色名稱</th>
              <th>角色代碼</th>
              <th>角色 ID</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="role in roles" :key="role.id">
              <td>{{ role.name }}</td>
              <td><span class="code">{{ role.code }}</span></td>
              <td>{{ role.id }}</td>
            </tr>
          </tbody>
        </table>
      </div>

      <div class="card">
        <h2>指派 / 收回角色</h2>
        <form class="form-inline" @submit.prevent="assignRole('assign')">
          <label>
            <span>選擇角色</span>
            <select v-model="selectedRoleId" required>
              <option :value="null" disabled>請選擇角色</option>
              <option v-for="role in roles" :key="role.id" :value="role.id">{{ role.name }}</option>
            </select>
          </label>

          <label>
            <span>使用者 ID</span>
            <input v-model.number="targetUserId" type="number" min="1" placeholder="輸入使用者 ID" required />
          </label>

          <div class="button-group">
            <button class="primary-button" type="submit" :disabled="isProcessing">
              {{ isProcessing ? '處理中...' : '指派角色' }}
            </button>
            <button class="danger-button" type="button" :disabled="isProcessing" @click="assignRole('revoke')">
              收回角色
            </button>
          </div>
        </form>

        <p v-if="actionMessage" class="status" :class="statusClass">{{ actionMessage }}</p>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useAuthStore } from '@/stores/auth'
import type { RoleSummary } from '@/types/auth'

const auth = useAuthStore()

const roles = ref<RoleSummary[]>([])
const isLoading = ref(true)
const loadError = ref('')

const selectedRoleId = ref<number | null>(null)
const targetUserId = ref<number | null>(null)
const isProcessing = ref(false)
const actionMessage = ref('')
const actionType = ref<'success' | 'error' | ''>('')

const statusClass = computed(() => (actionType.value === 'success' ? 'status--success' : 'status--error'))

const loadRoles = async () => {
  isLoading.value = true
  loadError.value = ''
  try {
    roles.value = await auth.getRoles()
  } catch (error) {
    console.error('取得角色失敗', error)
    loadError.value = auth.state.error ?? '無法載入角色，請稍後再試'
  } finally {
    isLoading.value = false
  }
}

const assignRole = async (action: 'assign' | 'revoke') => {
  actionMessage.value = ''
  actionType.value = ''
  if (!selectedRoleId.value || !targetUserId.value) return

  isProcessing.value = true
  try {
    if (action === 'assign') {
      await auth.assignRoleToUser(selectedRoleId.value, targetUserId.value)
      actionMessage.value = '已成功指派角色'
    } else {
      await auth.revokeRoleFromUser(selectedRoleId.value, targetUserId.value)
      actionMessage.value = '已成功收回角色'
    }
    actionType.value = 'success'
  } catch (error) {
    console.error('更新角色失敗', error)
    actionMessage.value = auth.state.error ?? '操作失敗，請確認是否具有權限'
    actionType.value = 'error'
  } finally {
    isProcessing.value = false
  }
}

onMounted(() => {
  loadRoles()
})
</script>

<style scoped>
.page-container {
  padding: 32px 24px;
}

.section {
  max-width: 960px;
  margin: 0 auto;
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.section-header {
  display: flex;
  flex-wrap: wrap;
  justify-content: space-between;
  align-items: center;
  gap: 16px;
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
  box-shadow: 0 14px 38px rgba(15, 23, 42, 0.08);
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.card h2 {
  margin: 0;
  font-size: 1.3rem;
  color: #1f2937;
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

.data-table {
  width: 100%;
  border-collapse: collapse;
  overflow: hidden;
  border-radius: 14px;
}

.data-table thead {
  background: #f3f4f6;
  text-align: left;
}

.data-table th,
.data-table td {
  padding: 12px 16px;
  border-bottom: 1px solid #e5e7eb;
  font-size: 0.95rem;
}

.data-table tbody tr:hover {
  background: #f9fafb;
}

.code {
  font-family: 'Fira Code', 'SFMono-Regular', Consolas, 'Liberation Mono', Menlo, monospace;
  background: #eef2ff;
  color: #3730a3;
  padding: 4px 6px;
  border-radius: 6px;
}

.form-inline {
  display: grid;
  gap: 16px;
  grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
  align-items: end;
}

.form-inline label {
  display: flex;
  flex-direction: column;
  gap: 8px;
  font-weight: 600;
  color: #374151;
}

.form-inline select,
.form-inline input {
  padding: 12px 14px;
  border-radius: 10px;
  border: 1px solid #d1d5db;
  font-size: 1rem;
}

.button-group {
  display: flex;
  gap: 12px;
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
  color: white;
}

.primary-button:hover:not(:disabled) {
  transform: translateY(-1px);
  filter: brightness(1.05);
}

.danger-button {
  background: #fee2e2;
  color: #b91c1c;
}

.danger-button:hover:not(:disabled) {
  background: #fecaca;
}

.ghost-button {
  background: transparent;
  border: 1px solid rgba(59, 130, 246, 0.4);
  color: #2563eb;
}

.ghost-button:disabled {
  opacity: 0.6;
  cursor: progress;
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