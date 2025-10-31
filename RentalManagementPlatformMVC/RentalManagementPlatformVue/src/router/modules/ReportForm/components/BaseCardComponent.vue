
<template>
  <div class="col-12 col-md-6 col-xl-4 py-3">
    <div class="card shadow-sm h-100">
      <div class="card-header d-flex justify-content-between align-items-center bg-light border-0">
        <div class="d-flex flex-column">
          <h5 class="mb-0 fw-semibold">{{ title }}</h5>
          <small v-if="subtitle" class="text-muted">{{ subtitle }}</small>
        </div>
        <div class="d-flex align-items-center gap-2">
          <slot name="header-extra"></slot>
          <button v-if="!loading" class="btn btn-sm btn-outline-primary" @click="$emit('edit')">修改</button>
          <button v-if="!loading" class="btn btn-sm btn-outline-danger" @click="$emit('remove')">刪除</button>
        </div>
      </div>

      <div v-if="loading" class="p-4 text-center text-muted">載入中...</div>
      <div v-else class="card-body">
        <slot></slot>
      </div>
      <div class="card-footer bg-white border-0">
        <slot name="footer"></slot>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
defineProps<{
  title: string
  subtitle?: string
  loading?: boolean
}>()
defineEmits(['edit','remove'])
</script>
