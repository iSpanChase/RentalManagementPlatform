<script setup lang="ts">
import { ref, nextTick, onMounted, computed } from 'vue'
import http from '@/plugins/http'
import ChatWindow from '@/components/ChatWindow.vue'
import { useAuthStore } from '@/stores/auth'
import { useChatStore } from '@/stores/chat'
import { getDisplayName } from '@/utils/faqrole'
import { getDisplayUserId } from '@/utils/faqrole'

type TicketDto = {
  id: string
  title: string
  userName: string
  requesterId: number
  status: string
  createdAt: string
  updatedAt: string
}

const auth = useAuthStore()
const chat = useChatStore()

// 視窗開關
const isOpen = ref(false)

// 建單資料
const title = ref('房租問題')
const ticket = ref<TicketDto | null>(null)

// 顯示名稱：自動綁定登入者資料（不能編輯）
const displayName = computed(() => getDisplayName(auth.state.profile))
const displayUserId = computed(()=> getDisplayUserId(auth.state.profile))

onMounted(async () => {
  // 還原登入狀態並補抓 profile
  auth.restoreSession()
  if (auth.isAuthenticated.value && !auth.state.profile) {
    await auth.fetchProfile().catch(() => {})
  }
})

// 產生隨機 ID
function makeId() {
  return (crypto as any)?.randomUUID?.() ??
    'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c => {
      const r = (Math.random() * 16) | 0
      const v = c === 'x' ? r : (r & 0x3) | 0x8
      return v.toString(16)
    })
}

const profile = auth.state.profile
// 建立工單
async function createTicket() {
  try {
    const { data } = await http.post<TicketDto>('/api/supporttickets', {
      title: title.value,
      userName: displayName.value,
      userID : displayUserId.value,
      requesterId: profile?.userId,
      requesterName: profile?.name || displayName.value,
      requesterEmail: profile?.email,
      requesterPhone: profile?.phone,
      requesterAddress: profile?.address,
      requesterAvatarUrl: profile?.profileImageUrl
    })
    ticket.value = data

    // 加入聊天室 & 顯示系統初始訊息
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
    // Demo 模式 fallback
    const id = makeId()
    ticket.value = {
      id,
      title: title.value,
      userName: displayName.value,
      requesterId : displayUserId.value,
      status: 'open',
      createdAt: new Date().toISOString(),
      updatedAt: new Date().toISOString()
    }
    await chat.join(id)
  }
}

// 開關切換
function toggle() {
  isOpen.value = !isOpen.value
}
</script>

<template>
  <!-- 浮動按鈕 -->
  <button class="chat-launcher" @click="toggle" aria-label="開啟客服視窗">
    <svg viewBox="0 0 24 24" width="22" height="22">
      <path d="M20 2H4a2 2 0 0 0-2 2v18l4-4h14a2 2 0 0 0 2-2V4a2 2 0 0 0-2-2Z" fill="currentColor"/>
    </svg>
  </button>

  <!-- 聊天視窗 -->
  <transition name="slide-up">
    <section v-if="isOpen" class="chat-widget">
      <header class="widget-head">
        <div class="title">
          <img class="brand" alt="" src="/favicon.ico" />
          <div>
            <div class="t1">客服聊天室</div>
            <div class="t2">您好a8 ，{{ displayName }}！</div>
          </div>
        </div>
        <button class="min" @click="isOpen = false" aria-label="最小化">—</button>
      </header>

      <div class="widget-body">
        <!-- 尚未建立工單 -->
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

        <!-- 已建立工單 -->
        <div v-else class="chatroom">
          <ChatWindow :ticket-id="ticket.id" role="user" :display-name="displayName" />
        </div>
      </div>
    </section>
  </transition>
</template>

<style scoped>
/* 浮動按鈕 */
.chat-launcher {
  position: fixed;
  right: 24px;
  bottom: 24px;
  width: 56px; height: 56px;
  border-radius: 50%;
  border: none;
  background: #2563eb;
  color: #fff;
  box-shadow: 0 8px 28px rgba(37,99,235,.45);
  cursor: pointer;
  z-index: 2147483647;
  display: grid; place-items: center;
}

/* 聊天主體 */
.chat-widget {
  position: fixed;
  right: 24px;
  bottom: 92px;
  width: 360px;
  max-height: 70vh;
  background: #fff;
  border-radius: 16px;
  box-shadow: 0 20px 60px rgba(0,0,0,.25);
  overflow: hidden;
  z-index: 2147483647;
  display: flex; flex-direction: column;
}

/* 頂部 */
.widget-head {
  display: flex; align-items: center; justify-content: space-between;
  padding: 10px 12px;
  background: #1f2937; color:#fff;
}
.title { display: flex; gap:10px; align-items: center; }
.brand { width: 28px; height: 28px; border-radius: 6px; }
.t1 { font-weight: 700; }
.t2 { font-size: 12px; opacity:.85 }

/* 表單與輸入框 */
.widget-body { padding: 10px; display: flex; flex-direction: column; }
.create-form { display: grid; gap: 10px; }
.row { display: grid; gap: 6px; }
.row span { font-size: 12px; color: #475569; }
.readonly-name {
  background: #f1f5f9;
  padding: 10px 12px;
  border-radius: 10px;
  color: #334155;
}

/* 按鈕 */
.primary {
  border: none;
  background: #2563eb;
  color:#fff;
  padding: 10px 12px;
  border-radius: 10px;
  cursor: pointer;
}
.primary:hover { filter: brightness(1.05); }

/* 動畫 */
.slide-up-enter-from, .slide-up-leave-to { transform: translateY(20px); opacity: 0; }
.slide-up-enter-active, .slide-up-leave-active { transition: all .2s ease; }
</style>
