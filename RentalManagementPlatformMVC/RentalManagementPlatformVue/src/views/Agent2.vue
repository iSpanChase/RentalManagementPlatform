<template>
  <!-- 浮動主按鈕 -->
  <button class="chat-launcher" @click="togglePanel" aria-label="開啟客服面板">
    💬
  </button>

  <!-- 客服後台的聊天清單 -->
  <transition name="fade">
    <section v-if="isOpen" class="agent-panel">
      <header class="panel-head">
        <h3>客服後台</h3>
        <button @click="isOpen = false">✕</button>
      </header>

      <div class="ticket-list">
        <div v-for="t in tickets" :key="t.id" class="ticket-row" @click="openChat(t)">
          <b>{{ t.title }}</b>
          <small>（{{ new Date(t.createdAt).toLocaleTimeString() }}）</small>
        </div>
      </div>
    </section>
  </transition>

  <!-- 動態生成的多個聊天視窗 -->
  <transition-group name="slide-up">
    <section v-for="(chat, i) in openChats" :key="chat.id"
             class="chat-widget" :style="{ right: `${24 + i * 380}px` }">
      <header class="widget-head">
        <div class="title">{{ chat.title }}（{{ chat.userName }}）</div>
        <button class="min" @click="closeChat(chat.id)">✕</button>
      </header>
      <div class="widget-body">
        <ChatWindow :ticket-id="chat.id" role="agent" :display-name="agentName" />
      </div>
    </section>
  </transition-group>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import ChatWindow from '@/components/ChatWindow.vue'
import http from '@/plugins/http'
import { useAuthStore } from '@/stores/faqauth'

interface TicketDto {
  id: string; title: string; userName: string; createdAt: string;
}

const auth = useAuthStore()
const agentName = auth.user?.name ?? '測試客服'
const isOpen = ref(false)
const tickets = ref<TicketDto[]>([])
const openChats = ref<TicketDto[]>([])

function togglePanel() { isOpen.value = !isOpen.value }

// 載入目前待處理的工單清單
onMounted(async () => {
  try {
    const { data } = await http.get<TicketDto[]>('/api/supporttickets')
    tickets.value = data
  } catch {
    // demo 假資料
    tickets.value = [
      { id: 'T-001', title: '房租問題', userName: 'CustomerA', createdAt: new Date().toISOString() },
      { id: 'T-002', title: '付款異常', userName: 'CustomerB', createdAt: new Date().toISOString() },
    ]
  }
})

// 開啟個別聊天
function openChat(ticket: TicketDto) {
  if (!openChats.value.find(c => c.id === ticket.id))
    openChats.value.push(ticket)
}

// 關閉個別聊天視窗
function closeChat(id: string) {
  openChats.value = openChats.value.filter(c => c.id !== id)
}
</script>

<style scoped>
.chat-launcher {
  position: fixed; right: 24px; bottom: 24px;
  width: 56px; height: 56px; border-radius: 50%;
  border: none; background: #1e40af; color: white;
  box-shadow: 0 8px 28px rgba(30,64,175,.5);
  cursor: pointer; z-index: 2147483647;
  font-size: 22px;
}

.agent-panel {
  position: fixed; right: 24px; bottom: 92px;
  width: 320px; background: #fff; border-radius: 12px;
  box-shadow: 0 16px 40px rgba(0,0,0,.25);
  z-index: 2147483647; overflow: hidden;
}
.panel-head {
  background: #1f2937; color: white;
  display: flex; justify-content: space-between; align-items: center;
  padding: 8px 12px;
}
.ticket-list { max-height: 260px; overflow-y: auto; }
.ticket-row { padding: 8px 12px; cursor: pointer; border-bottom: 1px solid #eee; }
.ticket-row:hover { background: #f1f5f9; }

.chat-widget {
  position: fixed; bottom: 24px;
  width: 360px; height: 480px;
  background: white; border-radius: 16px;
  box-shadow: 0 16px 48px rgba(0,0,0,.25);
  overflow: hidden; z-index: 2147483647;
}
.widget-head {
  background: #1f2937; color: white;
  padding: 8px 12px; display: flex; justify-content: space-between; align-items: center;
}
.widget-body { height: calc(100% - 44px); overflow: hidden; }
.slide-up-enter-active, .slide-up-leave-active { transition: all .2s ease; }
.slide-up-enter-from, .slide-up-leave-to { opacity: 0; transform: translateY(20px); }
</style>
