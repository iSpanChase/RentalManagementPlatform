<script setup lang="ts">
import { ref, nextTick, onMounted, computed, watch } from 'vue'
import http from '@/plugins/http'
import ChatWindow from '@/components/ChatWindow.vue'
import { useAuthStore } from '@/stores/auth'
import { useChatStore } from '@/stores/chat'
import { getDisplayName } from '@/utils/faqrole'

type TicketDto = {
  id: string; title: string; userName: string; status: string; createdAt: string; updatedAt: string
}

const auth = useAuthStore()
const chat = useChatStore()

const isOpen = ref(false)
const title = ref('房租問題')
const ticket = ref<TicketDto | null>(null)

const displayName = computed(() => getDisplayName(auth.state.profile))

onMounted(async () => {
  auth.restoreSession()
  if (auth.isAuthenticated.value && !auth.state.profile) {
    await auth.fetchProfile().catch(() => {})
  }
})

watch(() => auth.isAuthenticated.value, async (v) => {
  if (v && !auth.state.profile) await auth.fetchProfile().catch(() => {})
})

function makeId() {
  return (crypto as any)?.randomUUID?.() ??
    'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
      const r = (Math.random() * 16) | 0, v = c === 'x' ? r : (r & 0x3) | 0x8
      return v.toString(16)
    })
}

async function createTicket() {
  try {
    const { data } = await http.post<TicketDto>('/api/supporttickets', {
      title: title.value,
      userName: displayName.value
    })
    ticket.value = data
    await chat.join(ticket.value.id)
    ;(chat.messages[ticket.value.id] ||= []).push({
      id: makeId(),
      ticketId: ticket.value.id,
      senderRole: 'agent',
      senderName: '系統客服',
      content: '您好，稍等片刻，將有專人為您服務。',
      createdAt: new Date().toISOString()
    })
    await nextTick()
  } catch {
    const id = makeId()
    ticket.value = {
      id, title: title.value, userName: displayName.value,
      status: 'open', createdAt: new Date().toISOString(), updatedAt: new Date().toISOString()
    }
    await chat.join(id)
  }
}

function toggle() { isOpen.value = !isOpen.value }
</script>

<template>
  <button class="chat-launcher" @click="toggle">…</button>

  <transition name="slide-up">
    <section v-if="isOpen" class="chat-widget">
      <header class="widget-head">
        <div class="title">
          <img class="brand" alt="" src="/favicon.ico" />
          <div>
            <div class="t1">客服聊天室</div>
            <div class="t2">您好，{{ displayName }}！</div>
          </div>
        </div>
        <button class="min" @click="isOpen = false">—</button>
      </header>

      <div class="widget-body">
        <form v-if="!ticket" class="create-form" @submit.prevent="createTicket">
          <div class="row">
            <span>顯示名稱</span>
            <div class="readonly-name">{{ displayName }}</div>
          </div>
          <label class="row">
            <span>問題標題</span>
            <input v-model="title" placeholder="請輸入問題…" />
          </label>
          <button class="primary" type="submit">開始聊天</button>
        </form>

        <div v-else class="chatroom">
          <ChatWindow :ticket-id="ticket.id" role="user" :display-name="displayName" />
        </div>
      </div>
    </section>
  </transition>
</template>
