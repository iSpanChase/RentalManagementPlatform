<script setup lang="ts">
import { ref, nextTick, onMounted } from 'vue'
import http from '@/plugins/http'
import ChatWindow from '@/components/ChatWindow.vue'
import { useAuthStore } from '@/stores/faqauth'
import { useChatStore } from '@/stores/chat'

type TicketDto = {
  id: string; title: string; userName: string; status: string; createdAt: string; updatedAt: string
}

const auth = useAuthStore()
const chat = useChatStore()

// 右下角浮動視窗開關
const isOpen = ref(false)

// 建單資料
const title = ref('房租問題')
const ticket = ref<TicketDto | null>(null)

// 預設顯示名稱：從 auth 取，沒有就給一個暫名
const displayName = ref(auth.user?.name || '訪客')

// 建單
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

    // 加入房間 & 推入預設訊息（前端版；你若已做後端自動訊息可以移除這段）
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
    // 沒有後端也能 Demo
    const id = makeId()
    ticket.value = {
      id, title: title.value, userName: displayName.value,
      status: 'open', createdAt: new Date().toISOString(), updatedAt: new Date().toISOString()
    }
    await chat.join(id)
  }
}

// 切換開關
function toggle() { isOpen.value = !isOpen.value }

// 讓使用者名稱預設成 auth 的名稱
onMounted(() => {
  if (!auth.user) auth.useMock('user')   // 你已有登入可拿掉
  displayName.value = auth.user?.name || '訪客'
})
</script>

<template>
  <!-- 浮動按鈕（右下角） -->
  <button class="chat-launcher" @click="toggle" aria-label="開啟客服視窗">
    <!-- 簡單 SVG icon（不用額外套件） -->
    <svg viewBox="0 0 24 24" width="22" height="22" aria-hidden="true">
      <path d="M20 2H4a2 2 0 0 0-2 2v18l4-4h14a2 2 0 0 0 2-2V4a2 2 0 0 0-2-2Z" fill="currentColor"/>
    </svg>
    <span class="badge">1</span>
  </button>

  <!-- 浮動聊天視窗 -->
  <transition name="slide-up">
    <section v-if="isOpen" class="chat-widget" role="dialog" aria-modal="true">
      <header class="widget-head">
        <div class="title">
          <img class="brand" alt="" src="/favicon.ico" />
          <div>
            <div class="t1">客服聊天室</div>
            <div class="t2">您好，{{ displayName }}！</div>
          </div>
        </div>
        <div class="head-actions">
          <button class="min" @click="isOpen = false" aria-label="最小化">—</button>
        </div>
      </header>

      <div class="widget-body">
        <!-- 尚未建立工單：先顯示簡易表單 -->
        <form v-if="!ticket" class="create-form" @submit.prevent="createTicket">
          <label class="row">
            <span>顯示名稱</span>
            <input v-model="displayName" placeholder="您的稱呼" />
          </label>
          <label class="row">
            <span>問題標題</span>
            <input v-model="title" placeholder="請輸入問題…" />
          </label>
          <button class="primary" type="submit">開始聊天</button>
        </form>

        <!-- 已建立工單：顯示聊天室 -->
        <div v-else class="chatroom">
          <ChatWindow :ticket-id="ticket.id" role="user" :display-name="displayName" />
        </div>
      </div>
    </section>
  </transition>
</template>

<style scoped>
/* 浮動按鈕 */
.chat-launcher{
  position: fixed;
  right: 24px;
  bottom: 24px;
  width: 56px; height: 56px;
  border-radius: 50%;
  border: none;
  background: #2563eb;
  color: #fff;
  box-shadow: 0 8px 28px rgba(37, 99, 235, .45);
  cursor: pointer;
  z-index: 2147483647;  /* 超高，確保在最上層 */
  display: grid; place-items: center;
}
.chat-launcher .badge{
  position: absolute;
  top: -4px; right: -4px;
  background: #ef4444; color: #fff;
  font-size: 12px; padding: 2px 6px; border-radius: 999px;
}

/* 浮動聊天視窗（Messenger 風格） */
.chat-widget{
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
@media (max-width: 480px){
  .chat-widget{ right: 12px; left: 12px; width: auto; bottom: 84px; }
}

/* 頂部 */
.widget-head{
  display: flex; align-items: center; justify-content: space-between;
  padding: 10px 12px;
  background: #1f2937; color:#fff;
}
.title{ display: flex; gap:10px; align-items: center; }
.brand{ width: 28px; height: 28px; border-radius: 6px; }
.t1{ font-weight: 700; line-height: 1; }
.t2{ font-size: 12px; opacity:.85 }
.head-actions button{
  background: transparent; color: #cbd5e1; border: none; cursor: pointer;
  font-size: 18px; line-height: 1; padding: 4px 8px; border-radius: 6px;
}
.head-actions button:hover{ background: rgba(255,255,255,.1); }

/* 內容 */
.widget-body{ padding: 10px; min-height: 320px; display: flex; flex-direction: column; }
.create-form{ display: grid; gap: 10px; }
.row{ display: grid; gap: 6px; }
.row span{ font-size: 12px; color: #475569; }
input{
  width: 100%; padding: 10px 12px; border: 1px solid #d1d5db; border-radius: 10px;
}
.primary{
  border: none; background: #2563eb; color:#fff; padding: 10px 12px; border-radius: 10px; cursor: pointer;
}
.primary:hover{ filter: brightness(1.05); }

.chatroom{ height: 56vh; min-height: 320px; }

/* 動畫 */
.slide-up-enter-from, .slide-up-leave-to { transform: translateY(20px); opacity: 0; }
.slide-up-enter-active, .slide-up-leave-active { transition: all .2s ease; }
</style>
