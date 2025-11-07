<script setup lang="ts">
import { ref, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import ChatWindow from '@/components/ChatWindow.vue'
import http from '@/plugins/http'
import { useAuthStore } from '@/stores/auth'
import { useChatStore } from '@/stores/chat'
import { isManagerRole, getDisplayName } from '@/utils/faqrole'
// import { getDisplayUserId } from '@/utils/faqrole'

type TicketDto = {
  id: string
  title: string
  userName: string
  userID: number
  status: string
  createdAt: string
  updatedAt: string
  RequesterEmail : string
  RequesterPhone : string
  RequesterAddress : string
  RequesterAvatarUrl : string
}

const router = useRouter()
const auth = useAuthStore()
const chat = useChatStore()

const isOpen = ref(false)          // 控制浮窗開合
const title = ref('房租問題')
const displayName = ref(getDisplayName(auth.state.profile))
// const displayUserId = ref(getDisplayUserId(auth.state.profile))
const ticket = ref<TicketDto | null>(null)

function makeId(){
  return crypto.randomUUID?.() ??
    'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, c=>{
      const r=(Math.random()*16)|0, v=c==='x'?r:(r&0x3|0x8); return v.toString(16)
    })
}

// ★★★ 右下按鈕：先判斷角色
async function onLauncherClick() {
  // 還原並補齊必要資料
  auth.restoreSession()
  if (auth.isAuthenticated.value) {
    if (!auth.state.profile) await auth.fetchProfile().catch(() => {})
    if (!auth.state.permissions?.length && auth.fetchAbilities) {
      await auth.fetchAbilities().catch(() => {})
    }
  }

  // 管理角色 → 直接進 Agent
  if (isManagerRole(auth.state.roles)) {
    router.push({ name: 'support-agent' })
    return
  }

  // 其他（TENANT/HOST/SUPPLIER/GUEST）→ 開浮窗
  isOpen.value = !isOpen.value
}

const profile = auth.state.profile
async function createTicket(){
  const { data } = await http.post<TicketDto>('/api/supporttickets', {
    title: title.value,
    userName: displayName.value
  })
  ticket.value = data
  await chat.join(ticket.value.id)

  // 前端版預設訊息（若後端已有自動訊息可刪）
  ;(chat.messages[ticket.value.id] ||= []).push({
    id: makeId(),
    ticketId: ticket.value.id,
    senderRole: 'agent',
    senderName: '系統客服',
    content: '您好，稍等片刻，將有專人為您服務。',
    createdAt: new Date().toISOString()
  })
  await nextTick()
}
</script>

<template>
  <!-- 右下浮動按鈕 -->
  <button class="chat-launcher" @click="onLauncherClick" aria-label="開啟客服">
    <svg viewBox="0 0 24 24" width="22" height="22" aria-hidden="true">
      <path d="M20 2H4a2 2 0 0 0-2 2v18l4-4h14a2 2 0 0 0 2-2V4a2 2 0 0 0-2-2Z" fill="currentColor"/>
    </svg>
  </button>

  <!-- 浮動聊天視窗（只在一般使用者且已開啟時顯示） -->
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
          <button class="min" @click="isOpen=false">—</button>
        </div>
      </header>

      <div class="widget-body">
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
