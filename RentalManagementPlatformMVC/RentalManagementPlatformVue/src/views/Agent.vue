<script setup lang="ts">
import { ref, onMounted } from 'vue'
import http from '@/plugins/http'
import ChatWindow from '@/components/ChatWindow.vue'

type TicketDto = { id: string; title: string; createdAt: string }
const agentName = ref('Agent1')
const tickets = ref<TicketDto[]>([])
const opened = ref<TicketDto[]>([]) // 當前打開的多個房

async function loadTickets() {
  const { data } = await http.get<TicketDto[]>('/api/supporttickets?status=open')
  tickets.value = data
}

function openTicket(t: TicketDto) {
  if (!opened.value.find(x => x.id === t.id)) opened.value.push(t)
}

function closePanel(id: string) {
  opened.value = opened.value.filter(x => x.id !== id)
}

onMounted(loadTickets)
</script>

<template>
  <div class="layout">
    <aside>
      <h4>待處理工單</h4>
      <ul>
        <li v-for="t in tickets" :key="t.id">
          <button @click="openTicket(t)">{{ t.title }}（{{ new Date(t.createdAt).toLocaleTimeString() }}）</button>
        </li>
      </ul>
    </aside>

    <main>
      <section v-for="t in opened" :key="t.id" class="panel">
        <header class="panel-head">
          <h4>{{ t.title }} <small>({{ t.id.slice(0,8) }})</small></h4>
          <button @click="closePanel(t.id)">關閉面板</button>
        </header>
        <ChatWindow :ticket-id="t.id" role="agent" :display-name="agentName" />
      </section>

      <p v-if="!opened.length" class="placeholder">請從左側選擇工單</p>
    </main>
  </div>
</template>

<style scoped>
.layout{ display:grid; grid-template-columns:280px 1fr; gap:16px }
aside{ border:1px solid #ddd; border-radius:8px; padding:12px }
.panel{ border:1px solid #ddd; border-radius:8px; padding:12px; margin-bottom:16px }
.panel-head{ display:flex; align-items:center; justify-content:space-between }
.placeholder{ border:1px dashed #bbb; border-radius:8px; padding:24px; text-align:center }
</style>
