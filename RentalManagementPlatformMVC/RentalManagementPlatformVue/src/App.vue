<template>
    <div class="app-shell">
        <AppHeader />
        <main class="main-content">
        <RouterView />
        </main>
  </div>
</template>
    
<script setup lang="ts">
import { RouterView } from 'vue-router'
import AppHeader from '@/components/AppHeader.vue'
import '@/assets/main.css'
</script>