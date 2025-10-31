<script setup lang="ts">
import { ref, onMounted } from 'vue'
import http from '@/plugins/http'
import ChatWindow from '@/components/ChatWindow.vue'

type TicketDto = { id: string; title: string; userName: string; status: string; createdAt: string; updatedAt: string }

const agentName = ref('Agent1')
const tickets = ref<TicketDto[]>([])
const current = ref<TicketDto | null>(null)

async function loadTickets() {
  const { data } = await http.get<TicketDto[]>('/api/supporttickets?status=open')
  tickets.value = data
}
function openTicket(t: TicketDto) { current.value = t }

onMounted(loadTickets)
</script>

<template>
  <div>
    <h2>客服後台（Agent）</h2>
    <div style="display:grid;grid-template-columns:280px 1fr;gap:16px;">
      <aside style="border:1px solid #ddd;border-radius:8px;padding:12px;">
        <h4>待處理工單</h4>
        <ul style="list-style:none;padding:0;margin:0;">
          <li v-for="t in tickets" :key="t.id" style="margin-bottom:8px;">
            <button @click="openTicket(t)" style="width:100%;">
              {{ t.title }}（{{ new Date(t.createdAt).toLocaleTimeString() }}）
            </button>
          </li>
        </ul>
      </aside>

      <main v-if="current">
        <h4>Ticket: {{ current.id }} - {{ current.title }}</h4>
        <ChatWindow :ticket-id="current.id" role="agent" :display-name="agentName" />
      </main>

      <main v-else style="display:grid;place-items:center;border:1px dashed #bbb;border-radius:8px;">
        <p>請從左側選擇一個工單</p>
      </main>
    </div>
  </div>
</template>
