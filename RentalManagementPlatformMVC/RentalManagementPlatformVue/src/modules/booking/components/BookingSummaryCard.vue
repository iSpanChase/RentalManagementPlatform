<script setup>
import { ref } from 'vue';
import RoomInfo from './RoomInfoSection.vue';
import DateInfo from './DateInfoSection.vue';
import GuestInfo from './GuestInfoSection.vue';
import PriceInfo from './PriceInfoSection.vue';
import CancellationNotice from './CancellationNotice.vue';
</script>

<template>
  <BaseTransition></BaseTransition>
    <div class="booking-summary-card">
      <RoomInfo></RoomInfo>
      <DateInfo></DateInfo>
      <GuestInfo></GuestInfo>
      <PriceInfo></PriceInfo>
      <CancellationNotice></CancellationNotice>
    </div>
</template>

<style lang="scss" scoped>
.booking-summary-card {
  border: 1px solid #e0e0e0;
  border-radius: 8px;
  padding: 16px;
  background-color: #fff;
}
</style>
