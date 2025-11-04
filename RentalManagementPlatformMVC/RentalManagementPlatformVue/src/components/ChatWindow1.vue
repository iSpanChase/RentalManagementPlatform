<script setup lang="ts">
import { ref, onMounted, onBeforeUnmount, computed } from 'vue'
import { useChatStore } from '@/stores/chat'

const props = defineProps<{
  ticketId: string
  role: 'user' | 'agent'
  displayName: string
}>()

const chat = useChatStore()
const input = ref('')
const logEl = ref<HTMLDivElement | null>(null)

// 直接從 store 取該房的訊息陣列
const messages = computed(() => chat.messages[props.ticketId] ?? [])

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
</script>

<template>
  <div>
    <div class="meta">
      <span>房間：{{ props.ticketId.slice(0,8) }}</span>
      <span>連線：{{ chat.status }}</span>
    </div>

    <div ref="logEl" class="chat">
      <div v-for="m in messages" :key="m.id" class="msg">
        <b>[{{ new Date(m.createdAt).toLocaleTimeString() }}] {{ m.senderName }} ({{ m.senderRole }})</b>
        <div>{{ m.content }}</div>
      </div>
    </div>

    <div class="row">
      <input v-model="input" @keyup.enter="send" placeholder="輸入訊息…" />
      <button :disabled="chat.status!=='connected'" @click="send">送出</button>
    </div>
  </div>
</template>

<style scoped>
.meta{ display:flex; gap:12px; font-size:12px; color:#666; }
.chat{ border:1px solid #ddd; border-radius:8px; height:300px; overflow:auto; padding:8px; margin:12px 0;}
.msg{ padding:6px 0; border-bottom:1px dashed #eee;}
.row{ display:flex; gap:8px}
input{ flex:1; padding:8px; border:1px solid #ccc; border-radius:6px}
button{ padding:8px 12px; border:1px solid #999; background:#fafafa; border-radius:6px; cursor:pointer}
button:disabled{ opacity:.6; cursor:not-allowed }
</style>
