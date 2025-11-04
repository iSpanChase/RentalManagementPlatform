<template>
  <button 
    :disabled="disabled || loading" 
    :class="buttonClass"
    @click="$emit('click')"
  >
    <span v-if="loading" class="spinner-border spinner-border-sm me-2" role="status" aria-hidden="true"></span>
    <span>{{ text }}</span>
  </button>
</template>

<script setup>
import { computed } from 'vue';

const props = defineProps({
  text: { type: String, required: true },
  disabled: { type: Boolean, default: false },
  loading: { type: Boolean, default: false },
  customClass: { type: String, default: '' },
});

defineEmits(['click']);

const buttonClass = computed(() => {
  return ['btn', props.customClass || 'btn-success'];
});
</script>
