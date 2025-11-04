<script setup>
const props = defineProps({
  text: {
    type: String,
    default: '載入中...'
  },
  size: {
    type: String,
    default: 'medium', // small, medium, large
    validator: (value) => ['small', 'medium', 'large'].includes(value)
  },
  minHeight: {
    type: String,
    default: '200px'
  }
});

const sizeClasses = {
  small: 'spinner-sm',
  medium: 'spinner-md',
  large: 'spinner-lg'
};
</script>

<template>
  <div class="loading-spinner" :style="{ minHeight }">
    <div :class="['spinner-border', 'text-primary', sizeClasses[size]]" role="status">
      <span class="visually-hidden">{{ text }}</span>
    </div>
    <p v-if="text" class="loading-text">{{ text }}</p>
  </div>
</template>

<style lang="scss" scoped>
.loading-spinner {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  gap: 12px;

  .spinner-sm {
    width: 1.5rem;
    height: 1.5rem;
  }

  .spinner-md {
    width: 2rem;
    height: 2rem;
  }

  .spinner-lg {
    width: 3rem;
    height: 3rem;
  }

  .loading-text {
    margin: 0;
    color: #717171;
    font-size: 14px;
  }
}
</style>