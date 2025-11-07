<script setup lang="ts">
import { ref, onMounted } from 'vue'
import http from '@/plugins/http'
import ChatWindow from '@/components/ChatWindow.vue'
import { useChatStore } from '@/stores/chat'
import { useAuthStore } from '@/stores/auth'
import type { UserProfile } from '@/types/auth'
import defaultAvatar from '@/image/faq.png'

type TicketDto = {
  id: string
  title: string
  userName: string
  requesterId: number
  status: string
  createdAt: string
  updatedAt: string
}

const chat = useChatStore()
const auth = useAuthStore()

const tickets = ref<TicketDto[]>([])
const selectedTicket = ref<TicketDto | null>(null)
const selectedUser = ref<UserProfile | null>(null)
const loading = ref(false)
const loadingRight = ref(false)

onMounted(async () => {
  auth.restoreSession()
  if (auth.isAuthenticated.value && !auth.state.profile) {
    await auth.fetchProfile().catch(() => {})
  }
  await fetchTickets()
})

async function fetchTickets() {
  loading.value = true
  const { data } = await http.get<TicketDto[]>('/api/supporttickets')
  //console.log("data == ")
  tickets.value = data
  loading.value = false
}

function snapshotToProfile(t: TicketDto): UserProfile {
  return {
    userId: t.requesterId,
    username: t.userName,
    name: t.userName,
    email: '',
    phone: '',
    address: '',
    gender: '',
    point: null,
    profileImageUrl: null,
    isVerified: false,
    birthDate: '',
  }
}

async function openTicket(ticket: TicketDto) {
  if (selectedTicket.value?.id === ticket.id) return
  if (selectedTicket.value) await chat.leave(selectedTicket.value.id).catch(() => {})
  selectedTicket.value = ticket

  await chat.join(ticket.id).catch(e => console.error('join room failed', e))

  loadingRight.value = true
  selectedUser.value = snapshotToProfile(ticket)

  try {
    let profile: UserProfile | null = null

    // ✅ number 檢查用 > 0
    if (typeof ticket.requesterId === 'number' && ticket.requesterId > 0) {
      const { data } = await http.get<UserProfile>(`/api/Users/${ticket.requesterId}`)
      profile = data
    }
    // 🔄 備援：舊資料沒 id 時用 username 查（注意是 query 參數）
    else if (ticket.userName) {
      const { data } = await http.get<UserProfile>(`/api/Users/by-username`, {
        params: { username: ticket.userName }
      })
      profile = data
    }

    if (profile) selectedUser.value = profile
  } catch (err) {
    console.warn('load requester profile failed:', err)
  } finally {
    loadingRight.value = false
  }
}


/** 小工具：更友善的時間標示 */
function formatTime(iso: string) {
  const d = new Date(iso)
  const now = new Date()
  const isToday = d.toDateString() === now.toDateString()
  return isToday
    ? d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
    : d.toLocaleDateString() + ' ' + d.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
}
</script>

<template>
  <div class="agent-dashboard">
    <!-- 左側：會話列表 -->
    <aside class="sidebar">
      <div class="sidebar__header">
        <h3>會話列表</h3>
        <div class="pill">{{ tickets.length }}</div>
      </div>

      <div v-if="loading" class="skeleton-list">
        <div class="skeleton-item" v-for="i in 6" :key="i"></div>
      </div>

      <ul v-else class="ticket-list">
        <li
  v-for="t in tickets"
  :key="t.id"
  :class="['ticket', { active: t.id === selectedTicket?.id }]"
  @click="openTicket(t)"
>
  <div class="ticket__row1">
    <h4 class="ticket__title">{{ t.title }}</h4>
    <span class="badge" :data-variant="t.status || 'open'">
      {{ (t.status || 'Open').toUpperCase() }}
    </span>
  </div>

  <div class="ticket__row2">
    <div class="ticket__name">{{ t.userName }}</div>
    <time class="ticket__time">{{ formatTime(t.updatedAt) }}</time>
  </div>
