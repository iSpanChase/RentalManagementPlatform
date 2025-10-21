<script setup>
import { ref } from 'vue';
const noticeText = ref('Your booking has been successfully cancelled.');
</script>

<template>
  <div class="cancellation-notice">
    <p>{{ noticeText }}</p>
  </div>
</template>

<style lang="scss" scoped>

</style>
