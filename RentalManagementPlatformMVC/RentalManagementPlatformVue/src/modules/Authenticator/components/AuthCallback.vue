<template>
  <div class="container py-5 text-center">
    <h2 class="mb-2">正在完成登入…</h2>
    <p v-if="msg">{{ msg }}</p>
  </div>
</template>

<script setup lang="ts">
import { onMounted, ref } from 'vue'

const msg = ref('請稍候')

function q(name: string) {
  const v = new URLSearchParams(window.location.search).get(name)
  return v ? v.trim() : ''
}

onMounted(async () => {
  // 後端 finish 會帶 access / redirect（仍相容舊名）
  const access   = q('access')   || q('token')
  const refresh  = q('refresh')  || ''
  const redirect = q('redirect') || q('returnUrl') || '/'

  const error = q('error')
  if (error && !access) {
    msg.value = `授權失敗：${decodeURIComponent(error)}`
    setTimeout(() => window.location.replace(`/auth/login?redirect=${encodeURIComponent(redirect)}`), 900)
    return
  }

  if (!access) {
    // 沒拿到 token；若本機有舊 token 就直接回原頁，否則回登入
    if (localStorage.getItem('rmp.accessToken')) {
      setTimeout(() => window.location.replace(redirect || '/'), 100)
      return
    }
    msg.value = '未取得授權資訊，即將返回登入頁…'
    setTimeout(() => window.location.replace(`/auth/login?redirect=${encodeURIComponent(redirect)}`), 900)
    return
  }

  // ✅ 寫入你的專案鍵名
  localStorage.setItem('rmp.accessToken', access)
  if (refresh) localStorage.setItem('rmp.refreshToken', refresh)

  // （可選）給後續畫面一點時間載入，再跳
  setTimeout(() => {
    // 直接用原生導頁，避免任何守門再把你導回 callback
    window.location.replace(redirect || '/')
  }, 300)
})
</script>

<style scoped>
.container { max-width: 520px; }
</style>
