<template>
  <div class="filter-buttons">
    <button
      v-for="option in options"
      :key="option.value"
      :class="{ active: selected === option.value }"
      @click="setFilter(option.value)"
    >
      {{ option.label }}
    </button>
  </div>
</template>

<script setup>
import { ref } from 'vue'

const props = defineProps({
  modelValue: { type: String, default: 'all' },
})

const emit = defineEmits(['update:modelValue'])

const selected = ref(props.modelValue)

const options = [
  { label: '全部', value: 'all' },
  { label: '未使用', value: 'unused' },
  { label: '已使用', value: 'used' },
  { label: '已過期', value: 'expired' },
]

const setFilter = (value) => {
  selected.value = value
  emit('update:modelValue', value)
}
</script>

<style scoped>
.filter-buttons {
  display: flex;
  gap: 10px;
  justify-content: center;
  margin-bottom: 10px;
}

button {
  border: 1px solid #28a745;
  color: #28a745;
  background-color: white;
  border-radius: 20px;
  padding: 6px 14px;
  font-size: 0.9em;
  cursor: pointer;
  transition: 0.2s;
}

button.active {
  background-color: #28a745;
  color: white;
}
button:hover {
  background-color: #28a745;
  color: white;
}
</style>
