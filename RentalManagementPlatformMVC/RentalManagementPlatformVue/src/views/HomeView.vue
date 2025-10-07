<template>
  <div>
    <h1>首頁</h1>
    <button @click="loadData">載入資料</button>
    <pre>{{ data }}</pre>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const data = ref(null)

async function loadData() {
  try {
    const response = await fetch('/api/WeatherForecast') // 如果設有 proxy，可直接用相對路徑
    if (!response.ok) {
      throw new Error(`HTTP 錯誤！狀態碼：${response.status}`)
    }
    data.value = await response.json()
  } catch (err) {
    console.error('載入資料時發生錯誤：', err)
  }
}
</script>