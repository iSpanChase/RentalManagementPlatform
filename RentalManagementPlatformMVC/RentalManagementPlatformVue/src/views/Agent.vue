<script setup lang="ts">
import { ref, onMounted } from 'vue'
import http from '@/plugins/http'
import ChatWindow from '@/components/ChatWindow.vue'
import { useChatStore } from '@/stores/chat'

type TicketDto = {
  id: string
  title: string
  userName: string
  createdAt: string
  updatedAt: string
  status?: string
}

type UserProfile = {
  name: string
  email?: string
  phone?: string
  createdAt?: string
  avatar?: string
  note?: string
}

const chat = useChatStore()

// 假資料: 之後可以從後端 /api/supporttickets 取代
const tickets = ref<TicketDto[]>([])
const selectedTicket = ref<TicketDto | null>(null)
const selectedUser = ref<UserProfile | null>(null)
const loading = ref(false)

// 模擬抓取所有工單
async function fetchTickets() {
  loading.value = true
  const { data } = await http.get('/api/supporttickets')
  tickets.value = data
  loading.value = false
}

// 點選一個工單後
async function openTicket(ticket: TicketDto) {
  selectedTicket.value = ticket
  await chat.join(ticket.id)

  // 模擬抓取使用者資料
  const { data } = await http.get(`/api/users/${ticket.userName}`)
  selectedUser.value = data
}

onMounted(fetchTickets)
</script>

<template>
  <div class="agent-dashboard">
    <!-- 左欄：會話列表 -->
    <aside class="sidebar">
      <h3>會話列表</h3>
      <div v-if="loading" class="loading">載入中...</div>
      <ul v-else>
        <li
          v-for="ticket in tickets"
          :key="ticket.id"
          :class="{ active: ticket.id === selectedTicket?.id }"
          @click="openTicket(ticket)"
        >
          <div class="title">{{ ticket.title }}</div>
          <div class="meta">
            <span class="name">{{ ticket.userName }}</span>
            <span class="time">{{ new Date(ticket.updatedAt).toLocaleTimeString() }}</span>
          </div>
        </li>
      </ul>
    </aside>

    <!-- 中欄：聊天區 -->
    <main class="chat-area" v-if="selectedTicket">
      <header class="chat-header">
        <h3>{{ selectedTicket.title }}</h3>
        <span class="user-name">與 {{ selectedTicket.userName }} 聊天中</span>
      </header>
      <div class="chat-body">
        <ChatWindow :ticket-id="selectedTicket.id" role="agent" :display-name="'客服人員'" />
      </div>
    </main>
    <main class="chat-empty" v-else>
      <p>請從左側選擇一個對話。</p>
    </main>

    <!-- 右欄：客戶資訊 -->
    <aside class="profile" v-if="selectedUser">
      <div class="profile-card">
        <img :src="selectedUser.avatar || 'https://placekitten.com/100/100'" class="avatar" />
        <h4>{{ selectedUser.name }}</h4>
        <p><strong>信箱：</strong> {{ selectedUser.email || '未提供' }}</p>
        <p><strong>電話：</strong> {{ selectedUser.phone || '未提供' }}</p>
        <p><strong>建立時間：</strong> {{ selectedUser.createdAt || '不明' }}</p>
        <label>備註：</label>
        <textarea v-model="selectedUser.note" placeholder="輸入客服備註..."></textarea>
        <button class="save-btn">儲存備註</button>
      </div>
    </aside>
  </div>
</template>

<style scoped>
.agent-dashboard {
  display: grid;
  grid-template-columns: 280px 1fr 300px;
  height: 100vh;
  background: #f9fafb;
  overflow: hidden;
}

.sidebar {
  background: #fff;
  border-right: 1px solid #e5e7eb;
  padding: 12px;
  overflow-y: auto;
}
.sidebar h3 {
  margin-bottom: 12px;
}
.sidebar ul {
  list-style: none;
  padding: 0;
  margin: 0;
}
.sidebar li {
  padding: 10px;
  border-radius: 8px;
  margin-bottom: 6px;
  cursor: pointer;
  transition: background 0.2s;
}
.sidebar li:hover {
  background: #f3f4f6;
}
.sidebar li.active {
  background: #2563eb;
  color: white;
}
.sidebar .meta {
  display: flex;
  justify-content: space-between;
  font-size: 12px;
  opacity: 0.8;
}

.chat-area {
  display: flex;
  flex-direction: column;
  background: #f0f2f5;
}
.chat-header {
  padding: 10px 16px;
  background: white;
  border-bottom: 1px solid #e5e7eb;
}
.chat-body {
  flex: 1;
  padding: 16px;
  overflow-y: auto;
}

.profile {
  background: #fff;
  border-left: 1px solid #e5e7eb;
  padding: 16px;
  overflow-y: auto;
}
.profile-card {
  display: flex;
  flex-direction: column;
  gap: 8px;
}
.profile-card .avatar {
  width: 80px;
  height: 80px;
  border-radius: 50%;
  object-fit: cover;
}
.profile-card textarea {
  min-height: 80px;
  resize: none;
}
.save-btn {
  background: #2563eb;
  color: white;
  border: none;
  border-radius: 8px;
  padding: 8px;
  cursor: pointer;
}
.save-btn:hover {
  background: #1d4ed8;
}
.chat-empty {
  display: flex;
  justify-content: center;
  align-items: center;
  color: #6b7280;
}
</style>
