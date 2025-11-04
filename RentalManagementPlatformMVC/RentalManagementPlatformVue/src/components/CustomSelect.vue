<script setup lang="ts">
import { ref, computed } from 'vue';
import { onClickOutside } from '@vueuse/core';

interface Option {
  value: string | number;
  label: string;
}

const props = defineProps({
  modelValue: { type: [String, Number], default: '' },
  options: { type: Array as () => Option[], required: true },
  placeholder: { type: String, default: 'Select an option' },
});

const emit = defineEmits(['update:modelValue']);

const isOpen = ref(false);
const selectRef = ref<HTMLElement | null>(null);

onClickOutside(selectRef, () => {
  isOpen.value = false;
});

const selectedOption = computed(() => {
  return props.options.find(option => option.value === props.modelValue) || null;
});

const selectOption = (option: Option) => {
  emit('update:modelValue', option.value);
  isOpen.value = false;
};

const toggleDropdown = () => {
  isOpen.value = !isOpen.value;
};
</script>

<template>
  <div class="custom-select-wrapper" ref="selectRef">
    <div class="custom-select-trigger" @click="toggleDropdown">
      <span>{{ selectedOption ? selectedOption.label : placeholder }}</span>
      <div class="arrow" :class="{ 'open': isOpen }"></div>
    </div>
    <div class="custom-options" :class="{ 'open': isOpen }">
      <div
        class="custom-option"
        v-for="option in options"
        :key="option.value"
        @click="selectOption(option)"
        :class="{ 'selected': option.value === modelValue }"
      >
        {{ option.label }}
      </div>
    </div>
  </div>
</template>

<style scoped>
/* Basic styling for the custom select box */
.custom-select-wrapper {
  position: relative;
  width: 100%; /* Adjust as needed */
  cursor: pointer;
  font-family: inherit;
  font-size: inherit;
}

.custom-select-trigger {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 15px;
  border: 1px solid #ccc;
  border-radius: 4px;
  background-color: #fff;
}

.custom-select-trigger .arrow {
  width: 0;
  height: 0;
  border-left: 5px solid transparent;
  border-right: 5px solid transparent;
  border-top: 5px solid #333;
  transition: transform 0.2s;
}

.custom-select-trigger .arrow.open {
  transform: rotate(180deg);
}

.custom-options {
  position: absolute;
  top: 100%;
  left: 0;
  right: 0;
  border: 1px solid #ccc;
  border-top: none;
  border-radius: 0 0 4px 4px;
  background-color: #fff;
  z-index: 100;
  max-height: 200px;
  overflow-y: auto;
  display: none; /* Hidden by default */
}

.custom-options.open {
  display: block; /* Show when open */
}

.custom-option {
  padding: 10px 15px;
  cursor: pointer;
}

.custom-option:hover,
.custom-option.selected {
  background-color: #f0f0f0;
}
</style>
