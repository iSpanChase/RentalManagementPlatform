<!-- src/modules/reportForm/layouts/ReportFormLayout.vue -->
<template>
  <div class="reportform-layout">
    <h2>ReportFormLayout</h2>
    <!-- 若要放共用 nav / sidebar 可放在這 -->
    <router-view />
  </div>
</template>

<script setup>
// optional script
</script>

<style scoped>
.reportform-layout { padding: 12px; }
</style>