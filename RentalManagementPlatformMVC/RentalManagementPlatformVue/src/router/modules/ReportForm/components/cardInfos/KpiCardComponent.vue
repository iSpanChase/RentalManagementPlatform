<template>
  <div class="kpi-card text-center">
    <div class="kpi-value display-4 fw-bold">
      {{ formattedRate }}
    </div>
    <div class="kpi-label text-muted">
      入住率
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{
  data: { occupancyRate: number };
}>();

const formattedRate = computed(() => {
  const rate = props.data?.occupancyRate ?? 0;
  return `${rate.toFixed(1)}%`;
});
</script>

<style scoped>
.kpi-card {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  height: 100%;
  min-height: 200px; /* Ensure a minimum height */
}

.kpi-value {
  font-size: 3.5rem;
  color: #343a40;
}

.kpi-label {
  font-size: 1.2rem;
}
</style>
