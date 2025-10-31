<!-- src/modules/exampleB/layouts/ExampleBLayout.vue -->
<template>
  <div class="example-b-layout">
    <h2>ExampleBLayout</h2>
    <!-- 若要放共用 nav / sidebar 可放在這 -->
    <router-view />
  </div>
</template>

<script setup>
// optional script
</script>

<style scoped>
.example-b-layout { padding: 12px; }
</style>