</li>
      </ul>
    </aside>

    <!-- 中間：聊天 -->
    <main class="chat-area" v-if="selectedTicket">
      <header class="chat-header">
        <div class="chat-header__title">
          <h3 class="ellipsis">{{ selectedTicket.title }}</h3>
          <span class="sub">與 {{ selectedTicket.userName }} 聊天中</span>
        </div>
        <div class="chat-header__right">
          <span class="status-dot online"></span> connected
        </div>
      </header>

      <div class="chat-body card">
        <ChatWindow :ticket-id="selectedTicket.id" role="agent" :display-name="'客服人員'" />
      </div>
    </main>
    <main class="chat-empty" v-else>
      <div class="empty card">
        <div class="empty__title">選擇左側的會話開始聊天</div>
        <div class="empty__sub">支援工單將顯示在這裡</div>
      </div>
    </main>

    <!-- 右側：客戶側邊欄 -->
    <aside class="profile">
      <div class="profile-card card" v-if="!loadingRight && selectedUser">
        <div class="profile-card__head">
          <img :src="selectedUser?.profileImageUrl || defaultAvatar" class="avatar" alt="客戶頭像" />
          <div class="head-text">
            <div class="name ellipsis">{{ selectedUser.name || selectedUser.username || '未命名使用者' }}</div>
            <div class="tagline">
              <span class="chip" :data-ok="selectedUser.isVerified">{{ selectedUser.isVerified ? '已驗證' : '未驗證' }}</span>
            </div>
          </div>
        </div>

        <div class="divider"></div>

        <dl class="kv">
          <div class="kv__row">
            <dt>UserID</dt>
            <dd>{{ selectedUser.userId ?? '未提供' }}</dd>
          </div>
          <div class="kv__row">
            <dt>信箱</dt>
            <dd>
              <template v-if="selectedUser.email">
                <a :href="`mailto:${selectedUser.email}`">{{ selectedUser.email }}</a>
              </template>
              <template v-else>未提供</template>
            </dd>
          </div>
          <div class="kv__row">
            <dt>電話</dt>
            <dd>
              <template v-if="selectedUser.phone">
                <a :href="`tel:${selectedUser.phone}`">{{ selectedUser.phone }}</a>
              </template>
              <template v-else>未提供</template>
            </dd>
          </div>
          <div class="kv__row">
            <dt>地址</dt>
            <dd>{{ selectedUser.address || '未提供' }}</dd>
          </div>
        </dl>
      </div>

      <div v-else-if="loadingRight" class="profile-card card">
        <div class="skeleton avatar-skel"></div>
        <div class="skeleton line"></div>
        <div class="skeleton line short"></div>
        <div class="divider"></div>
        <div class="skeleton line"></div>
        <div class="skeleton line"></div>
        <div class="skeleton line"></div>
      </div>

      <div v-else class="profile-card card">
        尚未選擇會話或無法取得客戶資料。
      </div>
    </aside>
  </div>
</template>

<style scoped>
/* ========== 基礎設計系統 ========== */
:root {
  --bg: #f5f7fb;
  --panel: #ffffff;
  --text: #111827;
  --muted: #6b7280;
  --border: #e5e7eb;
  --primary: #2563eb;
  --primary-weak: #eaf0ff;
  --success: #16a34a;
  --warning: #f59e0b;
  --danger: #ef4444;
  --shadow: 0 6px 20px rgba(17,24,39,.08);
  --radius: 14px;
}

/* Layout */
.agent-dashboard {
  display: grid;
  grid-template-columns: 320px 1fr 360px;
  height: 100vh;
  background: var(--bg);
  gap: 16px;
  padding: 16px;
  box-sizing: border-box;
}

/* Cards */
.card {
  background: var(--panel);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow);
}

/* Sidebar */
.sidebar {
  background: var(--panel);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow);
  padding: 14px;
  overflow: hidden;
  display: flex;
  flex-direction: column;
  min-width: 0;
}
.sidebar__header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 10px;
}
.sidebar h3 { margin: 0; font-size: 16px; }
.pill {
  background: var(--primary-weak);
  color: var(--primary);
  border-radius: 999px;
  padding: 2px 10px;
  font-size: 12px;
  font-weight: 600;
}

.ticket {
  padding: 14px 16px;
  border: 1.5px solid #dce3f5;          /* 💡亮一點的淺藍邊 */
  border-radius: 14px;
  background: #ffffff;
  transition: all .18s ease;
  display: grid;
  gap: 8px;
  margin-bottom: 14px;                  /* 💡上下距離加大 */
}
.ticket:last-child { margin-bottom: 0; }

.ticket:hover {
  border-color: #a5b4fc;                /* hover 時邊框更亮藍 */
  box-shadow: 0 6px 18px rgba(37, 99, 235, 0.12); /* 淡淡陰影 */
  transform: translateY(-2px);
}

.ticket.active {
  border-color: #2563eb;                /* 主色藍邊 */
  box-shadow: 0 0 0 3px rgba(37, 99, 235, 0.18);
  background: #f9fbff;                  /* 略帶藍底 */
}

