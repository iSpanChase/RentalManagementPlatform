<template>
  <div class="container py-4">
    <h2 class="mb-3">待審核的系統管理員</h2>

    <div v-if="loading">讀取中…</div>
    <div v-else-if="rows.length === 0" class="text-muted">目前沒有待審核的帳號。</div>

    <table v-else class="table table-striped align-middle">
      <thead>
        <tr>
          <th>使用者</th>
          <th>Email</th>
          <th>建立時間</th>
          <th style="width:160px;"></th>
        </tr>
      </thead>
      <tbody>
        <tr v-for="u in rows" :key="u.userId">
          <td>{{ u.name || u.username || '(未填)' }}</td>
          <td>{{ u.email }}</td>
          <td>{{ formatTime(u.createdAt) }}</td>
          <td>
            <button class="btn btn-primary btn-sm" @click="approve(u.userId)" :disabled="approvingId===u.userId">
              {{ approvingId===u.userId ? '核准中…' : '核准為系統管理員' }}
            </button>
          </td>
        </tr>
      </tbody>
    </table>

    <div v-if="msg" class="mt-3 text-danger">{{ msg }}</div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import http from '@/services/http'   // 你現有的 axios 實例（baseURL=/api）

type Row = { userId:number; email:string; name?:string; username?:string; createdAt?:string }

const rows = ref<Row[]>([])
const loading = ref(false)
const approvingId = ref<number|null>(null)
const msg = ref('')

function formatTime(s?: string) {
  if (!s) return ''
  const d = new Date(s)
  return isNaN(d.getTime()) ? s : d.toLocaleString()
}

async function load() {
  msg.value = ''
  loading.value = true
  try {
    const { data } = await http.get<Row[]>('/Admin/operators/pending')   // GET 清單
    rows.value = Array.isArray(data) ? data : []
  } catch (e: any) {
    msg.value = e?.response?.data?.message || e?.message || '讀取失敗'
  } finally {
    loading.value = false
  }
}

async function approve(userId: number) {
  msg.value = ''
  approvingId.value = userId
  try {
    await http.post(`/Admin/users/${userId}/approve-operator`)          // POST 核准
    rows.value = rows.value.filter(x => x.userId !== userId)
  } catch (e: any) {
    msg.value = e?.response?.data?.message || e?.message || '核准失敗'
  } finally {
    approvingId.value = null
  }
}

onMounted(load)
</script>
