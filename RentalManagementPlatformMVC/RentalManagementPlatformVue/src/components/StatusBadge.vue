<script setup>
import { getStatusText, getStatusClass } from '@/composables/useOrderStatus';

const props = defineProps({
  status: {
    type: String,
    required: true
  }
});

const statusText = getStatusText(props.status);
const statusClass = getStatusClass(props.status);
</script>

<template>
  <span :class="['status-badge', statusClass]">
    {{ statusText }}
  </span>
</template>

<style lang="scss" scoped>
.status-badge {
  padding: 5px 12px;
  border-radius: 20px;
  font-size: 12px;
  font-weight: 600;
  display: inline-block;
  white-space: nowrap;

  &.status-deferred,
  &.status-unpaid {
    background-color: #fff3cd;
    color: #856404;
  }

  &.status-completed,
  &.status-paid {
    background-color: #d4edda;
    color: #155724;
  }

  &.status-cancelled {
    background-color: #f8d7da;
    color: #721c24;
  }

  &.status-refunded {
    background-color: #e2e3e5;
    color: #383d41;
  }
}
</style>