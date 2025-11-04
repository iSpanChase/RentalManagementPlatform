<template>
  <div class="pagination">
    <button :disabled="page <= 1" @click="changePage(page - 1)">上一頁</button>
    <span>第 {{ page }} / {{ totalPages }} 頁</span>
    <button :disabled="page >= totalPages" @click="changePage(page + 1)">下一頁</button>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  totalItems: Number,
  itemsPerPage: { type: Number, default: 6 },
  modelValue: { type: Number, default: 1 },
})

const emit = defineEmits(['update:modelValue'])

const page = computed({
  get: () => props.modelValue,
  set: (val) => emit('update:modelValue', val),
})

const totalPages = computed(() => Math.ceil(props.totalItems / props.itemsPerPage))

const changePage = (newPage) => {
  if (newPage >= 1 && newPage <= totalPages.value) {
    page.value = newPage
  }
}
</script>

<style scoped>
.pagination {
  display: flex;
  justify-content: center;
  align-items: center;
  gap: 8px;
  margin-top: 1rem;
}
button {
  padding: 6px 12px;
  border-radius: 4px;
  border: 1px solid #28a745;
  background-color: white;
  color: #28a745;
}
button:disabled {
  border-color: #ccc;
  color: #ccc;
}
</style>
