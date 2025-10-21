<script setup>
import { ref } from 'vue';
import BookingConfirmView from '../pages/BookingConfirmView.vue';
</script>

<!-- src/modules/reportForm/layouts/ReportFormLayout.vue -->
<template>
  <div class="booking-layout">
    <h2>First Booking</h2>
    <BookingConfirmView />
    <router-view />
  </div>
</template>

<style scoped>
.booking-layout { padding: 12px; }
</style>
