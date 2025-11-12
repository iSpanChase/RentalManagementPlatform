<script setup lang="ts">
import { ref } from 'vue';
import { assistantChat } from '@/api/assistantApi';

interface ChatItem {
  role: 'user' | 'assistant' | 'system';
  content: string;
}

interface RoomItem {
  id: number;
  title: string;
  price?: number;
}

const isOpen = ref(false);
const inputText = ref('');
const sending = ref(false);
const messages = ref<ChatItem[]>([
  { role: 'system', content: '嗨，我可以幫你用自然語言查詢附近房源。直接輸入地名或需求，例如：「台北101附近5公里房源」。' }
]);

const results = ref<RoomItem[]>([]);

async function send() {
  const text = inputText.value.trim();
  if (!text || sending.value) return;
  messages.value.push({ role: 'user', content: text });
  inputText.value = '';
  sending.value = true;
  try {
    const res = await assistantChat(text);
    const place = (res?.data as any)?.place as { name?: string } | undefined;
    const roomsRaw = ((res?.data as any)?.rooms as any[]) || [];
    results.value = roomsRaw
      .slice(0, 5)
      .map((r: any): RoomItem => ({
        id: Number(r.roomId ?? r.room_id ?? 0),
        title: String(r.title ?? ''),
        price: r.pricePerNight ?? r.price_per_night,
      }))
      .filter(r => !!r.id && !!r.title);

    const header = (() => {
      const count = roomsRaw?.length ?? 0;
      const name = place?.name ?? '';
      if (name && count >= 0) return `在「${name}」附近共找到 ${count} 間房源，以下為前 ${Math.min(5, count)} 間：`;
      if (count >= 0) return `共找到 ${count} 間房源，以下為前 ${Math.min(5, count)} 間：`;
      return '查詢完成。';
    })();

    const reply = (res?.message && res.message.trim().length > 0) ? res.message : header;
    messages.value.push({ role: 'assistant', content: reply });
  } catch (err: any) {
    messages.value.push({ role: 'assistant', content: '抱歉，查詢失敗，請稍後再試。' });
    console.error('assistant chat error', err);
  } finally {
    sending.value = false;
    scrollToBottom();
  }
}

function toggle() {
  isOpen.value = !isOpen.value;
  if (isOpen.value) setTimeout(scrollToBottom, 0);
}

function onKeydown(e: KeyboardEvent) {
  if (e.key === 'Enter' && !e.shiftKey) {
    e.preventDefault();
    send();
  }
}

function scrollToBottom() {
  const box = document.querySelector('.assistant-chat-panel .messages');
  if (box) box.scrollTop = box.scrollHeight;
}
</script>

<template>
  <div class="assistant-chat-root">
    <button class="assistant-fab" @click="toggle" aria-label="AI 查詢">
      <span>AI</span>
    </button>

    <div v-if="isOpen" class="assistant-chat-panel">
      <div class="header">
        <div class="title">AI 查詢</div>
        <button class="close" @click="toggle" aria-label="關閉">×</button>
      </div>
      <div class="messages">
        <div v-for="(m, i) in messages" :key="i" :class="['msg', m.role]">
          <div class="bubble">{{ m.content }}</div>
        </div>
      </div>
      <div v-if="results.length" class="results">
        <div class="result-item" v-for="r in results" :key="r.id">
          <router-link :to="`/rooms/${r.id}`">{{ r.title }}</router-link>
          <span v-if="r.price != null" class="price">$ {{ r.price }}</span>
        </div>
      </div>
      <div class="input-row">
        <textarea
          v-model="inputText"
          :disabled="sending"
          placeholder="輸入：台北101附近5公里房源"
          @keydown="onKeydown"
        ></textarea>
        <button class="send" :disabled="sending || !inputText.trim()" @click="send">送出</button>
      </div>
    </div>
  </div>
  
</template>

<style scoped>
.assistant-chat-root {
  position: fixed;
  right: 20px;
  bottom: 20px;
  z-index: 1100;
}

.assistant-fab {
  width: 56px;
  height: 56px;
  border-radius: 50%;
  border: 0;
  background: #1d4ed8;
  color: #fff;
  font-weight: 700;
  box-shadow: 0 6px 16px rgba(0,0,0,0.2);
  cursor: pointer;
}

.assistant-chat-panel {
  position: absolute;
  right: 0;
  bottom: 70px;
  width: 320px;
  max-height: 60vh;
  background: #fff;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  box-shadow: 0 12px 28px rgba(0,0,0,0.18);
  display: flex;
  flex-direction: column;
}

.header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 12px;
  border-bottom: 1px solid #e5e7eb;
}
.header .title { font-weight: 600; color: #111827; }
.header .close { border: 0; background: transparent; font-size: 20px; cursor: pointer; }

.messages {
  padding: 10px;
  overflow: auto;
  flex: 1 1 auto;
}
.msg { display: flex; margin: 6px 0; }
.msg.system { justify-content: center; }
.msg.user { justify-content: flex-end; }
.msg.assistant { justify-content: flex-start; }
.bubble {
  padding: 8px 10px;
  border-radius: 10px;
  max-width: 85%;
  line-height: 1.4;
  white-space: pre-wrap;
}
.msg.system .bubble { background: #f3f4f6; color: #374151; }
.msg.user .bubble { background: #1d4ed8; color: #fff; }
.msg.assistant .bubble { background: #eef2ff; color: #1f2937; }

.results {
  padding: 0 10px 8px 10px;
}
.result-item {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 4px 0;
  border-bottom: 1px dashed #e5e7eb;
}
.result-item .price {
  color: #334155;
  margin-left: 8px;
}

.input-row {
  border-top: 1px solid #e5e7eb;
  padding: 8px;
  display: flex;
  gap: 6px;
}
.input-row textarea {
  flex: 1 1 auto;
  resize: none;
  min-height: 38px;
  max-height: 110px;
  padding: 8px;
  border: 1px solid #e5e7eb;
  border-radius: 8px;
}
.input-row .send {
  border: 0;
  background: #1d4ed8;
  color: #fff;
  border-radius: 8px;
  padding: 0 12px;
  cursor: pointer;
}
</style>
