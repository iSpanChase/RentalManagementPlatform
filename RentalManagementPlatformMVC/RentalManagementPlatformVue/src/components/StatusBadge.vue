<script setup>
import { computed } from 'vue';
import { getStatusText, getStatusClass } from '@/composables/useOrderStatus';

const props = defineProps({
  status: {
    type: String,
    required: true
  }
});

const statusText = computed(() => getStatusText(props.status));
const statusClass = computed(() => getStatusClass(props.status));
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

  &.status-pending {
    background-color: #cce5ff;
    color: #004085;
  }

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

  &.status-failed {
    background-color: #f8d7da;
    color: #721c24;
  }

  &.status-refunded {
    background-color: #e2e3e5;
    color: #383d41;
  }
}
</style>
