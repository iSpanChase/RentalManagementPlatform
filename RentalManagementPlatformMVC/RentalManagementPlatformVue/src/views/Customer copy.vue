<script setup lang="ts">
import { ref } from 'vue'
import http from '../plugins/http'
import ChatWindow from '../components/ChatWindow.vue'

type TicketDto = { id: string; title: string; userName: string; status: string; createdAt: string; updatedAt: string }

const title = ref('房租問題')
const userName = ref('CustomerA')
const ticket = ref<TicketDto | null>(null)

async function createTicket() {
  const { data } = await http.post<TicketDto>('/api/supporttickets', { title: title.value, userName: userName.value })
  ticket.value = data
}
</script>

<template>
  <div>
    <h2>客服聊天室（客戶端）</h2>

    <div v-if="!ticket" style="display:grid;gap:8px;max-width:420px;">
      <input v-model="title" placeholder="問題標題" />
      <input v-model="userName" placeholder="顯示名稱" />
      <button @click="createTicket">建立工單並開始聊天</button>
    </div>

    <div v-else>
      <p><b>TicketId：</b>{{ ticket.id }}</p>
      <ChatWindow :ticket-id="ticket.id" role="user" :display-name="userName" />
    </div>
  </div>
</template>
