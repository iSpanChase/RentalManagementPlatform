<template>
  <div class="wrap">
    <div class="card">
      <h1>電子郵件驗證成功 ✔</h1>
      <p>您的 Email 已完成驗證，部分功能已全面開放。</p>

      <button class="primary" @click="goProfile">前往更新個人資料</button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'
import { onMounted } from 'vue'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const auth = useAuthStore()

onMounted(async () => {
  // 驗證成功後這裡再拉一次個資，讓 isVerified 立即更新
  await auth.fetchProfile?.()
})

const goProfile = () => {
  router.replace({ name: 'profile' }) // 你的路由名稱若不同，改成實際的
}
</script>

<style scoped>
.wrap { display:flex; align-items:center; justify-content:center; min-height:60vh; padding:24px; }
.card { text-align:center; background:#fff; border-radius:16px; padding:32px 28px; box-shadow:0 12px 30px rgba(0,0,0,.08); }
h1 { margin:0 0 8px; font-size:1.6rem; }
p { margin:0 0 16px; color:#6b7280; }
.primary { border:none; padding:10px 18px; border-radius:999px; background:linear-gradient(135deg,#2563eb,#7c3aed); color:#fff; font-weight:600; cursor:pointer; }
.primary:hover { filter:brightness(1.05); }
</style>