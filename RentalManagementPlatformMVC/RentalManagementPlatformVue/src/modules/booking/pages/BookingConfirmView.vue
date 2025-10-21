<script setup>
import { ref } from 'vue'
const message = ref('Hello')
</script>

<template>
  <div>{{ message }}</div>
</template>

<style lang="scss" scoped>
div { color: blue; }
</style>
