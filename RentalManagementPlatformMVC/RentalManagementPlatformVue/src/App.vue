<template>
  <div>
    <nav>
      <router-link to="/">首頁</router-link> |
      <router-link to="/about">關於</router-link>
      <router-link :to="{ name: 'ReportForm.IndexView' }">ReportForm</router-link>
    </nav>
    <router-view />
  </div>
</template>

<script setup>
import { onMounted } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
onMounted(() => {
  console.log('All route names:', router.getRoutes().map(r => r.name))
})
</script>