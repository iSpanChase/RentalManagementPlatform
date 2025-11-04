<script setup lang="ts">
import { ref, onMounted } from 'vue'
import http from '@/plugins/http'
import ChatWindow from '@/components/ChatWindow.vue'
import { useAuthStore } from '@/stores/faqauth'

type TicketDto = { id: string; title: string; userName: string; status: string; createdAt: string; updatedAt: string }

const tickets = ref<TicketDto[]>([])
const opened = ref<TicketDto[]>([])
const auth = useAuthStore()

async function loadTickets() {
  if (!auth.user) auth.useMock('agent') // 尚未接登入時先 mock
  const { data } = await http.get<TicketDto[]>('/api/supporttickets?status=open')
  tickets.value = data
}
function openTicket(t: TicketDto) {
  if (!opened.value.find(x => x.id === t.id)) opened.value.push(t)
}
onMounted(loadTickets)
</script>

<template>
  <div class="layout">
    <aside>
      <h4>客服後台（Agent）</h4>
      <p v-if="auth.user">目前登入：{{ auth.user.name }}（{{ auth.user.userId }}）</p>
      <ul>
        <li v-for="t in tickets" :key="t.id">
          <button @click="openTicket(t)">{{ t.title }}（{{ new Date(t.createdAt).toLocaleTimeString() }}）</button>
        </li>
      </ul>
    </aside>

    <main>
      <section v-for="t in opened" :key="t.id" class="panel">
        <header class="panel-head"><h4>{{ t.title }} <small>({{ t.id.slice(0,8) }})</small></h4></header>
        <ChatWindow :ticket-id="t.id" role="agent" :display-name="auth.user!.name" />
      </section>

      <p v-if="!opened.length" class="placeholder">請從左側選擇一個工單</p>
    </main>
  </div>
</template>
