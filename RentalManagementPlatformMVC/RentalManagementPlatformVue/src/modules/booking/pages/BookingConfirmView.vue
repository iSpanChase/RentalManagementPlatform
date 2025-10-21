<script setup>
import { ref } from 'vue'
import BookingSummaryCard from '../components/BookingSummaryCard.vue';
const text = ref('Hello from BookingConfirmView.vue')
</script>

<template>
  <div>
    <h3>{{ text }}</h3>
  </div>
  <BookingSummaryCard />
</template>

<style lang="scss" scoped>
div { color: blue; }
</style>
