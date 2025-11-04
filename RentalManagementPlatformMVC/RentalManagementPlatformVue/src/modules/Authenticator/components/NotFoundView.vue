<template>
  <div class="empty-state">
    <h1>找不到頁面</h1>
    <p>您要前往的頁面不存在，或是沒有存取權限。</p>
    <RouterLink class="primary-button" to="/">回到儀表板</RouterLink>
  </div>
</template>

<script setup lang="ts">
import { RouterLink } from 'vue-router'
</script>

<style scoped>
.empty-state {
  min-height: 60vh;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  text-align: center;
  gap: 16px;
}

.empty-state h1 {
  font-size: 2rem;
  margin: 0;
  color: #111827;
}

.empty-state p {
  margin: 0;
  color: #6b7280;
}

.primary-button {
  display: inline-block;
  padding: 12px 20px;
  border-radius: 999px;
  background: linear-gradient(135deg, #2563eb, #7c3aed);
  color: white;
  font-weight: 600;
  text-decoration: none;
}

.primary-button:hover {
  filter: brightness(1.05);
}
</style>