.ticket__row1 {
  display: grid;
  grid-template-columns: 1fr auto;
  align-items: start;
  gap: 8px;
}
.ticket__title {
  margin: 0;
  font-size: 15px;
  font-weight: 700;
  color: #0f172a;
  line-height: 1.3;
  display: -webkit-box;
  -webkit-line-clamp: 2;     /* 最多兩行，超出省略 */
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.ticket__row2 {
  display: grid;
  grid-template-columns: 1fr auto;
  align-items: center;
  color: #475569;
}
.ticket__name {
  font-size: 13px;
  font-weight: 600;
  white-space: nowrap; overflow: hidden; text-overflow: ellipsis;
}
.ticket__time {
  font-size: 12px;
  opacity: .85;
  white-space: nowrap;
}

.badge {
  font-size: 10.5px;
  padding: 2px 8px;
  border-radius: 999px;
  border: 1px solid var(--border);
  background: #f8fafc;
  color: #334155;
  height: fit-content;
}
.badge[data-variant="open"],
.badge[data-variant="Open"] { background:#ecfdf5; color:#065f46; border-color:#a7f3d0; }
.badge[data-variant="pending"] { background:#fff7ed; color:#9a3412; border-color:#fed7aa; }
.badge[data-variant="closed"] { background:#f1f5f9; color:#475569; border-color:#e2e8f0; }

/* Chat area */
.chat-area { display:flex; flex-direction:column; min-width:0; }
.chat-header {
  position: sticky; top: 0; z-index: 5;
  display:flex; align-items:center; justify-content:space-between;
  padding: 12px 16px; margin-bottom: 12px;
  background: var(--panel);
  border: 1px solid var(--border);
  border-radius: var(--radius);
  box-shadow: var(--shadow);
}
.chat-header__title h3 { margin: 0; font-size: 18px; line-height: 1.2; }
.chat-header .sub { color: var(--muted); font-size: 12px; }
.chat-header__right { color: var(--muted); font-size: 12px; display:flex; align-items:center; gap:6px; }
.status-dot { width:8px; height:8px; border-radius:50%; display:inline-block; }
.status-dot.online { background: var(--success); }

.chat-body {
  flex: 1;
  padding: 12px;
  overflow: auto;
}

/* Profile */
.profile { min-width:0; }
.profile-card { padding: 16px; }
.profile-card__head { display:flex; align-items:center; gap:12px; }
.avatar { width:72px; height:72px; border-radius:50%; object-fit:cover; border: 3px solid #eef2ff; }
.head-text .name { font-weight: 800; font-size: 18px; }
.tagline { margin-top: 2px; }

.chip {
  font-size: 12px; padding: 2px 8px; border-radius: 999px; border: 1px solid var(--border);
  background:#f1f5f9; color:#475569;
}
.chip[data-ok="true"] { background:#ecfdf5; color:#047857; border-color:#a7f3d0; }
.chip[data-ok="false"] { background:#fff1f2; color:#9f1239; border-color:#fecdd3; }

.divider { height:1px; background: var(--border); margin: 12px 0; }

.kv { display:grid; gap:8px; }
.kv__row { display:grid; grid-template-columns: 90px 1fr; align-items:center; }
.kv dt { color: var(--muted); font-size: 12px; }
.kv dd { margin: 0; font-size: 14px; }
.kv a { color: var(--primary); text-decoration: none; }
.kv a:hover { text-decoration: underline; }

/* Empty state */
.chat-empty { display:flex; align-items:center; justify-content:center; }
.empty { padding: 28px; text-align: center; }
.empty__title { font-size: 16px; font-weight: 700; }
.empty__sub { color: var(--muted); margin-top: 4px; }

/* Utilities */
.ellipsis { white-space: nowrap; overflow: hidden; text-overflow: ellipsis; }

/* Skeletons */
.skeleton-list { display:grid; gap:8px; }
.skeleton-item { height:64px; border-radius:12px; background: linear-gradient(90deg,#f3f4f6, #e5e7eb, #f3f4f6); background-size: 200% 100%; animation: sk 1.2s infinite; }
.skeleton { background: linear-gradient(90deg,#f3f4f6, #e5e7eb, #f3f4f6); background-size: 200% 100%; animation: sk 1.2s infinite; border-radius: 8px; }
.avatar-skel { width:72px; height:72px; border-radius:50%; }
.line { height:14px; margin-top:10px; }
.line.short { width:60%; }
@keyframes sk { 0%{background-position:200% 0} 100%{background-position:-200% 0} }

/* 響應式 */
@media (max-width: 1100px) {
  .agent-dashboard { grid-template-columns: 280px 1fr; }
  .profile { display: none; }
}
@media (max-width: 720px) {
  .agent-dashboard { grid-template-columns: 1fr; padding: 8px; gap: 8px; }
  .sidebar { order: 2; }
  .chat-area, .chat-empty { order: 1; }
}
</style>
