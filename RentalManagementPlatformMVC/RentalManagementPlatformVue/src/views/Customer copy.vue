<script setup lang="ts">
import { ref } from 'vue'
import http from '@/plugins/http'
import ChatWindow from '@/components/ChatWindow.vue'
import { useAuthStore } from '@/stores/faqauth'

type TicketDto = { id: string; title: string; userName: string; status: string; createdAt: string; updatedAt: string }

const title = ref('房租問題')
const ticket = ref<TicketDto | null>(null)
const auth = useAuthStore()

async function createTicket() {
  // 若還沒串登入，可先 mock 一個 user
  if (!auth.user) auth.useMock('user')

  const { data } = await http.post<TicketDto>('/api/supporttickets', {
    title: title.value,
    userName: auth.user!.name
  })
  ticket.value = data
}
</script>

<template>
  <div>
    <h2>客服聊天室（客戶端）</h2>
    <p v-if="auth.user">目前登入：{{ auth.user.name }}（{{ auth.user.userId }}）</p>

    <div v-if="!ticket" style="display:grid;gap:8px;max-width:420px;">
      <input v-model="title" placeholder="問題標題" />
      <button @click="createTicket">建立工單並開始聊天</button>
    </div>

    <div v-else>
      <p><b>TicketId：</b>{{ ticket.id }}</p>
      <ChatWindow :ticket-id="ticket.id" role="user" :display-name="auth.user!.name" />
    </div>
  </div>
</template>
