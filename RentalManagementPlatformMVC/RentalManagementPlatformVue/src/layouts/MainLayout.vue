<script setup>
import MainHeader from '@/components/layout/MainHeader.vue';
import MainFooter from '@/components/layout/MainFooter.vue';
</script>

<template>
  <div class="main-layout">
    <!-- 1. Header -->
    <MainHeader />

    <!-- 4. Main Content -->
    <main class="main-content">
      <RouterView />
    </main>

    <!-- 5. Footer -->
    <MainFooter />
  </div>
</template>

<style scoped>
.main-layout {
  min-height: 100vh;
  display: flex;
  flex-direction: column;
  background: #f8f9fa;
}

.main-content {
  flex: 1;
  padding-top: 70px; /* 預留header高度 */
}
</style>
