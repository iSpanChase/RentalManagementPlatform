<template>
<main class="forbidden">
<div class="card">
<div class="icon" aria-hidden="true">🚫</div>
<h1>403 禁止存取</h1>
<p class="hint">
你沒有存取此頁面的權限。
<span v-if="requiredPerms && requiredPerms.length">
需要的權限：
<code>{{ requiredPerms.join(', ') }}</code>
</span>
</p>


<div class="actions">
<button class="btn" type="button" @click="goBack">返回上一頁</button>
<button class="btn primary" type="button" @click="goHome">回到首頁</button>
<button class="btn outline" type="button" @click="toProfile">查看個人資料</button>
</div>


<details class="help">
<summary>需要協助？</summary>
<ul>
<li>請確認你已登入正確的帳號。</li>
<li>若你應該擁有此權限，請聯絡系統管理員調整角色/權限。</li>
<li>重新整理或重新登入後再試一次。</li>
</ul>
</details>
</div>
</main>
</template>

<script setup lang="ts">
import { useRoute, useRouter } from 'vue-router'


const route = useRoute()
const router = useRouter()


// 從路由 meta 讀取此頁需要的權限（若有）
const requiredPerms = (route.meta.requiredPerms as string[] | undefined) || []


const goHome = () => router.push({ path: '/' })
const toProfile = () => router.push({ path: '/profile' })
const goBack = () => {
if (window.history.length > 1) {
router.back()
} else {
goHome()
}
}
</script>

<style scoped>
.forbidden {
min-height: 70vh;
display: grid;
place-items: center;
padding: 24px;
background: var(--bg, #f6f7f9);
}
.card {
width: min(640px, 92vw);
background: #fff;
border: 1px solid #e5e7eb;
border-radius: 16px;
padding: 28px 24px;
box-shadow: 0 4px 24px rgba(0,0,0,0.06);
text-align: center;
}
.icon {
font-size: 48px;
line-height: 1;
margin-bottom: 8px;
}
h1 {
margin: 0 0 8px;
font-size: 24px;
}
.hint {
margin: 0 auto 20px;
color: #6b7280;
}
.hint code {
background: #f3f4f6;
padding: 2px 6px;
border-radius: 6px;
font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, "Liberation Mono", "Courier New", monospace;
}
.actions {
display: flex;
gap: 12px;
justify-content: center;
flex-wrap: wrap;
margin-bottom: 12px;
}
.btn {
appearance: none;
border: 1px solid #d1d5db;
background: #fff;
color: #111827;
border-radius: 10px;
padding: 10px 14px;
cursor: pointer;
transition: all .15s ease-in-out;
}
.btn:hover { box-shadow: 0 2px 10px rgba(0,0,0,.06); transform: translateY(-1px); }
.btn.primary { background: #1f6feb; color: #fff; border-color: #1f6feb; }
.btn.primary:hover { filter: brightness(1.05); }
.btn.outline { background: transparent; }
.help { text-align: left; margin-top: 8px; }
.help summary { cursor: pointer; color: #374151; }
</style>