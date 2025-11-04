<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, computed, watch } from 'vue'
import { useChatStore } from '@/stores/chat'

const props = defineProps<{
  ticketId: string
  role: 'user' | 'agent'         // 你是誰（當事人）
  displayName: string
}>()

const chat = useChatStore()
const input = ref('')
const logEl = ref<HTMLDivElement | null>(null)

// 只取該房訊息
const messages = computed(() => chat.messages[props.ticketId] ?? [])

// 決定訊息是不是自己發的：同角色即可（若你要更嚴謹，可加名字比對）
const isMine = (m: { senderRole: string; senderName: string }) =>
  m.senderRole === props.role

function autoScroll() {
  requestAnimationFrame(() => {
    if (logEl.value) logEl.value.scrollTop = logEl.value.scrollHeight
  })
}

async function send() {
  const text = input.value.trim()
  if (!text) return
  await chat.send(props.ticketId, props.role, props.displayName, text)
  input.value = ''
  autoScroll()
}

onMounted(async () => {
  await chat.join(props.ticketId)
  autoScroll()
})

onBeforeUnmount(async () => {
  await chat.leave(props.ticketId)
})

// 有新訊息時自動捲底
watch(messages, autoScroll, { deep: true })
</script>

<template>
  <div class="chatwrap">
    <div class="meta">
      <span>房間: {{ props.ticketId.slice(0,8) }}</span>
      <span>連線: {{ chat.status }}</span>
    </div>

    <div ref="logEl" class="chatlog">
      <div
        v-for="m in messages"
        :key="m.id"
        class="row"
        :class="isMine(m) ? 'right' : 'left'"
      >
        <!-- 對方在左、自己在右：頭像＋泡泡 -->
        <div class="avatar" :title="m.senderName">
          {{ (m.senderName || (m.senderRole==='agent' ? 'A' : 'U')).slice(0,1).toUpperCase() }}
        </div>

        <div class="bubble">
          <div class="name-time">
            <span class="name">{{ m.senderName || m.senderRole }}</span>
            <span class="time">{{ new Date(m.createdAt).toLocaleTimeString() }}</span>
          </div>
          <div class="content">{{ m.content }}</div>
        </div>
      </div>
    </div>

    <div class="inputrow">
      <input
        v-model="input"
        @keyup.enter="send"
        :placeholder="chat.status==='connected' ? '輸入訊息…' : '連線中…'"
        :disabled="chat.status!=='connected'"
      />
      <button @click="send" :disabled="chat.status!=='connected' || !input.trim()">送出</button>
    </div>
  </div>
</template>

<style scoped>
.chatwrap { width: 100%; }
.meta { display:flex; gap:12px; font-size:12px; color:#666; margin-bottom:6px; }
.chatlog {
  border: 1px solid #e5e7eb;
  border-radius: 10px;
  height: 360px;
  overflow: auto;
  padding: 10px;
  background: #fafafa;
}

/* 一則訊息的排列：左或右 */
.row {
  display: grid;
  grid-template-columns: 40px 1fr;
  align-items: end;
  gap: 8px;
  margin: 8px 0;
}
.row.right {
  grid-template-columns: 1fr 40px;   /* 右邊排列：先泡泡再頭像 */
}
.row.left .avatar { order: 1; }
.row.left .bubble { order: 2; }
.row.right .bubble { order: 1; justify-self: end; }
.row.right .avatar { order: 2; justify-self: end; }

/* 頭像 */
.avatar {
  width: 32px; height: 32px;
  border-radius: 50%;
  display: grid; place-items: center;
  font-size: 14px; font-weight: 700;
  color: white;
  background: #9ca3af; /* 灰 */
  user-select: none;
}

/* 泡泡 */
.bubble {
  max-width: 70%;
  padding: 8px 10px;
  border-radius: 14px;
  background: white;
  border: 1px solid #e5e7eb;
  box-shadow: 0 1px 1px rgba(0,0,0,.03);
}
.row.right .bubble {
  background: #e0f2fe;            /* 自己：淡藍 */
  border-color: #bae6fd;
}
.row.left .bubble {
  background: #ffffff;            /* 對方：白 */
  border-color: #e5e7eb;
}

/* 名稱 + 時間 */
.name-time {
  display: flex;
  align-items: baseline;
  gap: 8px;
  margin-bottom: 4px;
}
.name { font-weight: 700; font-size: 13px; color: #111827; }
.time { font-size: 12px; color: #6b7280; }

/* 文字 */
.content { white-space: pre-wrap; word-break: break-word; }

/* 輸入列 */
.inputrow {
  display: flex; gap: 8px; margin-top: 10px;
}
.inputrow input {
  flex: 1;
  padding: 10px 12px;
  border: 1px solid #d1d5db; border-radius: 8px;
}
.inputrow button {
  padding: 10px 14px;
  border: 1px solid #9ca3af; border-radius: 8px;
  background: #f9fafb; cursor: pointer;
}
.inputrow button:disabled { opacity: .6; cursor: not-allowed; }
</style>
