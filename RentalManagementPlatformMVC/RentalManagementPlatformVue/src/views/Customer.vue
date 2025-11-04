<script setup lang="ts">
import { ref, nextTick } from 'vue'
import http from '../plugins/http'
import ChatWindow from '../components/ChatWindow.vue'
import { useChatStore } from '@/stores/chat'   // ✅ 新增：引入 chat store

type TicketDto = { id: string; title: string; userName: string; status: string; createdAt: string; updatedAt: string }

const title = ref('房租問題')
const userName = ref('CustomerA')
const ticket = ref<TicketDto | null>(null)
const chat = useChatStore()                    // ✅ 新增：取得 store 實例

function makeId() {
  // 後備用：沒有 crypto.randomUUID 時使用
  return (crypto as any)?.randomUUID?.() ??
    'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
      const r = (Math.random() * 16) | 0, v = c === 'x' ? r : (r & 0x3) | 0x8
      return v.toString(16)
    })
}

async function createTicket() {
  try {
    const { data } = await http.post<TicketDto>('/api/supporttickets', {
      title: title.value, userName: userName.value
    })
    ticket.value = data

    // ✅ 立刻初始化該房的訊息（拉歷史並 JoinRoom）
    await chat.join(ticket.value.id)

    // ✅ 推入預設訊息（只存在前端，不會寫到後端）
    ;(chat.messages[ticket.value.id] ||= []).push({
      id: makeId(),
      ticketId: ticket.value.id,
      senderRole: 'agent',
      senderName: '系統客服',
      content: '您好，稍等片刻，將有專人為您服務。',
      createdAt: new Date().toISOString()
    })
  } catch (e) {
    // 如果你暫時沒有後端，也讓流程能跑（選用）
    const localId = makeId()
    ticket.value = { id: localId, title: title.value, userName: userName.value, status: 'open', createdAt: new Date().toISOString(), updatedAt: new Date().toISOString() }

    // 仍然加入房間並加入預設訊息
    await chat.join(localId)
    ;(chat.messages[localId] ||= []).push({
      id: makeId(),
      ticketId: localId,
      senderRole: 'agent',
      senderName: '系統客服',
      content: '您好，稍等片刻，將有專人為您服務。',
      createdAt: new Date().toISOString()
    })
  }

  // 等待畫面渲染後再自動捲底（如果你在 ChatWindow 有 autoScroll 也可省略）
  await nextTick()
}
</script>

<template>
  <div>
    <h2>客服聊天室</h2>

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