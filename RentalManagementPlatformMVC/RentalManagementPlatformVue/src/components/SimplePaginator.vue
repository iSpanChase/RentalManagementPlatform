<template>
  <div class="paginator" v-if="totalPages > 1">
    <button @click="changePage(1)" :disabled="currentPage === 1">&lt;&lt;</button>
    <button @click="changePage(currentPage - 1)" :disabled="currentPage === 1">&lt;</button>

    <span v-if="showStartEllipsis">...</span>

    <button
      v-for="page in visiblePages"
      :key="page"
      @click="changePage(page)"
      :class="{ active: currentPage === page }"
    >
      {{ page }}
    </button>

    <span v-if="showEndEllipsis">...</span>

    <button @click="changePage(currentPage + 1)" :disabled="currentPage === totalPages">&gt;</button>
    <button @click="changePage(totalPages)" :disabled="currentPage === totalPages">&gt;&gt;</button>
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';

const props = defineProps({
  currentPage: {
    type: Number,
    required: true,
  },
  totalPages: {
    type: Number,
    required: true,
  },
  maxVisiblePages: {
    type: Number,
    default: 7, // 3 before, current, 3 after
  },
});

const emit = defineEmits(['page-changed']);

const changePage = (page: number) => {
  if (page >= 1 && page <= props.totalPages) {
    emit('page-changed', page);
  }
};

const visiblePages = computed(() => {
  if (props.totalPages <= props.maxVisiblePages) {
    return Array.from({ length: props.totalPages }, (_, i) => i + 1);
  }

  const half = Math.floor(props.maxVisiblePages / 2);
  let start = Math.max(1, props.currentPage - half);
  let end = Math.min(props.totalPages, props.currentPage + half);

  if (props.currentPage - half < 1) {
    end = props.maxVisiblePages;
  }

  if (props.currentPage + half > props.totalPages) {
    start = props.totalPages - props.maxVisiblePages + 1;
  }

  const pages = [];
  for (let i = start; i <= end; i++) {
    pages.push(i);
  }
  return pages;
});

const showStartEllipsis = computed(() => {
    const firstVisible = visiblePages.value[0];
    return firstVisible !== undefined && firstVisible > 1;
});

const showEndEllipsis = computed(() => {
    const lastVisible = visiblePages.value[visiblePages.value.length - 1];
    return lastVisible !== undefined && lastVisible < props.totalPages;
});

</script>

<style scoped>
.paginator {
  display: flex;
  justify-content: center;
  align-items: center;
  margin-top: 20px;
}
.paginator button {
  margin: 0 5px;
  padding: 5px 10px;
  border: 1px solid #ccc;
  background-color: #fff;
  cursor: pointer;
}
.paginator button.active {
  background-color: #007bff;
  color: #fff;
  border-color: #007bff;
}
.paginator button:disabled {
  cursor: not-allowed;
  opacity: 0.5;
}
.paginator span {
  margin: 0 5px;
}
</style>
