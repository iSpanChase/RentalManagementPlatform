<!-- 小巧元件，顯示「限時」、「熱門」、「即將到期」等角標 -->
<template>
  <span
    v-if="badge"
    class="px-2 py-1 rounded-full text-xs font-semibold text-white"
    :class="badgeColor"
  >
    {{ badgeText }}
  </span>
</template>

<script setup lang="ts">
import { computed } from 'vue'

const props = defineProps<{ badge?: string }>()

const badgeColor = computed(() => {
  switch (props.badge) {
    case '限時':
      return 'bg-red-500'
    case '熱門':
      return 'bg-orange-500'
    case '即將到期':
      return 'bg-yellow-500'
    default:
      return 'bg-gray-400'
  }
})

const badgeText = computed(() => props.badge || '')
</script>
