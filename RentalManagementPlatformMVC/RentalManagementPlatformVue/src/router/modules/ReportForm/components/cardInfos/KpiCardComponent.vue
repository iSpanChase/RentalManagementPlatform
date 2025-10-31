<template>
  <div class="kpi-card text-center">
    <div class="kpi-value display-4 fw-bold">
      {{ formattedValue }}
    </div>
    <div class="kpi-label text-muted">
      {{ label }}
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps<{
  data: Record<string, number>;
  dataKey: string;
  label: string;
  unit: 'percentage' | 'currency';
}>();

const formattedValue = computed(() => {
  const value = props.data?.[props.dataKey] ?? 0;
  if (props.unit === 'percentage') {
    return `${value.toFixed(1)}%`;
  }
  if (props.unit === 'currency') {
    return new Intl.NumberFormat('zh-TW', { style: 'currency', currency: 'TWD', minimumFractionDigits: 0, maximumFractionDigits: 0 }).format(value);
  }
  return value;
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
