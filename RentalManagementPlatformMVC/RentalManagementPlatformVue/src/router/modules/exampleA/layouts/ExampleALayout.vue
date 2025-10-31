<!-- src/modules/exampleA/layouts/ExampleALayout.vue -->
<template>
  <div class="example-a-layout">
    <h2>ExampleALayout</h2>
    <!-- 若要放共用 nav / sidebar 可放在這 -->
    <router-view />
  </div>
</template>

<script setup>
// optional script
</script>

<style scoped>
.example-a-layout { padding: 12px; }
</style>